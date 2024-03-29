#include "uart_library.hpp"

//uartSYNC::uartSYNC(uart_inst_t *uartID, uint tx_gpio, uint rx_gpio, uint baud_ratio, uint data_bits, uint stop_bits, uart_parity_t parity, bool cts, bool rts, std::string nlcode) : uart(uartID), nlc(nlcode)
uartSYNC::uartSYNC(uart_inst_t *uartID, uint tx_gpio, uint rx_gpio, uint baud_ratio, uint data_bits, uint stop_bits, uart_parity_t parity, bool cts, bool rts, std::string nlcode)
: uart(uartID), nlc(nlcode)
{
    uart_init(uart, baud_ratio);
    gpio_set_function(tx_gpio, GPIO_FUNC_UART);
    gpio_set_function(rx_gpio, GPIO_FUNC_UART);
    uart_set_hw_flow(uart, cts, rts);
    uart_set_format(uart, data_bits, stop_bits, parity);
}

uartSYNC::uartSYNC(uart_inst_t *uartID, uint tx_gpio, uint rx_gpio, uint baud_ratio, std::string nlcode)
{
    uartSYNC(uartID, tx_gpio, rx_gpio, baud_ratio, 8, 1, UART_PARITY_NONE, false, false, nlcode);
}

uartSYNC::uartSYNC(uint tx_gpio, uint rx_gpio, uint baud_ratio, std::string nlcode)
{
    uartSYNC(uart0, tx_gpio, rx_gpio, baud_ratio, nlcode);
}

uartSYNC::uartSYNC(uint tx_gpio, uint rx_gpio, uint baud_ratio)
{
    uartSYNC(uart0, tx_gpio, rx_gpio, baud_ratio, "\n");
}
uartSYNC::uartSYNC() { uartSYNC(0, 1, 9600); }

uartSYNC::~uartSYNC()
{
}

std::string uartSYNC::readLine(bool echo)
{
    char chBF;
    int cnt = 0;
    std::string cmdline = "";
    do
    {
        chBF = uart_getc(uart);
        if(echo){uart_puts(uart, (std::string() + chBF).c_str());}
        cmdline += chBF;
        cnt++;
    } while (chBF && chBF != '\n');
    return cmdline;
}


uartSYNC::CommandLine uartSYNC::readCMD(bool echo)
{
    return uartSYNC::CommandLine("",NULL,0);
}

void uartSYNC::write(std::string str) { uart_puts(uart, str.c_str()); }

void uartSYNC::writeLine(std::string str) { write(str + nlc); }