#include "PCAB_Debugger_POS_Emu.hpp"

int main()
{
    uart_init(uart0, UART_BAUD_RATE);
    gpio_set_function(UART_TX_PIN, GPIO_FUNC_UART);
    gpio_set_function(UART_RX_PIN, GPIO_FUNC_UART);
    uart_set_hw_flow(uart0, false, false);
    uart_set_format(uart0, UART_DATA_BITS, UART_STOP_BIT, UART_PARITY_NONE);
    std::vector<uint8_t> dat;
    dat.clear();

    while (1)
    {
        uart_write_blocking(uart0, dat.data(),dat.size());
    }
    return 0;
}