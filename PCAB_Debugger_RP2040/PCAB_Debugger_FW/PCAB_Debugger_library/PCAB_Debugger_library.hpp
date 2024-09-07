#pragma once
#include "MWComLibCPP_library.hpp"
#include "uart_library.hpp"

class pcabCMD : uartSYNC
{
    private:
    uint de_gpio;
    bool de_mode;
    uartSYNC uart;

    public:

    /// @brief Command cord. 
    enum cmdCode{
        WrtDPS,
        GetDPS,
        SetDPS,
        WrtDSA,
        GetDSA,
        SetDSA,
        GetTMP_ID,
        GetTMP_VAL,
        GetTMP_CPU,
        GetVd,
        GetId,
        GetVin,
        GetPin,
        GetSTB_AMP,
        SetSTB_AMP,
        GetSTB_DRA,
        SetSTB_DRA,
        GetSTB_LNA,
        SetSTB_LNA,
        GetLPM,
        SetLPM,
        GetALD,
        SetALD,
        SaveMEM,
        LoadMEM,
        ReadROM,
        WriteROM,
        EraseROM,
        OverwriteROM,
        SetSN,
        RST,
        ECHO,
        CUI,
        GetIDN,
        GetIDR,
        GetMODE,
        Reboot,
        NONE,
        BINARY,
        ASCII,
        NUL
    };

    /// @brief Command line structure.
    struct CommandLine
    {
        std::string serialNum;
        uint64_t romID;
        cmdCode command;
        std::vector<std::string> argments;
        std::vector<uint8_t> argment;
        CommandLine(std::string serial, std::string rom, cmdCode cmd, std::string args[], uint numArgs)
        {
            serialNum = serial;
            if(!Convert::TryToUInt64(rom, 16u, romID)) { romID = 0; }
            command = cmd;
            argments.clear();
            for(uint i = 0 ; i < numArgs ; i++){ argments.push_back(args[i]);}
        }
        CommandLine(std::string serial, std::string rom, cmdCode cmd, std::vector<uint8_t> arg)
        {
            serialNum = serial;
            if(!Convert::TryToUInt64(rom, 16u, romID)) { romID = 0; }
            command = cmd;
            argment = arg;
        }
    };

    /// @brief Constructor (UART ID = uart0 / Data bits = 8 / Stop Bits = 1 / UART_PARITY_NONE / CTS = false / RTS = false)
    /// @param uart UART CLASS
    /// @param rs485de_gpio RS485 DE gpio pin number.
    pcabCMD(uartSYNC uart, uint rs485de_gpio);

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
    /// @param rs485de_gpio RS485 DE gpio pin number.
    pcabCMD(uart_inst_t *uartID, uint tx_gpio, uint rx_gpio, uint baud_ratio, uint data_bits, uint stop_bits, uart_parity_t parity, bool cts, bool rts, std::string nlcode, uint rs485de_gpio);

    /// @brief Constructor (Data bits = 8 / Stop Bits = 1 / UART_PARITY_NONE / CTS = false / RTS = false)
    /// @param uartID UART ID (uart0 or uart1)
    /// @param tx_gpio Tx gpio pin number.
    /// @param rx_gpio Rx gpio pin number.
    /// @param baud_ratio Baud ratio.
    /// @param nlcode New Line string.
    /// @param rs485de_gpio RS485 DE gpio pin number.
    pcabCMD(uart_inst_t *uartID, uint tx_gpio, uint rx_gpio, uint baud_ratio, std::string nlcode, uint rs485de_gpio);

    /// @brief Constructor (UART ID = uart0 / Data bits = 8 / Stop Bits = 1 / UART_PARITY_NONE / CTS = false / RTS = false)
    /// @param tx_gpio Tx gpio pin number.
    /// @param rx_gpio Rx gpio pin number.
    /// @param baud_ratio Baud ratio.
    /// @param nlcode New Line string.
    /// @param rs485de_gpio RS485 DE gpio pin number.
    pcabCMD(uint tx_gpio, uint rx_gpio, uint baud_ratio, std::string nlcode, uint rs485de_gpio);

    /// @brief Constructor (UART ID = uart0 / Data bits = 8 / Stop Bits = 1 / UART_PARITY_NONE / CTS = false / RTS = false)
    /// @param tx_gpio Tx gpio pin number.
    /// @param rx_gpio Rx gpio pin number.
    /// @param baud_ratio Baud ratio.
    /// @param rs485de_gpio RS485 DE gpio pin number.
    pcabCMD(uint tx_gpio, uint rx_gpio, uint baud_ratio, uint rs485de_gpio);

    /// @brief Constructor (UART ID = uart0 / Data bits = 8 / Stop Bits = 1 / UART_PARITY_NONE / CTS = false / RTS = false)
    pcabCMD();

    /// @brief Destructor
    ~pcabCMD();

    /// @brief Read string line as command.
    /// @param echo Return echo during communication.
    /// @param slpi true:SLPI mode / false:ASCII mode
    CommandLine readCMD(bool echo, bool slpi);

    /// @brief Output uart string and tarminal code. (auto DE state change)
    /// @param str Write string.
    void write(std::string str);
    
    /// @brief Output uart bynary date.
    /// @param dat Write bynary date.
    void writeBlock(std::vector<uint8_t> dat);

    /// @brief Output uart strings. (auto DE state change)
    /// @param str Write string.
    void writeLine(std::string str);

    /// @brief Enable DE Pin.
    void rs485enable();

    /// @brief Disable DE Pin.
    void rs485disable();

    /// @brief Get DE state.
    /// @return DE state.
    bool getRS485mode();
};



