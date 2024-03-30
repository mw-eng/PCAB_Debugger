#include "MWComLibRaspico_library.hpp"

std::string ltrim(const std::string &str, const std::string &targ)
{
    size_t start = str.find_first_not_of(targ);
    return (start == std::string::npos) ? "" : str.substr(start);
}

std::string rtrim(const std::string & str, const std::string& targ)
{
    size_t end = str.find_last_not_of(targ);
    return (end == std::string::npos) ? "" : str.substr(0, end + 1);
}

std::string trim(const std::string & str, const std::string& targ) { return rtrim(ltrim(str, targ), targ); }

std::string ltrim(const std::string& str) { return ltrim(str, " \n\r\t\f\v"); }

std::string rtrim(const std::string& str) { return rtrim(str, " \n\r\t\f\v"); }

std::string trim(const std::string& str) { return trim(str, " \n\r\t\f\v"); }

uint8_t uint8_bitShiftLeft(const uint8_t &bit, const uint8_t &shift)
{
    unsigned char chBF = bit;
    return (uint8_t)(chBF << shift);
}

bool strCompare(const std::string &a, const std::string &b, const bool &ignoreCase)
{
    if(a.length() <= 0 && b.length() <= 0){return true;}
    if (ignoreCase)
    {
        return strcasecmp(a.c_str(), b.c_str()) == 0 ? true : false;
    }
    else{ return a == b ? true : false; }
}

bool strCompare(const std::string &a, const std::string &b) { return strCompare(a, b, false); }

int conv_uint(const std::string &str)
{
    if(str.length() <= 0){return -1;}
    if (std::all_of(str.cbegin(), str.cend(), isdigit)) { return stoi(str); }
    return -1;
}

std::vector<std::string> split(const std::string &str, char delim) {
    std::vector<std::string> elems;
    std::string item;
    for (char ch: str) {
        if (ch == delim) {
            if (!item.empty())
                elems.push_back(item);
            item.clear();
        }
        else {
            item += ch;
        }
    }
    if (!item.empty())
        elems.push_back(item);
    return elems;
}