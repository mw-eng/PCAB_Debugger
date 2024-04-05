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
: pcabCMD(uart0, tx_gpio, rx_gpio, baud_ratio, "\n"){}

pcabCMD::pcabCMD() : pcabCMD(0, 1, 9600){}

pcabCMD::~pcabCMD() {}

pcabCMD::CommandLine pcabCMD::readCMD(bool echo)
{
    uartSYNC::CommandLine cmdBF = uart.readCMD(echo);
    std::string cmd = String::trim(cmdBF.command);
    if(cmd == "" && cmdBF.argments.size() == 0) { return pcabCMD::CommandLine(cmdBF.serialNum, cmdCode::NUL, NULL, 0); }
    std::string strArr[cmdBF.argments.size()];
    std::copy(cmdBF.argments.begin(), cmdBF.argments.end(), strArr);
    if (String::strCompare(cmd, "WrtDPS", true)) { return pcabCMD::CommandLine(cmdBF.serialNum, cmdCode::WrtDPS, strArr, cmdBF.argments.size()); }
    if (String::strCompare(cmd, "GetDPS", true)) { return pcabCMD::CommandLine(cmdBF.serialNum, cmdCode::GetDPS, strArr, cmdBF.argments.size()); }
    if (String::strCompare(cmd, "SetDPS", true)) { return pcabCMD::CommandLine(cmdBF.serialNum, cmdCode::SetDPS, strArr, cmdBF.argments.size()); }
    if (String::strCompare(cmd, "WrtDSA", true)) { return pcabCMD::CommandLine(cmdBF.serialNum, cmdCode::WrtDSA, strArr, cmdBF.argments.size()); }
    if (String::strCompare(cmd, "GetDSA", true)) { return pcabCMD::CommandLine(cmdBF.serialNum, cmdCode::GetDSA, strArr, cmdBF.argments.size()); }
    if (String::strCompare(cmd, "SetDSA", true)) { return pcabCMD::CommandLine(cmdBF.serialNum, cmdCode::SetDSA, strArr, cmdBF.argments.size()); }
    if (String::strCompare(cmd, "GetTMP.ID", true)) { return pcabCMD::CommandLine(cmdBF.serialNum, cmdCode::GetTMP_ID, strArr, cmdBF.argments.size()); }
    if (String::strCompare(cmd, "GetTMP.Val", true)) { return pcabCMD::CommandLine(cmdBF.serialNum, cmdCode::GetTMP_VAL, strArr, cmdBF.argments.size()); }
    if (String::strCompare(cmd, "GetTMP.CPU", true)) { return pcabCMD::CommandLine(cmdBF.serialNum, cmdCode::GetTMP_CPU, strArr, cmdBF.argments.size()); }
    if (String::strCompare(cmd, "GetVd", true)) { return pcabCMD::CommandLine(cmdBF.serialNum, cmdCode::GetVd, strArr, cmdBF.argments.size()); }
    if (String::strCompare(cmd, "GetId", true)) { return pcabCMD::CommandLine(cmdBF.serialNum, cmdCode::GetId, strArr, cmdBF.argments.size()); }
    if (String::strCompare(cmd, "GetVin", true)) { return pcabCMD::CommandLine(cmdBF.serialNum, cmdCode::GetVin, strArr, cmdBF.argments.size()); }
    if (String::strCompare(cmd, "GetSTB.AMP", true)) { return pcabCMD::CommandLine(cmdBF.serialNum, cmdCode::GetSTB_AMP, strArr, cmdBF.argments.size()); }
    if (String::strCompare(cmd, "GetSTB.DRA", true)) { return pcabCMD::CommandLine(cmdBF.serialNum, cmdCode::GetSTB_DRA, strArr, cmdBF.argments.size()); }
    if (String::strCompare(cmd, "GetSTB.LNA", true)) { return pcabCMD::CommandLine(cmdBF.serialNum, cmdCode::GetSTB_LNA, strArr, cmdBF.argments.size()); }
    if (String::strCompare(cmd, "SetSTB.AMP", true)) { return pcabCMD::CommandLine(cmdBF.serialNum, cmdCode::SetSTB_AMP, strArr, cmdBF.argments.size()); }
    if (String::strCompare(cmd, "SetSTB.DRA", true)) { return pcabCMD::CommandLine(cmdBF.serialNum, cmdCode::SetSTB_DRA, strArr, cmdBF.argments.size()); }
    if (String::strCompare(cmd, "SetSTB.LNA", true)) { return pcabCMD::CommandLine(cmdBF.serialNum, cmdCode::SetSTB_LNA, strArr, cmdBF.argments.size()); }
    if (String::strCompare(cmd, "GetLPM", true)) { return pcabCMD::CommandLine(cmdBF.serialNum, cmdCode::GetLPM, strArr, cmdBF.argments.size()); }
    if (String::strCompare(cmd, "SetLPM", true)) { return pcabCMD::CommandLine(cmdBF.serialNum, cmdCode::SetLPM, strArr, cmdBF.argments.size()); }
    if (String::strCompare(cmd, "SetALD", true)) { return pcabCMD::CommandLine(cmdBF.serialNum, cmdCode::SetALD, strArr, cmdBF.argments.size()); }
    if (String::strCompare(cmd, "GetALD", true)) { return pcabCMD::CommandLine(cmdBF.serialNum, cmdCode::GetALD, strArr, cmdBF.argments.size()); }
    if (String::strCompare(cmd, "SMEM", true)) { return pcabCMD::CommandLine(cmdBF.serialNum, cmdCode::SaveMEM, strArr, cmdBF.argments.size()); }
    if (String::strCompare(cmd, "LMEM", true)) { return pcabCMD::CommandLine(cmdBF.serialNum, cmdCode::LoadMEM, strArr, cmdBF.argments.size()); }
    if (String::strCompare(cmd, "RROM", true)) { return pcabCMD::CommandLine(cmdBF.serialNum, cmdCode::ReadROM, strArr, cmdBF.argments.size()); }
    if (String::strCompare(cmd, "WROM", true)) { return pcabCMD::CommandLine(cmdBF.serialNum, cmdCode::WriteROM, strArr, cmdBF.argments.size()); }
    if (String::strCompare(cmd, "EROM", true)) { return pcabCMD::CommandLine(cmdBF.serialNum, cmdCode::EraseROM, strArr, cmdBF.argments.size()); }
    if (String::strCompare(cmd, "SetSN", true)) { return pcabCMD::CommandLine(cmdBF.serialNum, cmdCode::SetSN, strArr, cmdBF.argments.size()); }
    if (String::strCompare(cmd, "RST", true)) { return pcabCMD::CommandLine(cmdBF.serialNum, cmdCode::RST, strArr, cmdBF.argments.size()); }
    if (String::strCompare(cmd, "*RST", true)) { return pcabCMD::CommandLine(cmdBF.serialNum, cmdCode::RST, strArr, cmdBF.argments.size()); }
    if (String::strCompare(cmd, "ECHO", true)) { return pcabCMD::CommandLine(cmdBF.serialNum, cmdCode::ECHO, strArr, cmdBF.argments.size()); }
    if (String::strCompare(cmd, "CUI", true)) { return pcabCMD::CommandLine(cmdBF.serialNum, cmdCode::CUI, strArr, cmdBF.argments.size()); }
    if (String::strCompare(cmd, "GetIDN", true)) { return pcabCMD::CommandLine(cmdBF.serialNum, cmdCode::GetIDN, strArr, cmdBF.argments.size()); }
    if (String::strCompare(cmd, "*IDN?", true)) { return pcabCMD::CommandLine(cmdBF.serialNum, cmdCode::GetIDN, strArr, cmdBF.argments.size()); }
    else { return pcabCMD::CommandLine(cmdBF.serialNum, cmdCode::NONE, strArr, cmdBF.argments.size()); }
}

#pragma endregion pcabCMD Class
