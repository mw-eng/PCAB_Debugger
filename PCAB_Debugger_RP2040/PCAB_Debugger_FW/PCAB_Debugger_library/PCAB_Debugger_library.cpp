#include "PCAB_Debugger_library.hpp"
#include "MWComLibCPP_library.hpp"

#pragma region pcabCMD Class

pcabCMD::pcabCMD(uartSYNC uart, uint rs485de_gpio) : uart(uart), de_gpio(rs485de_gpio), de_mode(false)
{
    gpio_init(rs485de_gpio);
    gpio_set_dir(rs485de_gpio ,GPIO_OUT);
    rs485disable();
}

pcabCMD::pcabCMD(uart_inst_t *uartID, uint tx_gpio, uint rx_gpio, uint baud_ratio, uint data_bits, uint stop_bits, uart_parity_t parity, bool cts, bool rts, std::string nlcode, uint rs485de_gpio)
: uart(uartSYNC(uartID, tx_gpio, rx_gpio, baud_ratio, data_bits, stop_bits, parity, cts, rts, nlcode)), de_gpio(rs485de_gpio), de_mode(false)
{
    gpio_init(rs485de_gpio);
    gpio_set_dir(rs485de_gpio ,GPIO_OUT);
    rs485disable();
}

pcabCMD::pcabCMD(uart_inst_t *uartID, uint tx_gpio, uint rx_gpio, uint baud_ratio, std::string nlcode, uint rs485de_gpio)
: pcabCMD(uartID, tx_gpio, rx_gpio, baud_ratio, 8, 1, UART_PARITY_NONE, false, false, nlcode, rs485de_gpio){}

pcabCMD::pcabCMD(uint tx_gpio, uint rx_gpio, uint baud_ratio, std::string nlcode, uint rs485de_gpio)
: pcabCMD(uart0, tx_gpio, rx_gpio, baud_ratio, nlcode, rs485de_gpio){}

pcabCMD::pcabCMD(uint tx_gpio, uint rx_gpio, uint baud_ratio, uint rs485de_gpio)
: pcabCMD(uart0, tx_gpio, rx_gpio, baud_ratio, "\r\n", rs485de_gpio){}

pcabCMD::pcabCMD() : pcabCMD(0, 1, 9600, 2){}

pcabCMD::~pcabCMD() {}

pcabCMD::CommandLine pcabCMD::readCMD(bool echo)
{
    uartSYNC::CommandLine cmdBF = uart.readCMD(echo);
    std::string cmd = String::trim(cmdBF.command);
    if(cmd == "" && cmdBF.argments.size() == 0) { return pcabCMD::CommandLine(cmdBF.serialNum, cmdBF.romID, cmdCode::NUL, NULL, 0); }
    std::string strArr[cmdBF.argments.size()];
    std::copy(cmdBF.argments.begin(), cmdBF.argments.end(), strArr);
    if (String::strCompare(cmd, "WrtDPS", true)) { return pcabCMD::CommandLine(cmdBF.serialNum, cmdBF.romID, cmdCode::WrtDPS, strArr, cmdBF.argments.size()); }
    if (String::strCompare(cmd, "GetDPS", true)) { return pcabCMD::CommandLine(cmdBF.serialNum, cmdBF.romID, cmdCode::GetDPS, strArr, cmdBF.argments.size()); }
    if (String::strCompare(cmd, "SetDPS", true)) { return pcabCMD::CommandLine(cmdBF.serialNum, cmdBF.romID, cmdCode::SetDPS, strArr, cmdBF.argments.size()); }
    if (String::strCompare(cmd, "WrtDSA", true)) { return pcabCMD::CommandLine(cmdBF.serialNum, cmdBF.romID, cmdCode::WrtDSA, strArr, cmdBF.argments.size()); }
    if (String::strCompare(cmd, "GetDSA", true)) { return pcabCMD::CommandLine(cmdBF.serialNum, cmdBF.romID, cmdCode::GetDSA, strArr, cmdBF.argments.size()); }
    if (String::strCompare(cmd, "SetDSA", true)) { return pcabCMD::CommandLine(cmdBF.serialNum, cmdBF.romID, cmdCode::SetDSA, strArr, cmdBF.argments.size()); }
    if (String::strCompare(cmd, "GetTMP.ID", true)) { return pcabCMD::CommandLine(cmdBF.serialNum, cmdBF.romID, cmdCode::GetTMP_ID, strArr, cmdBF.argments.size()); }
    if (String::strCompare(cmd, "GetTMP.Val", true)) { return pcabCMD::CommandLine(cmdBF.serialNum, cmdBF.romID, cmdCode::GetTMP_VAL, strArr, cmdBF.argments.size()); }
    if (String::strCompare(cmd, "GetTMP.CPU", true)) { return pcabCMD::CommandLine(cmdBF.serialNum, cmdBF.romID, cmdCode::GetTMP_CPU, strArr, cmdBF.argments.size()); }
    if (String::strCompare(cmd, "GetVd", true)) { return pcabCMD::CommandLine(cmdBF.serialNum, cmdBF.romID, cmdCode::GetVd, strArr, cmdBF.argments.size()); }
    if (String::strCompare(cmd, "GetId", true)) { return pcabCMD::CommandLine(cmdBF.serialNum, cmdBF.romID, cmdCode::GetId, strArr, cmdBF.argments.size()); }
    if (String::strCompare(cmd, "GetVin", true)) { return pcabCMD::CommandLine(cmdBF.serialNum, cmdBF.romID, cmdCode::GetVin, strArr, cmdBF.argments.size()); }
    if (String::strCompare(cmd, "GetPin", true)) { return pcabCMD::CommandLine(cmdBF.serialNum, cmdBF.romID, cmdCode::GetPin, strArr, cmdBF.argments.size()); }
    if (String::strCompare(cmd, "GetSTB.AMP", true)) { return pcabCMD::CommandLine(cmdBF.serialNum, cmdBF.romID, cmdCode::GetSTB_AMP, strArr, cmdBF.argments.size()); }
    if (String::strCompare(cmd, "GetSTB.DRA", true)) { return pcabCMD::CommandLine(cmdBF.serialNum, cmdBF.romID, cmdCode::GetSTB_DRA, strArr, cmdBF.argments.size()); }
    if (String::strCompare(cmd, "GetSTB.LNA", true)) { return pcabCMD::CommandLine(cmdBF.serialNum, cmdBF.romID, cmdCode::GetSTB_LNA, strArr, cmdBF.argments.size()); }
    if (String::strCompare(cmd, "SetSTB.AMP", true)) { return pcabCMD::CommandLine(cmdBF.serialNum, cmdBF.romID, cmdCode::SetSTB_AMP, strArr, cmdBF.argments.size()); }
    if (String::strCompare(cmd, "SetSTB.DRA", true)) { return pcabCMD::CommandLine(cmdBF.serialNum, cmdBF.romID, cmdCode::SetSTB_DRA, strArr, cmdBF.argments.size()); }
    if (String::strCompare(cmd, "SetSTB.LNA", true)) { return pcabCMD::CommandLine(cmdBF.serialNum, cmdBF.romID, cmdCode::SetSTB_LNA, strArr, cmdBF.argments.size()); }
    if (String::strCompare(cmd, "GetLPM", true)) { return pcabCMD::CommandLine(cmdBF.serialNum, cmdBF.romID, cmdCode::GetLPM, strArr, cmdBF.argments.size()); }
    if (String::strCompare(cmd, "SetLPM", true)) { return pcabCMD::CommandLine(cmdBF.serialNum, cmdBF.romID, cmdCode::SetLPM, strArr, cmdBF.argments.size()); }
    if (String::strCompare(cmd, "SetALD", true)) { return pcabCMD::CommandLine(cmdBF.serialNum, cmdBF.romID, cmdCode::SetALD, strArr, cmdBF.argments.size()); }
    if (String::strCompare(cmd, "GetALD", true)) { return pcabCMD::CommandLine(cmdBF.serialNum, cmdBF.romID, cmdCode::GetALD, strArr, cmdBF.argments.size()); }
    if (String::strCompare(cmd, "SMEM", true)) { return pcabCMD::CommandLine(cmdBF.serialNum, cmdBF.romID, cmdCode::SaveMEM, strArr, cmdBF.argments.size()); }
    if (String::strCompare(cmd, "LMEM", true)) { return pcabCMD::CommandLine(cmdBF.serialNum, cmdBF.romID, cmdCode::LoadMEM, strArr, cmdBF.argments.size()); }
    if (String::strCompare(cmd, "RROM", true)) { return pcabCMD::CommandLine(cmdBF.serialNum, cmdBF.romID, cmdCode::ReadROM, strArr, cmdBF.argments.size()); }
    if (String::strCompare(cmd, "WROM", true)) { return pcabCMD::CommandLine(cmdBF.serialNum, cmdBF.romID, cmdCode::WriteROM, strArr, cmdBF.argments.size()); }
    if (String::strCompare(cmd, "OROM", true)) { return pcabCMD::CommandLine(cmdBF.serialNum, cmdBF.romID, cmdCode::OverwriteROM, strArr, cmdBF.argments.size()); }
    if (String::strCompare(cmd, "EROM", true)) { return pcabCMD::CommandLine(cmdBF.serialNum, cmdBF.romID, cmdCode::EraseROM, strArr, cmdBF.argments.size()); }
    if (String::strCompare(cmd, "SetSN", true)) { return pcabCMD::CommandLine(cmdBF.serialNum, cmdBF.romID, cmdCode::SetSN, strArr, cmdBF.argments.size()); }
    if (String::strCompare(cmd, "RST", true)) { return pcabCMD::CommandLine(cmdBF.serialNum, cmdBF.romID, cmdCode::RST, strArr, cmdBF.argments.size()); }
    if (String::strCompare(cmd, "*RST", true)) { return pcabCMD::CommandLine(cmdBF.serialNum, cmdBF.romID, cmdCode::RST, strArr, cmdBF.argments.size()); }
    if (String::strCompare(cmd, "ECHO", true)) { return pcabCMD::CommandLine(cmdBF.serialNum, cmdBF.romID, cmdCode::ECHO, strArr, cmdBF.argments.size()); }
    if (String::strCompare(cmd, "CUI", true)) { return pcabCMD::CommandLine(cmdBF.serialNum, cmdBF.romID, cmdCode::CUI, strArr, cmdBF.argments.size()); }
    if (String::strCompare(cmd, "GetMODE", true)) { return pcabCMD::CommandLine(cmdBF.serialNum, cmdBF.romID, cmdCode::GetMODE, strArr, cmdBF.argments.size()); }
    if (String::strCompare(cmd, "ReBOOT", true)) { return pcabCMD::CommandLine(cmdBF.serialNum, cmdBF.romID, cmdCode::Reboot, strArr, cmdBF.argments.size()); }
    if (String::strCompare(cmd, "GetIDN", true)) { return pcabCMD::CommandLine(cmdBF.serialNum, cmdBF.romID, cmdCode::GetIDN, strArr, cmdBF.argments.size()); }
    if (String::strCompare(cmd, "*IDN?", true)) { return pcabCMD::CommandLine(cmdBF.serialNum, cmdBF.romID, cmdCode::GetIDN, strArr, cmdBF.argments.size()); }
    if (String::strCompare(cmd, "GetIDR", true)) { return pcabCMD::CommandLine(cmdBF.serialNum, cmdBF.romID, cmdCode::GetIDR, strArr, cmdBF.argments.size()); }
    else { return pcabCMD::CommandLine(cmdBF.serialNum, cmdBF.romID, cmdCode::NONE, strArr, cmdBF.argments.size()); }
}

void pcabCMD::write(std::string str)
{
    if(!de_mode){rs485enable(); sleep_ms(10);}
    uart.write(str);
    sleep_ms(10);
    rs485disable();
}

void pcabCMD::writeLine(std::string str)
{
    if(!de_mode){rs485enable(); sleep_ms(10);}
    uart.writeLine(str);
    sleep_ms(10);
    rs485disable();
}

void pcabCMD::rs485enable()
{
    gpio_put(de_gpio, true);
    de_mode = true;
}

void pcabCMD::rs485disable()
{
    gpio_put(de_gpio, false);
    de_mode = false;
}

bool pcabCMD::getRS485mode(){ return de_mode; }

#pragma endregion pcabCMD Class
