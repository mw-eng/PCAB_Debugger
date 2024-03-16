#include <string>
#include <cstring>
#include <iostream>
#include <algorithm>
#include <sstream>
#include <vector>
#include "hardware/uart.h"
#include "enum.h"
#include "config.h"

const std::string WHITESPACE = " \n\r\t\f\v";

std::string ltrim(const std::string &s)
{
    size_t start = s.find_first_not_of(WHITESPACE);
    return (start == std::string::npos) ? "" : s.substr(start);
}

std::string rtrim(const std::string &s)
{
    size_t end = s.find_last_not_of(WHITESPACE);
    return (end == std::string::npos) ? "" : s.substr(0, end + 1);
}

std::string trim(const std::string &s) { return rtrim(ltrim(s)); }

uint8_t uint8_bitLeftShift(uint8_t bit, uint8_t shift)
{
    unsigned char chBF = bit;
    return (uint8_t)(chBF << shift);
}

bool strCompare(const std::string a, const std::string b, const bool ignoreCase)
{
    if (ignoreCase)
    {
        return strcasecmp(a.c_str(), b.c_str()) == 0 ? true : false;
    }
    else{ return a == b ? true : false; }
}

bool strCompare(const std::string a, const std::string b) { return strCompare(a, b, false); }

int conv_uint(const std::string &str)
{
    if (std::all_of(str.cbegin(), str.cend(), isdigit)) { return stoi(str); }
    return -1;
}

std::string getCMDLINE(const bool retchar)
{
    char chBF;
    int cnt = 0;
    std::string cmdline = "";
    do
    {
        chBF = uart_getc(UART_ID);
        if(retchar){uart_puts(UART_ID, (std::string() + chBF).c_str());}
        cmdline += chBF;
        cnt++;
        if (cnt >= CMD_MAX)
        {
            cmdline = "";
            break;
        }
    } while (chBF && chBF != '\n');
    return cmdline;
}

cmd getCMD(const std::string &strCMD)
{
    if (strCompare(trim(strCMD).substr(0, 5), "WrtPS", true)) { return cmd(code::WrtPS, -1, -1); }
    if (strCompare(trim(strCMD).substr(0, 5), "GetPS", true)) { return cmd(code::GetPS, conv_uint(trim(strCMD).substr(5)), -1); }
    if (strCompare(trim(strCMD).substr(0, 6), "GetTMP", true)) { return cmd(code::GetTMP, conv_uint(trim(strCMD).substr(6)), -1); }
    if (strCompare(trim(strCMD).substr(0, 5), "GetId", true)) { return cmd(code::GetId, conv_uint(trim(strCMD).substr(5)), -1); }
    if (strCompare(trim(strCMD).substr(0, 5), "GetVd", true)) { return cmd(code::GetVd, conv_uint(trim(strCMD).substr(5)), -1); }
    if (strCompare(trim(strCMD).substr(0, 10), "GetSTB.AMP", true)) { return cmd(code::GetSTB_AMP, -1, -1); }
    if (strCompare(trim(strCMD).substr(0, 10), "GetSTB.DRA", true)) { return cmd(code::GetSTB_DRA, -1, -1); }
    if (strCompare(trim(strCMD).substr(0, 10), "GetSTB.LNA", true)) { return cmd(code::GetSTB_LNA, -1, -1); }
    if (strCompare(trim(strCMD).substr(0, 6), "GetLPM", true)) { return cmd(code::GetLPM, -1, -1); }
    if (strCompare(trim(strCMD).substr(0, 5), "SetPS", true))
    {
        std::vector<std::string> strVCT;
        std::string strBF;
        std::stringstream ss{trim(strCMD).substr(5)};
        while (getline(ss, strBF, ' ')) { strVCT.push_back(strBF); }
        if (strVCT.size() == 2) { return cmd(code::SetPS, conv_uint(strVCT[0]), conv_uint(strVCT[1])); }
        else { return cmd(code::SetPS, -1, -1); }
    }
    if (strCompare(trim(strCMD).substr(0, 10), "SetSTB.AMP", true)) { return cmd(code::SetSTB_AMP, -1, conv_uint(trim(strCMD).substr(11))); }
    if (strCompare(trim(strCMD).substr(0, 10), "SetSTB.DRA", true)) { return cmd(code::SetSTB_DRA, -1, conv_uint(trim(strCMD).substr(11))); }
    if (strCompare(trim(strCMD).substr(0, 10), "SetSTB.LNA", true)) { return cmd(code::SetSTB_LNA, -1, conv_uint(trim(strCMD).substr(11))); }
    if (strCompare(trim(strCMD).substr(0, 6), "SetLPM", true)) { return cmd(code::SetLPM, -1, conv_uint(trim(strCMD).substr(7))); }
    if (strCompare(trim(strCMD).substr(0, 6), "SetALD", true)) { return cmd(code::SetALD, -1, conv_uint(trim(strCMD).substr(7))); }
    if (strCompare(trim(strCMD).substr(0, 6), "GetALD", true)) { return cmd(code::GetALD, -1, -1); }
    if (strCompare(trim(strCMD).substr(0, 4), "SMEM", true)) { return cmd(code::SaveMEM, -1, -1); }
    if (strCompare(trim(strCMD).substr(0, 4), "LMEM", true)) { return cmd(code::LoadMEM, -1, -1); }
    if (strCompare(trim(strCMD).substr(0, 3), "RST", true)) { return cmd(code::RST, -1, -1); }
    if (strCompare(trim(strCMD).substr(0, 3), "CUI", true)) { return cmd(code::CUI, -1, conv_uint(trim(strCMD).substr(4))); }
    return cmd(code::NONE, -1, -1);
}