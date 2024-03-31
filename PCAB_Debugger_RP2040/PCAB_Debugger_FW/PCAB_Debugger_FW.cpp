#include "MWComLibCPP_library.hpp"
#include "PCAB_Debugger_library.hpp"
#include "ds18b20_library.hpp"
#include "flash_library.hpp"
#include "spi_library.hpp"
#include "Configure.hpp"

int main()
{
    uint8_t dat[FLASH_PAGE_SIZE];
    readMEMORYblock(31 * (UINT16_MAX + 1), dat);
    eraseMEMORYblock(31 * (UINT16_MAX + 1));
    saveMEMORYblock(31 * (UINT16_MAX + 1), dat);

    ds18b20 *sens = new ds18b20(pio0, SNS_TEMP_PIN);
    std::vector<uint64_t> romCODE = sens->getSENS_ROMCODE();
    std::vector<std::string> temp = sens->readTEMP();

    pcabCMD *uart = new pcabCMD(UART_TX_PIN, UART_RX_PIN, UART_BAUD_RATE);

    for (uint64_t &x:romCODE)
    {
        uart->uart.writeLine(std::to_string(x));
    }
    for (std::string &x:temp)
    {
        uart->uart.writeLine(x);
    }


    uart->uart.writeLine("test");
    while(1){
        pcabCMD::CommandLine cmd = uart->readCMD(true);
        uart->uart.write("\r\n");
        uart->uart.writeLine(std::to_string(cmd.command));
        for(auto &num : cmd.argments){
            uart->uart.writeLine(num);
        }
    };
    delete uart;
    return 0;
}