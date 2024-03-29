#include "MWComLibRaspico_library.hpp"
#include "uart_library.hpp"
#include "Configure.hpp"

int main()
{
    uartSYNC *uart = new uartSYNC(UART_TX_PIN, UART_RX_PIN, UART_BAUD_RATE);
    uart->writeLine("test");
    while(1){
        uart->writeLine("test");
        std::string strBF = uart->readLine(true);
    };
    delete uart;
    return 0;
}