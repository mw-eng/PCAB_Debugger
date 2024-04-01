#include "PCAB_Debugger_FW.hpp"

const static std::string FW_VENDOR = "Orient Microwave Corp.";
const static std::string FW_MODEL = "PCAB_Debugger";
const static std::string FW_REV = "1.1.0";

ds18b20 *sens;
adc *analog;
spi *spi_ps;
pcabCMD *uart;
bool modeCUI = true;
bool modeECHO = true;
uint serialNum = 0;

void setup()
{
    stdio_init_all();
    sens = new ds18b20(pio0, SNS_TEMP_PIN);
    analog = new adc();
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
}

int main()
{
    setup();
    while (1)
    {
        pcabCMD::CommandLine cmd = uart->readCMD(modeECHO);
        if(modeECHO && modeCUI){uart->uart.writeLine("");}
        switch (cmd.command)
        {
        case pcabCMD::cmdCode::WrtDPS:
            break;
        case pcabCMD::cmdCode::GetDPS:
            break;
        case pcabCMD::cmdCode::SetDPS:
            break;
        case pcabCMD::cmdCode::WrtDSA:
            uart->uart.writeLine("ERR > Not supported in current version."); 
            break;
        case pcabCMD::cmdCode::GetDSA:
            uart->uart.writeLine("ERR > Not supported in current version."); 
            break;
        case pcabCMD::cmdCode::SetDSA:
            uart->uart.writeLine("ERR > Not supported in current version."); 
            break;
        case pcabCMD::cmdCode::GetTMP_ID:
            break;
        case pcabCMD::cmdCode::GetTMP_VAL:
            break;
        case pcabCMD::cmdCode::GetId:
            break;
        case pcabCMD::cmdCode::GetVd:
            break;
        case pcabCMD::cmdCode::GetSTB_AMP:
            break;
        case pcabCMD::cmdCode::GetSTB_DRA:
            break;
        case pcabCMD::cmdCode::GetSTB_LNA:
            break;
        case pcabCMD::cmdCode::GetLPM:
            break;
        case pcabCMD::cmdCode::SetSTB_AMP:
            break;
        case pcabCMD::cmdCode::SetSTB_DRA:
            break;
        case pcabCMD::cmdCode::SetSTB_LNA:
            break;
        case pcabCMD::cmdCode::SetLPM:
            break;
        case pcabCMD::cmdCode::GetALD:
            break;
        case pcabCMD::cmdCode::SetALD:
            break;
        case pcabCMD::cmdCode::SaveMEM:
            break;
        case pcabCMD::cmdCode::LoadMEM:
            break;
        case pcabCMD::cmdCode::ReadROM:
            if(cmd.argments.size() != 1) { uart->uart.writeLine("ERR > Number of arguments does not match."); }
            else
            {
                uint16_t block;
                if(!Convert::TryToUInt16(cmd.argments[0], 10, block)) { uart->uart.writeLine("ERR > Argument error."); }
                else
                {
                    if(block > ROM_BLOCK_MAX) { uart->uart.writeLine("ERR > Specified block is out of range."); }
                    else
                    {
                        uint8_t romDAT[FLASH_PAGE_SIZE];
                        readROMblock(blockAddress(block), romDAT);
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
                                for(int i = FLASH_PAGE_SIZE - FLASH_PAGE_SIZE % 16 ; i < FLASH_PAGE_SIZE - 1 ; i++ )
                                {
                                    uart->uart.write(Convert::ToString(romDAT[i], 16, 2) + " ");
                                }
                                uart->uart.write(Convert::ToString(romDAT[FLASH_PAGE_SIZE - 1], 16, 2));
                                uart->uart.writeLine("");
                            }
                        } else { for(uint8_t byteBF : romDAT) { uart->uart.write(Convert::ToString(byteBF, 16, 2)); } uart->uart.writeLine(""); }
                    }
                }
            }
            break;
        case pcabCMD::cmdCode::WriteROM:
            break;
        case pcabCMD::cmdCode::EraseROM:
            break;
        case pcabCMD::cmdCode::GetSN:
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
                if(Convert::TryToBool(cmd.argments[0], mode)) { uart->uart.writeLine("ERR > Argument error."); }
                else { modeECHO = mode; }
            }
            break;
        case pcabCMD::cmdCode::CUI:
            if(cmd.argments.size() != 1) { uart->uart.writeLine("ERR > Number of arguments does not match."); }
            else
            {
                bool mode;
                if(Convert::TryToBool(cmd.argments[0], mode)) { uart->uart.writeLine("ERR > Argument error."); }
                else { modeCUI = mode; }
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
                    uart->uart.writeLine("SerialNo > " + Convert::ToString(serialNum, 10, 3));
                    uart->uart.writeLine("Revision > " + FW_REV);
                } else { uart->uart.writeLine(FW_VENDOR + "," + FW_MODEL + "," + Convert::ToString(serialNum, 10, 3) + "," + FW_REV); }
            }
            break;
        case pcabCMD::cmdCode::NONE:
            uart->uart.writeLine("ERR > Command Not Found.");
            break;
        default:
            uart->uart.writeLine("");
            break;
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
