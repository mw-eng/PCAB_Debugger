#include "MWComLibRaspico_library.hpp"
#include "uart_library.hpp"
#include "Configure.hpp"

int main()
{
    uartSYNC *uart = new uartSYNC(UART_TX_PIN, UART_RX_PIN, UART_BAUD_RATE);
    //uartSYNC *uart = new uartSYNC();
    uart->writeLine("test");
    while(1){
        uartSYNC::CommandLine cmd = uart->readCMD(true);
        uart->write("\r\n");
        uart->writeLine(std::to_string(cmd.command));
        for(auto &num : cmd.argments){
            uart->writeLine(num);
        }
        
    };
    delete uart;
    return 0;
}