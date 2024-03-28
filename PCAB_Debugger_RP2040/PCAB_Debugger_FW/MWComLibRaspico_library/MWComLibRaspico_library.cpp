#include "MWComLibRaspico_library.h"

/// <summary>Trimming the specified string from the beginning(Left).</summary>
/// <param name="str">String to be trimmed.</param>
/// <param name="targ">Specified string.</param>
/// <returns>String after trimming</returns>
std::string ltrim(const std::string &str, const std::string &targ)
{
    size_t start = str.find_first_not_of(targ);
    return (start == std::string::npos) ? "" : str.substr(start);
}

/// <summary>Trimming the specified string from the end(Right).</summary>
/// <param name="str">String to be trimmed.</param>
/// <param name="targ">Specified string.</param>
/// <returns>String after trimming</returns>
std::string rtrim(const std::string & str, const std::string& targ)
{
    size_t end = str.find_last_not_of(targ);
    return (end == std::string::npos) ? "" : str.substr(0, end + 1);
}

/// <summary>Trimming the specified string from the beginning(Left).</summary>
/// <param name="str">String to be trimmed.</param>
/// <param name="targ">Specified string.</param>
/// <returns>String after trimming</returns>
std::string trim(const std::string & str, const std::string& targ) { return rtrim(ltrim(str, targ), targ); }

std::string ltrim(const std::string& str) { return ltrim(str, " \n\r\t\f\v"); }
std::string rtrim(const std::string& str) { return rtrim(str, " \n\r\t\f\v"); }
std::string trim(const std::string& str) { return trim(str, " \n\r\t\f\v"); }

uint8_t uint8_bitLeftShift(const uint8_t &bit, const uint8_t &shift)
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