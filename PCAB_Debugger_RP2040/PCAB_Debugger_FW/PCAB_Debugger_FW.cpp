#include "MWComLibCPP_library.hpp"
#include "PCAB_Debugger_library.hpp"
#include "flash_library.hpp"
#include "Configure.hpp"

int main()
{
    uint8_t dat[FLASH_PAGE_SIZE];
    readMEMORYblock(31 * (UINT16_MAX + 1), dat);
    eraseMEMORYblock(31 * (UINT16_MAX + 1));
    saveMEMORYblock(31 * (UINT16_MAX + 1), dat);
    pcabCMD *uart = new pcabCMD(UART_TX_PIN, UART_RX_PIN, UART_BAUD_RATE);
    //uartSYNC *uart = new uartSYNC();

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