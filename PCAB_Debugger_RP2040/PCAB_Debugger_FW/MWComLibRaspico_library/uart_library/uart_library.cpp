#include "uart_library.hpp"
#include "MWComLibCPP_library.hpp"

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
: uartSYNC(uartID, tx_gpio, rx_gpio, baud_ratio, 8, 1, UART_PARITY_NONE, false, false, nlcode){}

uartSYNC::uartSYNC(uint tx_gpio, uint rx_gpio, uint baud_ratio, std::string nlcode)
: uartSYNC(uart0, tx_gpio, rx_gpio, baud_ratio, nlcode){}

uartSYNC::uartSYNC(uint tx_gpio, uint rx_gpio, uint baud_ratio)
: uartSYNC(uart0, tx_gpio, rx_gpio, baud_ratio, "\r\n"){}

uartSYNC::uartSYNC() : uartSYNC(0, 1, 9600){}

uartSYNC::~uartSYNC() {}

std::string uartSYNC::readLine(bool echo)
{
    char chBF;
    std::string strBf = "";
    do
    {
        chBF = uart_getc(uart);
        if(echo){uart_puts(uart, (std::string() + chBF).c_str());}
        strBf += chBF;
        if(strBf.find_last_not_of(nlc) == std::string::npos){return "";}
    } while (strBf.find_last_not_of(nlc) == strBf.length() - 1 );
    return rtrim(strBf, nlc);
}

void uartSYNC::write(std::string str) { uart_puts(uart, str.c_str()); }

void uartSYNC::writeLine(std::string str) { write(str + nlc); }

uartSYNC::CommandLine uartSYNC::readCMD(bool echo)
{
    std::string strBf = readLine(echo);
    std::vector<std::string> strVect = split(strBf, ' ');
    if(strVect.size() <= 0){return uartSYNC::CommandLine("", NULL, 0);}
    strBf = strVect[0];
    strVect.erase(std::cbegin(strVect));
    std::string strArr[strVect.size()];
    std::copy(strVect.begin(), strVect.end(), strArr);
    return uartSYNC::CommandLine(strBf, strArr, strVect.size());
}