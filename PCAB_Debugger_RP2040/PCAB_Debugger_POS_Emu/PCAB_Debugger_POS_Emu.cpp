#include "PCAB_Debugger_POS_Emu.hpp"

int main()
{
    uartSYNC uart = uartSYNC(uart0, UART_TX_PIN, UART_RX_PIN, UART_BAUD_RATE, "\r\n");

    std::vector<uint8_t> dat;
    dat.clear();

    while (1)
    {
        uart_write_blocking(uart0, dat.data(),dat.size());
    }
    return 0;
}