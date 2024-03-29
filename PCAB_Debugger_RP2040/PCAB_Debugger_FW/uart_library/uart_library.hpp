#include <string>
#include <vector>
#include "pico/stdlib.h"

class uartSYNC
{
    private:
    uart_inst_t *uart;
    std::string nlc;
    
    public:

    /// @brief Command cord. 
    enum cmdCode{
        WrtPS,
        GetPS,
        SetPS,
        GetTMP,
        GetId,
        GetVd,
        GetSTB_AMP,
        SetSTB_AMP,
        GetSTB_DRA,
        SetSTB_DRA,
        GetSTB_LNA,
        SetSTB_LNA,
        GetLPM,
        SetLPM,
        SetALD,
        GetALD,
        SaveMEM,
        LoadMEM,
        GetIDN,
        SetIDN,
        RST,
        CUI,
        NONE
    };

    /// @brief Command line structure.
    struct CommandLine
    {
        cmdCode command;
        std::vector<std::string> argments;
        CommandLine(cmdCode cmd, std::string args[], uint numArgs)
        {
            command = cmd;
            argments.clear();
            for(uint i = 0 ; i < numArgs ; i++){ argments.push_back(args[i]);}
        }
    };

    /// @brief Constructor
    /// @param uartID UART ID (uart0 or uart1)
    /// @param tx_gpio Tx gpio pin number.
    /// @param rx_gpio Rx gpio pin number.
    /// @param baud_ratio Baud ratio.
    /// @param data_bits Number of data bits.
    /// @param stop_bits Number of stop bits.
    /// @param parity Parity.
    /// @param cts CTS
    /// @param rts RTX
    /// @param nlcode New Line string.
    uartSYNC(uart_inst_t *uartID, uint tx_gpio, uint rx_gpio, uint baud_ratio, uint data_bits, uint stop_bits, uart_parity_t parity, bool cts, bool rts, std::string nlcode);

    /// @brief Constructor (Data bits = 8 / Stop Bits = 1 / UART_PARITY_NONE / CTS = false / RTS = false)
    /// @param uartID UART ID (uart0 or uart1)
    /// @param tx_gpio Tx gpio pin number.
    /// @param rx_gpio Rx gpio pin number.
    /// @param baud_ratio Baud ratio.
    /// @param nlcode New Line string.
    uartSYNC(uart_inst_t *uartID, uint tx_gpio, uint rx_gpio, uint baud_ratio, std::string nlcode);

    /// @brief Constructor (UART ID = uart0 / Data bits = 8 / Stop Bits = 1 / UART_PARITY_NONE / CTS = false / RTS = false)
    /// @param tx_gpio Tx gpio pin number.
    /// @param rx_gpio Rx gpio pin number.
    /// @param baud_ratio Baud ratio.
    /// @param nlcode New Line string.
    uartSYNC(uint tx_gpio, uint rx_gpio, uint baud_ratio, std::string nlcode);

    /// @brief Constructor (UART ID = uart0 / Data bits = 8 / Stop Bits = 1 / UART_PARITY_NONE / CTS = false / RTS = false)
    /// @param tx_gpio Tx gpio pin number.
    /// @param rx_gpio Rx gpio pin number.
    /// @param baud_ratio Baud ratio.
    uartSYNC(uint tx_gpio, uint rx_gpio, uint baud_ratio);

    /// @brief Constructor (UART ID = uart0 / Data bits = 8 / Stop Bits = 1 / UART_PARITY_NONE / CTS = false / RTS = false)
    uartSYNC();

    /// @brief Destructor
    ~uartSYNC();

    /// @brief Read one line of string.
    /// @param echo Return echo during communication.
    std::string readLine(bool echo);

    /// @brief Read string line as command.
    /// @param echo Return echo during communication.
    CommandLine readCMD(bool echo);
    
    /// @brief Send string.
    /// @param str string to send.
    void write(std::string str);

    /// @brief Send string as one line. (Add new line string automatically string to send.)
    /// @param str string to send.
    void writeLine(std::string str);
};

