#define DEBUG_ADMIN
//#define DEBUG_FRB

#include "PCAB_Debugger_FW.hpp"
#define SNPRINTF_BUFFER_LEN 50
#define NUMBER_OF_SYSTEM 15
const static std::string FW_VENDOR = "Orient Microwave Corp.";
const static std::string FW_MODEL = "LX00-0004-00";
const static std::string FW_REV = "1.1.0";
// ROM Block Number
#define ROM_BLOCK_USER 16   // Range of user available space from this block number to ROM_BLOCK_MAX - 2
#define ROM_BLOCK_MAX 32    // 16M (Raspberry Pi Pico)
//#define ROM_BLOCK_MAX 64    // 32M
//#define ROM_BLOCK_MAX 128   // 64M
//#define ROM_BLOCK_MAX 256   // 128M (PCAB)
//#define ROM_BLOCK_MAX 512   // 256M


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
    if(blockNum < ROM_BLOCK_USER || ROM_BLOCK_MAX - 1 < blockNum) { return false; }
    if(bootMode == 0x2A || (ROM_BLOCK_USER <= blockNum && blockNum < ROM_BLOCK_MAX - 2)) { return true; }
    return false;
}

void writeROM(const uint16_t &blockNum, const uint8_t blockDAT[FLASH_PAGE_SIZE])
{
    if(bootMode == 0x2A || bootMode == 0x04 || bootMode == 0x05 || bootMode == 0x06 || bootMode == 0x07)
    {
        if(editRangeCheck(blockNum))
        {
            writeROMblock(blockAddress(blockNum), blockDAT);
            uart->uart.writeLine("DONE > Write ROM block " + Convert::ToString(blockNum, 10, 0) + ".");
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
    readROMblock(blockAddress(ROM_BLOCK_MAX - 1), romBF);
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
        stBF.push_back(dpsBF[i]);
        dpsNOW[i] = dpsBF[i] & 0x3F;
    }
    spi_ps->spi_write_read(stBF);
}

void writeDSA()
{
    std::vector<uint8_t> stBF;
    for(uint16_t i = NUMBER_OF_SYSTEM ; i > 0 ; i-- )
    {
        stBF.push_back(dsaBF[i]);
        dsaNOW[i] = dsaBF[i] & 0x3F;
    }
    spi_sa->spi_write_read(stBF);
}

#pragma endregion

void setup()
{
    stdio_init_all();
    sens = new ds18b20(pio0, SNS_TEMP_PIN);
    analog = new adc(true, true, true, false, 3.3f);
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
#ifdef DEBUG_ADMIN
    bootMode = 0x2A;
#endif
#ifdef DEBUG_FRB
    bootMode = 0x20;
#endif
    serialNum = readSerialNum();
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
                                uart->uart.write(Convert::ToString(dpsNOW[0], 10, 0));
                                for(int i = 1; i < NUMBER_OF_SYSTEM; i++ ) { uart->uart.write("," + Convert::ToString(dpsNOW[i], 10, 0)); }
                                uart->uart.writeLine("");
                            }
                            else if(num == 0)
                            {
                                uart->uart.write(Convert::ToString(dpsBF[0], 10, 0));
                                for(int i = 1; i < NUMBER_OF_SYSTEM; i++ ) { uart->uart.write("," + Convert::ToString(dpsBF[i], 10, 0)); }
                                uart->uart.writeLine("");
                            }
                            else if(blNOW) { uart->uart.writeLine(Convert::ToString(dpsNOW[num - 1], 10, 0)); }
                            else { uart->uart.writeLine(Convert::ToString(dpsBF[num - 1], 10, 0)); }
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
                        if(conf > (1 << 6)) { uart->uart.writeLine("ERR > The specified Argument2 is out of range."); break; }
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
                case pcabCMD::cmdCode::GetTMP_ID:
                    if(cmd.argments.size() != 1) { uart->uart.writeLine("ERR > Number of arguments does not match."); }
                    else
                    {
                        uint8_t num;
                        if(!Convert::TryToUInt8(cmd.argments[0], 10, num)) { uart->uart.writeLine("ERR > Argument error."); break; }
                        if(sens->getNumberOfSenser() < num) { uart->uart.writeLine("ERR > Specified sensor does not exist."); break; }
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
                                uart->uart.writeLine(Convert::ToString(code[0], 16, 0));
                                for(uint8_t i = 1 ; i < code.size() ; i++)
                                { uart->uart.writeLine("," + Convert::ToString(code[i], 16, 0)); }
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
                                uart->uart.writeLine(Convert::ToString(code, 16, 0));
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
                        if(sens->getNumberOfSenser() < num) { uart->uart.writeLine("ERR > Specified sensor does not exist."); break; }
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
                                uart->uart.writeLine(std::to_string(code[0]));
                                for(uint8_t i = 1 ; i < code.size() ; i++)
                                { uart->uart.writeLine("," + std::to_string(code[i])); }
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
                            int len = snprintf(ch, sizeof(ch), "Vd > %.3f [V]", analog->readVoltageADC0());
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
                        gpio_put(STB_AMP_PIN, !lowMODE);
                        if(lowMODE) { uart->uart.writeLine("DONE > Setting HEA to low power mode."); }
                        else { uart->uart.writeLine("DONE > Setting HEA to full power mode."); }
                    }
                    break;
                case pcabCMD::cmdCode::SaveMEM:
                    break;
                case pcabCMD::cmdCode::LoadMEM:
                    break;
                case pcabCMD::cmdCode::ReadROM:
                    if(cmd.argments.size() != 1) { uart->uart.writeLine("ERR > Number of arguments does not match."); }
                    else
                    {
                        uint16_t blockNum;
                        if(!Convert::TryToUInt16(cmd.argments[0], 10, blockNum)) { uart->uart.writeLine("ERR > Argument error."); break; }
                        if(blockNum > ROM_BLOCK_MAX - 1) { uart->uart.writeLine("ERR > Specified block is out of range."); break; }
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
                            uart->uart.writeLine("DONE > Erase ROM block " + Convert::ToString(blockNum, 10, 0) + ".");
                        } else { uart->uart.writeLine("ERR > Specified block is out of range."); }
                    }
                    break;
                case pcabCMD::cmdCode::SetSN:
                    break;
                case pcabCMD::cmdCode::RST:
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
