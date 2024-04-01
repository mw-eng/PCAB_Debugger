#include "PCAB_Debugger_library.hpp"
#include "MWComLibCPP_library.hpp"

#pragma region pcabCMD Class

pcabCMD::pcabCMD(uartSYNC uart) : uart(uart){}

pcabCMD::pcabCMD(uart_inst_t *uartID, uint tx_gpio, uint rx_gpio, uint baud_ratio, uint data_bits, uint stop_bits, uart_parity_t parity, bool cts, bool rts, std::string nlcode)
: uart(uartSYNC(uartID, tx_gpio, rx_gpio, baud_ratio, data_bits, stop_bits, parity, cts, rts, nlcode)){}

pcabCMD::pcabCMD(uart_inst_t *uartID, uint tx_gpio, uint rx_gpio, uint baud_ratio, std::string nlcode)
: pcabCMD(uartID, tx_gpio, rx_gpio, baud_ratio, 8, 1, UART_PARITY_NONE, false, false, nlcode){}

pcabCMD::pcabCMD(uint tx_gpio, uint rx_gpio, uint baud_ratio, std::string nlcode)
: pcabCMD(uart0, tx_gpio, rx_gpio, baud_ratio, nlcode){}

pcabCMD::pcabCMD(uint tx_gpio, uint rx_gpio, uint baud_ratio)
: pcabCMD(uart0, tx_gpio, rx_gpio, baud_ratio, "\r\n"){}

pcabCMD::pcabCMD() : pcabCMD(0, 1, 9600){}

pcabCMD::~pcabCMD() {}

pcabCMD::CommandLine pcabCMD::readCMD(bool echo)
{
    uartSYNC::CommandLine cmdBF = uart.readCMD(echo);
    std::string strArr[cmdBF.argments.size()];
    std::copy(cmdBF.argments.begin(), cmdBF.argments.end(), strArr);
    if (strCompare(trim(cmdBF.command), "GetSTB.AMP", true)) { return pcabCMD::CommandLine(cmdCode::GetSTB_AMP, strArr, cmdBF.argments.size()); }
    if (strCompare(trim(cmdBF.command), "GetSTB.DRA", true)) { return pcabCMD::CommandLine(cmdCode::GetSTB_DRA, strArr, cmdBF.argments.size()); }
    if (strCompare(trim(cmdBF.command), "GetSTB.LNA", true)) { return pcabCMD::CommandLine(cmdCode::GetSTB_LNA, strArr, cmdBF.argments.size()); }
    if (strCompare(trim(cmdBF.command), "SetSTB.AMP", true)) { return pcabCMD::CommandLine(cmdCode::SetSTB_AMP, strArr, cmdBF.argments.size()); }
    if (strCompare(trim(cmdBF.command), "SetSTB.DRA", true)) { return pcabCMD::CommandLine(cmdCode::SetSTB_DRA, strArr, cmdBF.argments.size()); }
    if (strCompare(trim(cmdBF.command), "SetSTB.LNA", true)) { return pcabCMD::CommandLine(cmdCode::SetSTB_LNA, strArr, cmdBF.argments.size()); }
    if (strCompare(trim(cmdBF.command), "GetLPM", true)) { return pcabCMD::CommandLine(cmdCode::GetLPM, strArr, cmdBF.argments.size()); }
    if (strCompare(trim(cmdBF.command), "SetLPM", true)) { return pcabCMD::CommandLine(cmdCode::SetLPM, strArr, cmdBF.argments.size()); }
    if (strCompare(trim(cmdBF.command), "SetALD", true)) { return pcabCMD::CommandLine(cmdCode::SetALD, strArr, cmdBF.argments.size()); }
    if (strCompare(trim(cmdBF.command), "GetALD", true)) { return pcabCMD::CommandLine(cmdCode::GetALD, strArr, cmdBF.argments.size()); }
    if (strCompare(trim(cmdBF.command), "SetIDN", true)) { return pcabCMD::CommandLine(cmdCode::SetIDN, strArr, cmdBF.argments.size()); }
    if (strCompare(trim(cmdBF.command), "GetIDN", true)) { return pcabCMD::CommandLine(cmdCode::GetIDN, strArr, cmdBF.argments.size()); }
    if (strCompare(trim(cmdBF.command), "GetTMP", true)) { return pcabCMD::CommandLine(cmdCode::GetTMP, strArr, cmdBF.argments.size()); }    
    if (strCompare(trim(cmdBF.command), "WrtPS", true)) { return pcabCMD::CommandLine(cmdCode::WrtPS, strArr, cmdBF.argments.size()); }
    if (strCompare(trim(cmdBF.command), "GetPS", true)) { return pcabCMD::CommandLine(cmdCode::GetPS, strArr, cmdBF.argments.size()); }
    if (strCompare(trim(cmdBF.command), "GetId", true)) { return pcabCMD::CommandLine(cmdCode::GetId, strArr, cmdBF.argments.size()); }
    if (strCompare(trim(cmdBF.command), "GetVd", true)) { return pcabCMD::CommandLine(cmdCode::GetVd, strArr, cmdBF.argments.size()); }
    if (strCompare(trim(cmdBF.command), "SetPS", true)) { return pcabCMD::CommandLine(cmdCode::GetVd, strArr, cmdBF.argments.size()); }
    if (strCompare(trim(cmdBF.command), "SMEM", true)) { return pcabCMD::CommandLine(cmdCode::SaveMEM, strArr, cmdBF.argments.size()); }
    if (strCompare(trim(cmdBF.command), "LMEM", true)) { return pcabCMD::CommandLine(cmdCode::LoadMEM, strArr, cmdBF.argments.size()); }
    if (strCompare(trim(cmdBF.command), "RST", true)) { return pcabCMD::CommandLine(cmdCode::RST, strArr, cmdBF.argments.size()); }
    if (strCompare(trim(cmdBF.command), "*RST", true)) { return pcabCMD::CommandLine(cmdCode::RST, strArr, cmdBF.argments.size()); }
    if (strCompare(trim(cmdBF.command), "ECHO", true)) { return pcabCMD::CommandLine(cmdCode::CUI, strArr, cmdBF.argments.size()); }
    if (strCompare(trim(cmdBF.command), "*IDN?", true)) { return pcabCMD::CommandLine(cmdCode::CUI, strArr, cmdBF.argments.size()); }
    return pcabCMD::CommandLine(cmdCode::NONE, strArr, cmdBF.argments.size());
}

#pragma endregion pcabCMD Class
