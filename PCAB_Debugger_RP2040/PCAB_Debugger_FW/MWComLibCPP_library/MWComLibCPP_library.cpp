#include "MWComLibCPP_library.hpp"

uint64_t pow64(uint8_t x, uint y)
{
    uint64_t ret = 1;
    for(uint i = 0 ; i < y ; i++ ){ ret *= x;}
    return ret;
}

std::string Convert::ToString(bool val, bool formatString)
{
    if(formatString && val){ return "true"; }
    if(formatString && !val){ return "false"; }
    if(!formatString && val){ return "1"; }
    return "0";
}
std::string Convert::ToString(bool val) { return ToString(val, false); }
std::string Convert::ToString(int64_t val, uint BaseNumber, uint digit)
{
    int64_t ulngBF = val;
    std::string strBf = "";
    std::string str = "";
    while(ulngBF)
    {
        if(ulngBF % BaseNumber < 10) { strBf.push_back('0' + ulngBF % BaseNumber); }
        else { strBf.push_back('0' + ulngBF % BaseNumber + 7); }
        ulngBF /= BaseNumber;
    }
    if(strBf.length() < digit)
    {
        for(int i = 0 ; i < digit - strBf.length() ; i++ ) { str.push_back('0'); }
    }
    for(int i = strBf.length() ; 0 < i ; i-- ){ str.push_back(strBf[i - 1]); }
    return str;
}
std::string Convert::ToString(int64_t val, uint BaseNumber){ return ToString(val, BaseNumber, 0);}

//bool Convert::TryToBool(std::string str, bool out){}
//bool Convert::TryToInt(std::string str, int out){}
//bool Convert::TryToDouble(std::string str, double out){}
//bool Convert::TryToFloat(std::string str, float out){}
//bool Convert::TryToUInt(std::string str, uint out){}
//bool Convert::TryToUInt8(std::string str, uint8_t out){}
//bool Convert::TryToUInt16(std::string str, uint16_t out){}
//bool Convert::TryToUInt32(std::string str, uint32_t out){}
bool Convert::TryToUInt64(std::string str, uint8_t BaseNumber, uint64_t& out)
{
    out = 0;
    if(BaseNumber > 36){ return false; }
    for(uint i = 0 ; i < str.length() ; i++ )
    {
        uint8_t uiBF = str[i];
        if(48 <= uiBF && uiBF <= 57) { out += (uiBF - 48) * pow64(BaseNumber, str.length() - i - 1); }
        else if(65 <= uiBF && uiBF <= 90) { out += (uiBF - 55) * pow64(BaseNumber, str.length() - i - 1); }
        else if(97 <= uiBF && uiBF <= 122) { out += (uiBF - 87) * pow64(BaseNumber, str.length() - i - 1); }
        else { return false; }
    }
    return true;
}


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
