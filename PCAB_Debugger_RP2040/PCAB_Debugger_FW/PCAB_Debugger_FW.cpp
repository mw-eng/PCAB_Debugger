#include "PCAB_Debugger_FW.hpp"

ds18b20 *sens;
adc *analog;
spi *spi_ps;
pcabCMD *uart;
bool cuiMODE = true;

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
        pcabCMD::CommandLine cmd = uart->readCMD(true);
        switch (cmd.command)
        {
        case pcabCMD::cmdCode::WrtPS:
            break;
        case pcabCMD::cmdCode::GetPS:
            break;
        case pcabCMD::cmdCode::SetPS:
            break;
        case pcabCMD::cmdCode::GetTMP:
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
        case pcabCMD::cmdCode::SetALD:
            break;
        case pcabCMD::cmdCode::GetALD:
            break;
        case pcabCMD::cmdCode::SaveMEM:
            break;
        case pcabCMD::cmdCode::LoadMEM:
            break;
        case pcabCMD::cmdCode::ReadROM:
            if(cmd.argments.size() != 1) { uart->uart.writeLine("ERR > Argument Error."); }
            else
            {
                uint16_t block;
                if(!Convert::TryToUInt16(cmd.argments[0], 10, block)) { uart->uart.writeLine("ERR > Argument Error."); }
                else
                {
                    if(block > ROM_BLOCK_MAX) { uart->uart.writeLine("ERR > Specified block is out of range."); }
                    else
                    {
                        uint8_t romDAT[FLASH_PAGE_SIZE];
                        readROMblock(blockAddress(block), romDAT);
                        if(cuiMODE)
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
                            for(int i = FLASH_PAGE_SIZE - FLASH_PAGE_SIZE % 16 ; i < FLASH_PAGE_SIZE - 1 ; i++ )
                            {
                                uart->uart.write(Convert::ToString(romDAT[i], 16, 2) + " ");
                            }
                            uart->uart.write(Convert::ToString(romDAT[FLASH_PAGE_SIZE - 1], 16, 2));
                            uart->uart.writeLine("");
                        } else { for(uint8_t byteBF : romDAT) { uart->uart.write(Convert::ToString(byteBF, 16, 2)); } uart->uart.writeLine(""); }
                    }
                }
            }
            break;
        case pcabCMD::cmdCode::WriteROM:
            break;
        case pcabCMD::cmdCode::EraseROM:
            break;
        case pcabCMD::cmdCode::GetID:
            break;
        case pcabCMD::cmdCode::SetID:
            break;
        case pcabCMD::cmdCode::RST:
            break;
        case pcabCMD::cmdCode::ECHO:
            break;
        case pcabCMD::cmdCode::GetIDN:
            break;
        case pcabCMD::cmdCode::NONE:
            uart->uart.writeLine("ERR > Command Not Found.");
            break;
        default:
            break;
        }
    }

    
//    uint8_t dat[FLASH_PAGE_SIZE];
//    readROMblock(31 * (UINT16_MAX + 1), dat);
//    eraseROMblock(31 * (UINT16_MAX + 1));
//    saveROMblock(31 * (UINT16_MAX + 1), dat);
//
//    std::vector<uint64_t> romCODE = sens->getSENS_ROMCODE();
//    std::vector<std::string> temp = sens->readTEMP();
//
//    float adc0 = analog->readVoltageADC0();
//    float vsys = analog->readVsys();
//    float cpuT = analog->readTempCPU();
//    uint16_t adc0ui = analog->readADC0();
//
//    uart->uart.writeLine(Convert::ToString(25834, 16, 5));
//    int iBF;
//    if(Convert::TryToInt("+13355", iBF))
//    {
//        uart->uart.writeLine(std::to_string(iBF));
//    }
//
//    uint64_t uiBF;
//    if(Convert::TryToUInt64("64EfFc", 16, uiBF))
//    {
//        uart->uart.writeLine(Convert::ToString(uiBF, 16, 0));
//    }
//
//    for (uint64_t &x:romCODE)
//    {
//        uart->uart.writeLine(std::to_string(x));
//    }
//    for (std::string &x:temp)
//    {
//        uart->uart.writeLine(x);
//    }
//    uart->uart.writeLine(std::to_string(adc0));
//    uart->uart.writeLine(std::to_string(vsys));
//    uart->uart.writeLine(std::to_string(cpuT));
//    uart->uart.writeLine(std::to_string(adc0ui));
//
//
//    uart->uart.writeLine("test");
//    while(1){
//        pcabCMD::CommandLine cmd = uart->readCMD(true);
//        uart->uart.write("\r\n");
//        uart->uart.writeLine(std::to_string(cmd.command));
//        for(auto &num : cmd.argments){
//            uart->uart.writeLine(num);
//        }
//    };
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
