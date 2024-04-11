//#define DEBUG_BOOT_MODE 0x01
//#define DEBUG_BOOT_MODE 0x20
#define DEBUG_BOOT_MODE 0x2A
// ROM Block Number
#define ROM_BLOCK_USER 16   // Range of user available space from this block number to ROM_BLOCK_NUM - 2

#include "PCAB_Debugger_FW.hpp"
#define SNPRINTF_BUFFER_LEN 50
#define NUMBER_OF_SYSTEM 15
#define ROM_BLOCK_NUM PICO_FLASH_SIZE_BYTES / 65536
const static std::string FW_VENDOR = "Orient Microwave Corp.";
const static std::string FW_MODEL = "LX00-0004-00";
const static std::string FW_REV = "1.1.2";


ds18b20 *sens;
adc *analog;
spi *spi_ps;
spi *spi_sa;
pcabCMD *uart;
bool modeCUI = true;
bool modeECHO = false;
std::string serialNum = "";
uint8_t bootMode = 0;
uint8_t dpsBF[NUMBER_OF_SYSTEM];
uint8_t dpsNOW[NUMBER_OF_SYSTEM];
uint8_t dsaBF[NUMBER_OF_SYSTEM + 1];
uint8_t dsaNOW[NUMBER_OF_SYSTEM + 1];
bool stbAMP = false;
bool stbDRA = false;
bool stbLNA = false;
bool lowMODE = false;

#pragma region Private Function

bool editRangeCheck(const uint16_t &blockNum)
{
    if(blockNum < ROM_BLOCK_USER || ROM_BLOCK_NUM - 1 < blockNum) { return false; }
    if(bootMode == 0x2A) { return true; }
    if((bootMode == 0x02 || bootMode == 0x03 || bootMode == 0x06 || bootMode == 0x07) && (blockNum == ROM_BLOCK_NUM - 2)) { return true; }
    if((bootMode == 0x04 || bootMode == 0x05 || bootMode == 0x06 || bootMode == 0x07) && (ROM_BLOCK_USER <= blockNum && blockNum < ROM_BLOCK_NUM - 3)) { return true; }
    return false;
}

void writeROM(const uint16_t &blockNum, const uint8_t blockDAT[FLASH_PAGE_SIZE])
{
    if(bootMode == 0x2A || bootMode == 0x04 || bootMode == 0x05 || bootMode == 0x06 || bootMode == 0x07)
    {
        if(editRangeCheck(blockNum))
        {
            writeROMblock(blockAddress(blockNum), blockDAT);
            uart->uart.writeLine("DONE > Write ROM block " + Convert::ToString(blockNum, 10, 1) + ".");
        } else { uart->uart.writeLine("ERR > Specified block is out of range."); }
    } else { uart->uart.writeLine("ERR > It is in an unusable state."); }
}

void writeROM(const std::string &num, const std::string &data)
{
    uint16_t blockNum;
    if(!Convert::TryToUInt16(num, 10, blockNum) || data.size() != 2 * FLASH_PAGE_SIZE)
    { uart->uart.writeLine("ERR > Argument1 error."); }
    else
    {
        uint8_t blockDAT[FLASH_PAGE_SIZE];
        for( uint i = 0 ; i < FLASH_PAGE_SIZE ; i++ )
        {
            if(!Convert::TryToUInt8(data.substr(2 * i, 2) , 16, blockDAT[i]))
            {
                uart->uart.writeLine("ERR > Argument2 error.");
                return;
            }
        }
        writeROM(blockNum, blockDAT);
    }
}

std::string readSerialNum()
{
    uint8_t romBF[FLASH_PAGE_SIZE];
    readROMblock(blockAddress(ROM_BLOCK_NUM - 1), romBF);
    // Read SERIAL NUMBER
    std::string serial = "";
    uint8_t len = romBF[FLASH_PAGE_SIZE - 1];
    if(len > 16 || len == 0) { return ""; }
    for(uint i = FLASH_PAGE_SIZE - 16; i < FLASH_PAGE_SIZE - 16 + len ; i ++)
    { if(isgraph(romBF[i])) { serial.push_back(romBF[i]); } }
    if(serial.size() == len) { return serial; }
    else { return ""; }
}

void writeDPS()
{
    std::vector<uint8_t> stBF;
    for(uint16_t i = NUMBER_OF_SYSTEM ; i > 0 ; i-- )
    {
        stBF.push_back(dpsBF[i - 1]);
        dpsNOW[i - 1] = dpsBF[i - 1] & 0x3F;
    }
    spi_ps->spi_write_read(stBF);
}

void writeDSA()
{
    std::vector<uint8_t> stBF;
    for(uint16_t i = NUMBER_OF_SYSTEM ; i > 0 ; i-- )
    {
        stBF.push_back(dsaBF[i - 1] ^ 0x3F);
        dsaNOW[i - 1] = dsaBF[i - 1] & 0x3F;
    }
    spi_sa->spi_write_read(stBF);
}

void saveSTATE(const uint16_t &blockNum, const uint8_t &num)
{
    uint8_t romBF[FLASH_PAGE_SIZE];
    uint8_t ioBF;
    ioBF = stbAMP;
    ioBF += 2 * stbDRA;
    ioBF += 4 * stbLNA;
    ioBF += 8 * lowMODE;
    readROMblock(blockAddress(blockNum), romBF);
    for(uint i = 0; i < 15 ; i ++) { romBF[i + num * 0x40] = dpsNOW[i]; }
    romBF[15 + num * 0x40] = ioBF;
    for(uint i = 0; i < 16 ; i ++) { romBF[i + num * 0x40 + 16] = dsaNOW[i]; }
    writeROMblock(blockAddress(blockNum), romBF);
}

bool loadSTATE(const uint16_t &blockNum, const uint8_t &num)
{
    uint8_t romBF[FLASH_PAGE_SIZE];
    uint8_t ioBF;
    readROMblock(blockAddress(blockNum), romBF);
    ioBF = romBF[15 + num * 0x40];
    for(uint i = 0; i < 15; i++)
    {
        if((romBF[i + num * 0x40] & 0xC0) != 0) { return false; }
    }
    for(uint i = 0; i < 16; i++)
    {
        if((romBF[i + num * 0x40 + 16] & 0xC0) != 0) { return false; }
    }
    if((ioBF & 0xF0) != 0) { return false; }
    if((romBF[15 + num * 0x40 + 16] & 0xE0) != 0) { return false; }
    for(uint i = 0; i < 15; i++) { dpsBF[i] = romBF[i + num * 0x40]; }
    for(uint i = 0; i < 16; i++) { dsaNOW[i] = romBF[i + num * 0x40 + 16]; }
    stbAMP = (ioBF >> 0) & 1;
    stbDRA = (ioBF >> 1) & 1;
    stbLNA = (ioBF >> 2) & 1;
    lowMODE = (ioBF >> 3) & 1;
    return true;
}

#pragma endregion

void setup()
{
    stdio_init_all();
    sens = new ds18b20(pio0, SNS_TEMP_PIN);
    analog = new adc(true, true, false, false, 3.3f);
    spi_ps = new spi(spi0, SPI_CLK, SPI0_CLK_PIN, SPI0_TX_PIN, SPI0_RX_PIN, SPI0_LE_PIN, SPI_BITS, SPI_MODE, SPI_ORDER);
    uart = new pcabCMD(UART_TX_PIN, UART_RX_PIN, UART_BAUD_RATE);
    //GPIO Setup
    gpio_init(LPW_MOD_PIN);
    gpio_init(STB_DRA_PIN);
    gpio_init(STB_AMP_PIN);
    gpio_init(STB_LNA_PIN);
    gpio_init(SW_1_PIN);
    gpio_init(SW_2_PIN);
    gpio_init(SW_3_PIN);
    gpio_init(SW_4_PIN);
    gpio_init(SW_5_PIN);
    gpio_init(SW_6_PIN);
    gpio_set_dir(LPW_MOD_PIN ,GPIO_OUT);
    gpio_set_dir(STB_DRA_PIN ,GPIO_OUT);
    gpio_set_dir(STB_AMP_PIN ,GPIO_OUT);
    gpio_set_dir(STB_LNA_PIN ,GPIO_OUT);
    gpio_set_dir(SW_1_PIN ,GPIO_IN);
    gpio_set_dir(SW_2_PIN ,GPIO_IN);
    gpio_set_dir(SW_3_PIN ,GPIO_IN);
    gpio_set_dir(SW_4_PIN ,GPIO_IN);
    gpio_set_dir(SW_5_PIN ,GPIO_IN);
    gpio_set_dir(SW_6_PIN ,GPIO_IN);

    //Get Boot Mode
    bootMode = !gpio_get(SW_1_PIN);
    bootMode += 2 * !gpio_get(SW_2_PIN);
    bootMode += 4 * !gpio_get(SW_3_PIN);
    bootMode += 8 * !gpio_get(SW_4_PIN);
    bootMode += 16 * !gpio_get(SW_5_PIN);
    bootMode += 32 * !gpio_get(SW_6_PIN);
#ifdef DEBUG_BOOT_MODE
    bootMode = DEBUG_BOOT_MODE;
#endif
    serialNum = readSerialNum();

    // Resture Factory STATE
    if((bootMode == 0x20 || bootMode == 0x00) && !loadSTATE(ROM_BLOCK_NUM - 1, 0)) { }
    else if((bootMode != 0x01 && bootMode != 0x03 && bootMode != 0x05 && bootMode != 0x07) || !loadSTATE(ROM_BLOCK_NUM - 2, 0)) 
    {
        for(int i = 0; i < NUMBER_OF_SYSTEM; i++ ) { dpsBF[i] = 0; dsaBF[i] = 8;}
        dsaBF[NUMBER_OF_SYSTEM] = 0;
        stbAMP = false;
        stbDRA = false;
        stbLNA = false;
        lowMODE = false;
    }
    // save factory state
    if(bootMode == 0x20)
    {
        saveSTATE(ROM_BLOCK_NUM - 2, 0);
        saveSTATE(ROM_BLOCK_NUM - 2, 1);
        saveSTATE(ROM_BLOCK_NUM - 2, 2);
        saveSTATE(ROM_BLOCK_NUM - 2, 3);
    }
    
    // Write now state.
    writeDPS();
    gpio_put(STB_AMP_PIN, !stbAMP);
    gpio_put(STB_DRA_PIN, !stbDRA);
    gpio_put(STB_LNA_PIN, !stbLNA);
    gpio_put(LPW_MOD_PIN, !lowMODE);
}

int main()
{
    setup();
    while (1)
    {
        pcabCMD::CommandLine cmd = uart->readCMD(modeECHO);
        if( cmd.serialNum.size() > 0 && (String::strCompare(cmd.serialNum, "*", true) || String::strCompare(cmd.serialNum, serialNum, true)))
        {
            if(modeECHO && modeCUI){uart->uart.writeLine("");}
            switch (cmd.command)
            {
                case pcabCMD::cmdCode::WrtDPS:
                    if(cmd.argments.size() != 0) { uart->uart.writeLine("ERR > Number of arguments does not match."); }
                    else
                    {
                        writeDPS();
                        uart->uart.writeLine("DONE > Write Digital Phase Shifter Status.");
                    }
                    break;
                case pcabCMD::cmdCode::GetDPS:
                    if(cmd.argments.size() != 2) { uart->uart.writeLine("ERR > Number of arguments does not match."); }
                    else
                    {
                        bool blNOW;
                        if(String::strCompare(cmd.argments[0], "now")) { blNOW = true; }
                        else if(String::strCompare(cmd.argments[0], "bf")) { blNOW = false; }
                        else
                        {
                            if(!Convert::TryToBool(cmd.argments[0], blNOW)) { uart->uart.writeLine("ERR > Argument1 error."); break; }
                        }
                        uint16_t num;
                        if(!Convert::TryToUInt16(cmd.argments[1], 10, num)) { uart->uart.writeLine("ERR > Argument2 error."); break; }
                        if(NUMBER_OF_SYSTEM + 1 < num) { uart->uart.writeLine("ERR > The specified Argument1 is out of range."); break; }
                        if(modeCUI)
                        {
                            char ch[SNPRINTF_BUFFER_LEN];
                            int len;
                            if(blNOW && num == 0)
                            {
                                for(int i = 0; i < NUMBER_OF_SYSTEM; i++ )
                                {
                                    len = snprintf(ch, sizeof(ch), "Now DPS[%02d] > %7.3f[deg] (0d%03d, 0b%s)", i + 1, dpsNOW[i] * 5.625f, dpsNOW[i], Convert::ToString(dpsNOW[i], 2, 6).c_str());
                                    uart->uart.writeLine(std::string(ch, len));
                                }
                            }
                            else if(num == 0)
                            {
                                for(int i = 0; i < NUMBER_OF_SYSTEM; i++ )
                                {
                                    len = snprintf(ch, sizeof(ch), "Buffer DPS[%02d] > %7.3f[deg] (0d%03d, 0b%s)", i + 1, dpsBF[i] * 5.625f, dpsBF[i], Convert::ToString(dpsBF[i], 2, 6).c_str());
                                    uart->uart.writeLine(std::string(ch, len));
                                }
                            }
                            else if(blNOW)
                            {
                                len = snprintf(ch, sizeof(ch), "Now DPS[%02d] > %7.3f[deg] (0d%03d, 0b%s)", num, dpsNOW[num - 1] * 5.625f, dpsNOW[num - 1], Convert::ToString(dpsNOW[num - 1], 2, 6).c_str());
                                uart->uart.writeLine(std::string(ch, len));
                            }
                            else
                            {
                                len = snprintf(ch, sizeof(ch), "Buffer DPS[%02d] > %7.3f[deg] (0d%03d, 0b%s)", num, dpsBF[num - 1] * 5.625f, dpsBF[num - 1], Convert::ToString(dpsBF[num - 1], 2, 6).c_str());
                                uart->uart.writeLine(std::string(ch, len));
                            }
                            
                        }
                        else
                        {
                            if(blNOW && num == 0)
                            {
                                uart->uart.write(Convert::ToString(dpsNOW[0], 10, 1));
                                for(int i = 1; i < NUMBER_OF_SYSTEM; i++ ) { uart->uart.write("," + Convert::ToString(dpsNOW[i], 10, 1)); }
                                uart->uart.writeLine("");
                            }
                            else if(num == 0)
                            {
                                uart->uart.write(Convert::ToString(dpsBF[0], 10, 1));
                                for(int i = 1; i < NUMBER_OF_SYSTEM; i++ ) { uart->uart.write("," + Convert::ToString(dpsBF[i], 10, 1)); }
                                uart->uart.writeLine("");
                            }
                            else if(blNOW) { uart->uart.writeLine(Convert::ToString(dpsNOW[num - 1], 10, 1)); }
                            else { uart->uart.writeLine(Convert::ToString(dpsBF[num - 1], 10, 1)); }
                        }
                    }
                    break;
                case pcabCMD::cmdCode::SetDPS:
                    if(cmd.argments.size() != 2) { uart->uart.writeLine("ERR > Number of arguments does not match."); }
                    else
                    {
                        uint16_t num;
                        uint8_t conf;
                        if(!Convert::TryToUInt16(cmd.argments[0], 10, num)) { uart->uart.writeLine("ERR > Argument1 error."); break; }
                        if(NUMBER_OF_SYSTEM < uint(num - 1)) { uart->uart.writeLine("ERR > The specified Argument1 is out of range."); break; }
                        if(!Convert::TryToUInt8(cmd.argments[1], 10, conf)) { uart->uart.writeLine("ERR > Argument2 error."); break; }
                        if(conf > (1u << 6)) { uart->uart.writeLine("ERR > The specified Argument2 is out of range."); break; }
                        dpsBF[num - 1] = conf;
                        uart->uart.writeLine("DONE > Set to buffer.");
                    }
                    break;
                case pcabCMD::cmdCode::WrtDSA:
                    uart->uart.writeLine("ERR > Not supported in current version."); 
                    break;
                    if(cmd.argments.size() != 0) { uart->uart.writeLine("ERR > Number of arguments does not match."); }
                    else
                    {
                        writeDSA();
                        uart->uart.writeLine("DONE > Write Digital Step Attenuator Status.");
                    }
                    break;
                case pcabCMD::cmdCode::GetDSA:
                    uart->uart.writeLine("ERR > Not supported in current version."); 
                    break;
                case pcabCMD::cmdCode::SetDSA:
                    uart->uart.writeLine("ERR > Not supported in current version."); 
                    break;
                case pcabCMD::cmdCode::GetSTB_AMP:
                    if(cmd.argments.size() != 0) { uart->uart.writeLine("ERR > Number of arguments does not match."); }
                    else
                    {
                        if(modeCUI)
                        {
                            if(stbAMP) { uart->uart.writeLine("AMP > STANDBY MODE."); }
                            else { uart->uart.writeLine("AMP > RUN MODE."); }
                        }
                        else { uart->uart.writeLine(Convert::ToString(stbAMP)); }
                    }
                    break;
                case pcabCMD::cmdCode::GetSTB_DRA:
                    if(cmd.argments.size() != 0) { uart->uart.writeLine("ERR > Number of arguments does not match."); }
                    else
                    {
                        if(modeCUI)
                        {
                            if(stbDRA) { uart->uart.writeLine("DRA > STANDBY MODE."); }
                            else { uart->uart.writeLine("DRA > RUN MODE."); }
                        }
                        else { uart->uart.writeLine(Convert::ToString(stbDRA)); }
                    }
                    break;
                case pcabCMD::cmdCode::GetSTB_LNA:
                    if(cmd.argments.size() != 0) { uart->uart.writeLine("ERR > Number of arguments does not match."); }
                    else
                    {
                        if(modeCUI)
                        {
                            if(stbLNA) { uart->uart.writeLine("LNA > STANDBY MODE."); }
                            else { uart->uart.writeLine("LNA > RUN MODE."); }
                        }
                        else { uart->uart.writeLine(Convert::ToString(stbLNA)); }
                    }
                    break;
                case pcabCMD::cmdCode::GetLPM:
                    if(cmd.argments.size() != 0) { uart->uart.writeLine("ERR > Number of arguments does not match."); }
                    else
                    {
                        if(modeCUI)
                        {
                            if(lowMODE) { uart->uart.writeLine("HEA > LOW POWER MODE."); }
                            else { uart->uart.writeLine("HEA > FULL POWER MODE."); }
                        }
                        else { uart->uart.writeLine(Convert::ToString(lowMODE)); }
                    }
                    break;
                case pcabCMD::cmdCode::SetSTB_AMP:
                    if(cmd.argments.size() != 1) { uart->uart.writeLine("ERR > Number of arguments does not match."); }
                    else
                    {
                        bool bitBF;
                        if(!Convert::TryToBool(cmd.argments[0], bitBF)) { uart->uart.writeLine("ERR > Argument error."); break; }
                        stbAMP = bitBF;
                        gpio_put(STB_AMP_PIN, !stbAMP);
                        if(stbAMP) { uart->uart.writeLine("DONE > Setting AMP to standby mode."); }
                        else { uart->uart.writeLine("DONE > Setting AMP to run mode."); }
                    }
                    break;
                case pcabCMD::cmdCode::SetSTB_DRA:
                    if(cmd.argments.size() != 1) { uart->uart.writeLine("ERR > Number of arguments does not match."); }
                    else
                    {
                        bool bitBF;
                        if(!Convert::TryToBool(cmd.argments[0], bitBF)) { uart->uart.writeLine("ERR > Argument error."); break; }
                        stbDRA = bitBF;
                        gpio_put(STB_DRA_PIN, !stbDRA);
                        if(stbDRA) { uart->uart.writeLine("DONE > Setting DRA to standby mode."); }
                        else { uart->uart.writeLine("DONE > Setting DRA to run mode."); }
                    }
                    break;
                case pcabCMD::cmdCode::SetSTB_LNA:
                    if(cmd.argments.size() != 1) { uart->uart.writeLine("ERR > Number of arguments does not match."); }
                    else
                    {
                        bool bitBF;
                        if(!Convert::TryToBool(cmd.argments[0], bitBF)) { uart->uart.writeLine("ERR > Argument error."); break; }
                        stbLNA = bitBF;
                        gpio_put(STB_LNA_PIN, !stbLNA);
                        if(stbLNA) { uart->uart.writeLine("DONE > Setting LNA to standby mode."); }
                        else { uart->uart.writeLine("DONE > Setting LNA to run mode."); }
                    }
                    break;
                case pcabCMD::cmdCode::SetLPM:
                    if(cmd.argments.size() != 1) { uart->uart.writeLine("ERR > Number of arguments does not match."); }
                    else
                    {
                        bool bitBF;
                        if(!Convert::TryToBool(cmd.argments[0], bitBF)) { uart->uart.writeLine("ERR > Argument error."); break; }
                        lowMODE = bitBF;
                        gpio_put(LPW_MOD_PIN, !lowMODE);
                        if(lowMODE) { uart->uart.writeLine("DONE > Setting HEA to low power mode."); }
                        else { uart->uart.writeLine("DONE > Setting HEA to full power mode."); }
                    }
                    break;
                case pcabCMD::cmdCode::GetTMP_ID:
                    if(cmd.argments.size() != 1) { uart->uart.writeLine("ERR > Number of arguments does not match."); }
                    else
                    {
                        uint8_t num;
                        if(!Convert::TryToUInt8(cmd.argments[0], 10, num)) { uart->uart.writeLine("ERR > Argument error."); break; }
                        if(sens->getNumberOfSenser() < num || sens->getNumberOfSenser() == 0) { uart->uart.writeLine("ERR > Specified sensor does not exist."); break; }
                        if(num == 0)
                        {
                            std::vector<uint64_t> code = sens->getSENS_ROMCODE();
                            if(modeCUI)
                            {
                                char ch[SNPRINTF_BUFFER_LEN];
                                for(uint8_t i = 0 ; i < code.size() ; i++)
                                {
                                    int len = snprintf(ch, sizeof(ch), "%+3u > %s", i, Convert::ToString(code[i], 16, 16).c_str());
                                    uart->uart.writeLine(std::string(ch, len));
                                }
                            }
                            else
                            {
                                uart->uart.write(Convert::ToString(code[0], 16, 1));
                                for(uint8_t i = 1 ; i < code.size() ; i++)
                                { uart->uart.write("," + Convert::ToString(code[i], 16, 1)); }
                                uart->uart.writeLine("");
                            }
                        }
                        else
                        {
                            uint64_t code = sens->getSENS_ROMCODE(num - 1);
                            if(modeCUI)
                            {
                                char ch[SNPRINTF_BUFFER_LEN];
                                int len = snprintf(ch, sizeof(ch), "%u > %s", num, Convert::ToString(code, 16, 16).c_str());
                                uart->uart.writeLine(std::string(ch, len));
                            }
                            else
                            {
                                uart->uart.writeLine(Convert::ToString(code, 16, 1));
                            }
                        }
                    }
                    break;
                case pcabCMD::cmdCode::GetTMP_VAL:
                    if(cmd.argments.size() != 1) { uart->uart.writeLine("ERR > Number of arguments does not match."); }
                    else
                    {
                        uint8_t num;
                        if(!Convert::TryToUInt8(cmd.argments[0], 10, num)) { uart->uart.writeLine("ERR > Argument error."); break; }
                        if(sens->getNumberOfSenser() < num || sens->getNumberOfSenser() == 0) { uart->uart.writeLine("ERR > Specified sensor does not exist."); break; }
                        if(num == 0)
                        {
                            std::vector<float> code = sens->readTEMP();
                            if(modeCUI)
                            {
                                char ch[SNPRINTF_BUFFER_LEN];
                                for(uint8_t i = 0 ; i < code.size() ; i++)
                                {
                                    int len = snprintf(ch, sizeof(ch), "%+3u > %0.000f [degC]", i, code[i]);
                                    uart->uart.writeLine(std::string(ch, len));
                                }
                            }
                            else
                            {
                                uart->uart.write(std::to_string(code[0]));
                                for(uint8_t i = 1 ; i < code.size() ; i++)
                                { uart->uart.write("," + std::to_string(code[i])); }
                                uart->uart.writeLine("");
                            }
                        }
                        else
                        {
                            float code = sens->readTEMP(num - 1);
                            if(modeCUI)
                            {
                                char ch[SNPRINTF_BUFFER_LEN];
                                int len = snprintf(ch, sizeof(ch), "%+3u > %0.000f [degC]", num);
                                uart->uart.writeLine(std::string(ch, len));
                            }
                            else
                            {
                                uart->uart.writeLine(std::to_string(code));
                            }
                        }
                    }
                    break;
                case pcabCMD::cmdCode::GetTMP_CPU:
                    if(cmd.argments.size() != 0) { uart->uart.writeLine("ERR > Number of arguments does not match."); }
                    else
                    {
                        if(modeCUI)
                        {
                            char ch[SNPRINTF_BUFFER_LEN];
                            int len = snprintf(ch, sizeof(ch), "CPU TEMP > %.2f [degC]", analog->readTempCPU());
                            uart->uart.writeLine(std::string(ch, len));
                        } else { uart->uart.writeLine(std::to_string(analog->readADC4())); }
                    }
                    break;
                case pcabCMD::cmdCode::GetVd:
                    if(cmd.argments.size() != 0) { uart->uart.writeLine("ERR > Number of arguments does not match."); }
                    else
                    {
                        if(modeCUI)
                        {
                            char ch[SNPRINTF_BUFFER_LEN];
                            int len = snprintf(ch, sizeof(ch), "Vd > %.3f [V]", analog->readVoltageADC0() * 10.091);
                            uart->uart.writeLine(std::string(ch, len));
                        } else { uart->uart.writeLine(std::to_string(analog->readADC0())); }
                    }
                    break;
                case pcabCMD::cmdCode::GetId:
                    if(cmd.argments.size() != 0) { uart->uart.writeLine("ERR > Number of arguments does not match."); }
                    else
                    {
                        if(modeCUI)
                        {
                            char ch[SNPRINTF_BUFFER_LEN];
                            int len = snprintf(ch, sizeof(ch), "Id > %.3f [A]", ( analog->readVoltageADC1() - 1.65) / 0.09);
                            uart->uart.writeLine(std::string(ch, len));
                        } else { uart->uart.writeLine(std::to_string(analog->readADC1())); }
                    }
                    break;
                case pcabCMD::cmdCode::GetVin:
                    uart->uart.writeLine("ERR > Not supported in current version."); 
                    break;
                case pcabCMD::cmdCode::SaveMEM:
                    if(cmd.argments.size() > 1) { uart->uart.writeLine("ERR > Number of arguments does not match."); }
                    else
                    {
                        uint16_t blockNum = ROM_BLOCK_NUM - 2;
                        uint8_t num = 0;
                        if(cmd.argments.size() == 0) { blockNum = ROM_BLOCK_NUM - 2; num = 0; }
                        if(cmd.argments.size() == 1)
                        {
                            std::vector<std::string> strVect = String::split(cmd.argments[0], '-');
                            if(strVect.size() == 1)
                            { if(!Convert::TryToUInt8(strVect[0], 10, num)) { uart->uart.writeLine("ERR > Argument error."); break; } }
                            else if(strVect.size() == 2)
                            { if(!Convert::TryToUInt16(strVect[0], 10, blockNum) || !Convert::TryToUInt8(strVect[1], 10, num)) { uart->uart.writeLine("ERR > Argument error."); break; } }
                            else { uart->uart.writeLine("ERR > Argument error."); break; }
                            if(num > 3) { uart->uart.writeLine("ERR > Specified number is out of range."); break; }
                        }
                        if(!editRangeCheck(blockNum)) { uart->uart.writeLine("ERR > Specified block is out of range."); break; }
                        saveSTATE(blockNum, num);
                        uart->uart.writeLine("DONE > Save state.");
                    }
                    break;
                case pcabCMD::cmdCode::LoadMEM:
                    if(cmd.argments.size() > 1) { uart->uart.writeLine("ERR > Number of arguments does not match."); }
                    else
                    {
                        uint16_t blockNum = ROM_BLOCK_NUM - 2;
                        uint8_t num = 0;
                        if(cmd.argments.size() == 0) { blockNum = ROM_BLOCK_NUM - 2; num = 0; }
                        if(cmd.argments.size() == 1)
                        {
                            std::vector<std::string> strVect = String::split(cmd.argments[0], '-');
                            if(strVect.size() == 1)
                            { if(!Convert::TryToUInt8(strVect[0], 10, num)) { uart->uart.writeLine("ERR > Argument error."); break; } }
                            else if(strVect.size() == 2)
                            { if(!Convert::TryToUInt16(strVect[0], 10, blockNum) || !Convert::TryToUInt8(strVect[1], 10, num)) { uart->uart.writeLine("ERR > Argument error."); break; } }
                            else { uart->uart.writeLine("ERR > Argument error."); break; }
                            if(num > 3) { uart->uart.writeLine("ERR > Specified number is out of range."); break; }
                            if( blockNum != ROM_BLOCK_NUM - 2 && !editRangeCheck(blockNum)) { uart->uart.writeLine("ERR > Specified block is out of range."); break; }
                        }
                        if(!loadSTATE(blockNum, num)) { uart->uart.writeLine("ERR > No valid settings were found for the specified address."); break; }
                        writeDPS();
                        gpio_put(STB_AMP_PIN, !stbAMP);
                        gpio_put(STB_DRA_PIN, !stbDRA);
                        gpio_put(STB_LNA_PIN, !stbLNA);
                        gpio_put(LPW_MOD_PIN, !lowMODE);
                        uart->uart.writeLine("DONE > Load state.");
                        //writeDSA();
                    }
                    break;
                case pcabCMD::cmdCode::ReadROM:
                    if(cmd.argments.size() != 1) { uart->uart.writeLine("ERR > Number of arguments does not match."); }
                    else
                    {
                        uint16_t blockNum;
                        if(!Convert::TryToUInt16(cmd.argments[0], 10, blockNum)) { uart->uart.writeLine("ERR > Argument error."); break; }
                        if(blockNum > ROM_BLOCK_NUM - 1) { uart->uart.writeLine("ERR > Specified block is out of range."); break; }
                        uint8_t romDAT[FLASH_PAGE_SIZE];
                        readROMblock(blockAddress(blockNum), romDAT);
                        if(modeCUI)
                        {
                            for(int i = 0 ; i < 16 * (int)(FLASH_PAGE_SIZE / 16) ; i += 16 )
                            {
                                for(int j = 0 ; j < 15 ; j++)
                                {
                                    uart->uart.write(Convert::ToString(romDAT[i + j], 16, 2) + " ");
                                }
                                uart->uart.write(Convert::ToString(romDAT[i + 15], 16, 2));
                                uart->uart.writeLine("");
                            }
                            if(FLASH_PAGE_SIZE % 16 != 0)
                            {
                                for(uint i = FLASH_PAGE_SIZE - FLASH_PAGE_SIZE % 16 ; i < FLASH_PAGE_SIZE - 1 ; i++ )
                                {
                                    uart->uart.write(Convert::ToString(romDAT[i], 16, 2) + " ");
                                }
                                uart->uart.write(Convert::ToString(romDAT[FLASH_PAGE_SIZE - 1], 16, 2));
                                uart->uart.writeLine("");
                            }
                        } else { for(uint8_t byteBF : romDAT) { uart->uart.write(Convert::ToString(byteBF, 16, 2)); } uart->uart.writeLine(""); }
                    }
                    break;
                case pcabCMD::cmdCode::WriteROM:
                    if(cmd.argments.size() != 2) { uart->uart.writeLine("ERR > Number of arguments does not match."); }
                    else { writeROM(cmd.argments[0], cmd.argments[1]); }
                    break;
                case pcabCMD::cmdCode::EraseROM:
                    if(cmd.argments.size() != 1) { uart->uart.writeLine("ERR > Number of arguments does not match."); }
                    else
                    {
                        uint16_t blockNum;
                        if(!Convert::TryToUInt16(cmd.argments[0], 10, blockNum)) { uart->uart.writeLine("ERR > Argument error."); break; }
                        if(bootMode != 0x2A && bootMode != 0x04 && bootMode != 0x05 && bootMode != 0x06)
                        { uart->uart.writeLine("ERR > It is in an unusable state."); break; }
                        if(editRangeCheck(blockNum))
                        {
                            eraseROMblock(blockAddress(blockNum));
                            uart->uart.writeLine("DONE > Erase ROM block " + Convert::ToString(blockNum, 10, 1) + ".");
                        } else { uart->uart.writeLine("ERR > Specified block is out of range."); }
                    }
                    break;
                case pcabCMD::cmdCode::OverwriteROM:
                    if(cmd.argments.size() != 2) { uart->uart.writeLine("ERR > Number of arguments does not match."); }
                    else { }
                    break;
                case pcabCMD::cmdCode::SetSN:
                    if(cmd.argments.size() != 1) { uart->uart.writeLine("ERR > Number of arguments does not match."); }
                    else
                    {
                        if(cmd.argments[0].size() == 0) { uart->uart.writeLine("ERR > Argument error."); break; }
                        if(cmd.argments[0].size() > 16) { uart->uart.writeLine("ERR > Serial code is too long. Limit to 15 characters."); break; }
                        if(!editRangeCheck(ROM_BLOCK_NUM - 1)) { uart->uart.writeLine("ERR > It is in an unusable state."); break; }
                        
                        uint8_t romBF[FLASH_PAGE_SIZE];
                        readROMblock(blockAddress(ROM_BLOCK_NUM - 1), romBF);
                        for(uint i = 0; i < cmd.argments[0].size() ; i ++) { romBF[i + FLASH_PAGE_SIZE - 16] = cmd.argments[0][i]; }
                        romBF[FLASH_PAGE_SIZE - 1] = cmd.argments[0].size();
                        writeROMblock(blockAddress(ROM_BLOCK_NUM - 1), romBF);
                        serialNum = cmd.argments[0];
                        uart->uart.writeLine("DONE > Write serial number.");
                    }
                    break;
                case pcabCMD::cmdCode::RST:
                    if(cmd.argments.size() != 0) { uart->uart.writeLine("ERR > Number of arguments does not match."); }
                    else
                    {
                        if(!loadSTATE(NUMBER_OF_SYSTEM - 1, 0))
                        {
                            for(int i = 0; i < NUMBER_OF_SYSTEM; i++ ) { dpsBF[i] = 0; dsaBF[i] = 0;}
                            dsaBF[NUMBER_OF_SYSTEM] = 0;
                            stbAMP = false;
                            stbDRA = false;
                            stbLNA = false;
                            lowMODE = false;
                        }
                        writeDPS();
                        gpio_put(STB_AMP_PIN, !stbAMP);
                        gpio_put(STB_DRA_PIN, !stbDRA);
                        gpio_put(STB_LNA_PIN, !stbLNA);
                        gpio_put(LPW_MOD_PIN, !lowMODE);
                        uart->uart.writeLine("DONE > Reset state.");
                    }
                    break;
                case pcabCMD::cmdCode::ECHO:
                    if(cmd.argments.size() != 1) { uart->uart.writeLine("ERR > Number of arguments does not match."); }
                    else
                    {
                        bool mode;
                        if(!Convert::TryToBool(cmd.argments[0], mode)) { uart->uart.writeLine("ERR > Argument error."); break; }
                        modeECHO = mode;
                        if(modeECHO) { uart->uart.writeLine("DONE > WITH ECHO."); }
                        else { uart->uart.writeLine("DONE > WITHOUT ECHO."); }
                    }
                    break;
                case pcabCMD::cmdCode::CUI:
                    if(cmd.argments.size() != 1) { uart->uart.writeLine("ERR > Number of arguments does not match."); }
                    else
                    {
                        bool mode;
                        if(!Convert::TryToBool(cmd.argments[0], mode)) { uart->uart.writeLine("ERR > Argument error."); break; }
                        modeCUI = mode;
                        if(modeCUI) { uart->uart.writeLine("DONE > CUI MODE."); }
                        else { uart->uart.writeLine("DONE > GUI MODE."); }
                    }
                    break;
                case pcabCMD::cmdCode::GetMODE:
                    if(cmd.argments.size() != 0) { uart->uart.writeLine("ERR > Number of arguments does not match."); }
                    else{ uart->uart.writeLine("0x" + Convert::ToString(bootMode, 16, 2));}
                    break;
                case pcabCMD::cmdCode::Reboot:
                    if(cmd.argments.size() != 0) { uart->uart.writeLine("ERR > Number of arguments does not match."); }
                    setup();
                    break;
                case pcabCMD::cmdCode::GetIDN:
                    if(cmd.argments.size() != 0) { uart->uart.writeLine("ERR > Number of arguments does not match."); }
                    else
                    {
                        if(modeCUI)
                        {
                            uart->uart.writeLine("Vendor   > " + FW_VENDOR);
                            uart->uart.writeLine("Model    > " + FW_MODEL);
                            uart->uart.writeLine("SerialNo > " + serialNum);
                            uart->uart.writeLine("Revision > " + FW_REV);
                        } else { uart->uart.writeLine(FW_VENDOR + "," + FW_MODEL + "," + serialNum + "," + FW_REV); }
                    }
                    break;
                case pcabCMD::cmdCode::GetRID:
                    if(cmd.argments.size() != 0) { uart->uart.writeLine("ERR > Number of arguments does not match."); }
                    else
                    {
                        uint8_t id[FLASH_UNIQUE_ID_SIZE_BYTES];
                        flash::getID(id);
                        for(uint8_t idBF:id) { uart->uart.write(Convert::ToString(idBF,16,2)); }
                        uart->uart.writeLine("");
                    }
                    break;
                case pcabCMD::cmdCode::NONE:
                    uart->uart.writeLine("ERR > Command Not Found.");
                    break;
                default:
                    //uart->uart.writeLine("");
                    break;
            }
        }
    }
    close();
    return 0;
}

void close()
{
    delete uart;
    delete spi_ps;
    delete analog;
    delete sens;
}
