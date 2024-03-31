#include "uart_library.hpp"

class pcabCMD : uartSYNC
{
    public:
    uartSYNC uart;

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

    /// @brief Constructor (UART ID = uart0 / Data bits = 8 / Stop Bits = 1 / UART_PARITY_NONE / CTS = false / RTS = false)
    /// @param uart UART CLASS
    pcabCMD(uartSYNC uart);

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
    pcabCMD(uart_inst_t *uartID, uint tx_gpio, uint rx_gpio, uint baud_ratio, uint data_bits, uint stop_bits, uart_parity_t parity, bool cts, bool rts, std::string nlcode);

    /// @brief Constructor (Data bits = 8 / Stop Bits = 1 / UART_PARITY_NONE / CTS = false / RTS = false)
    /// @param uartID UART ID (uart0 or uart1)
    /// @param tx_gpio Tx gpio pin number.
    /// @param rx_gpio Rx gpio pin number.
    /// @param baud_ratio Baud ratio.
    /// @param nlcode New Line string.
    pcabCMD(uart_inst_t *uartID, uint tx_gpio, uint rx_gpio, uint baud_ratio, std::string nlcode);

    /// @brief Constructor (UART ID = uart0 / Data bits = 8 / Stop Bits = 1 / UART_PARITY_NONE / CTS = false / RTS = false)
    /// @param tx_gpio Tx gpio pin number.
    /// @param rx_gpio Rx gpio pin number.
    /// @param baud_ratio Baud ratio.
    /// @param nlcode New Line string.
    pcabCMD(uint tx_gpio, uint rx_gpio, uint baud_ratio, std::string nlcode);

    /// @brief Constructor (UART ID = uart0 / Data bits = 8 / Stop Bits = 1 / UART_PARITY_NONE / CTS = false / RTS = false)
    /// @param tx_gpio Tx gpio pin number.
    /// @param rx_gpio Rx gpio pin number.
    /// @param baud_ratio Baud ratio.
    pcabCMD(uint tx_gpio, uint rx_gpio, uint baud_ratio);

    /// @brief Constructor (UART ID = uart0 / Data bits = 8 / Stop Bits = 1 / UART_PARITY_NONE / CTS = false / RTS = false)
    pcabCMD();

    /// @brief Destructor
    ~pcabCMD();

    /// @brief Read string line as command.
    /// @param echo Return echo during communication.
    CommandLine readCMD(bool echo);

};



