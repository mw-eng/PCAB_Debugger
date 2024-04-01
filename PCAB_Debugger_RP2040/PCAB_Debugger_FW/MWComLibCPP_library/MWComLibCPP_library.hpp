#include <string>
#include <cstring>
#include <vector>
#include <algorithm>

class Convert
{
    public:
    static std::string ToString(bool val, bool formatString);
    static std::string ToString(bool val);
    static std::string ToString(int64_t val, uint BaseNumber, uint digit);
    static std::string ToString(int64_t val, uint BaseNumber);

//    static bool TryToBool(std::string str, bool out);
//    static bool TryToInt(std::string str, int out);
//    static bool TryToDouble(std::string str, double out);
//    static bool TryToFloat(std::string str, float out);
//    static bool TryToUInt(std::string str, uint out);
//    static bool TryToUInt8(std::string str, uint8_t out);
//    static bool TryToUInt16(std::string str, uint16_t out);
//    static bool TryToUInt32(std::string str, uint32_t out);
    static bool TryToUInt64(std::string str, uint8_t BaseNumber, uint64_t& out);
};

/// @brief Trimming the specified std::string from the beginning(Left).
/// @param str std::string to be trimmed.
/// @param targ Specified std::string.
/// @return std::string after trimming.
std::string ltrim(const std::string &str, const std::string &targ);

/// @brief Trimming the specified std::string from the end(Right).
/// @param str std::string to be trimmed.
/// @param targ Specified std::string.
/// @return std::string after trimming.
std::string rtrim(const std::string & str, const std::string& targ);

/// @brief Trimming the beginning(Left) and end(Right) of the specified std::string.
/// @param str std::string to be trimmed.
/// @param targ Specified std::string.
/// @return std::string after trimming.
std::string trim(const std::string & str, const std::string& targ);

/// @brief Trimming the character (Space, CR, LF, Tab, 0x0c, 0x0b) from the beginning(Left).
/// @param str std::string to be trimmed.
/// @return std::string after trimming
std::string ltrim(const std::string& str);

/// @brief Trimming the character (Space, CR, LF, Tab, 0x0c, 0x0b) from the end(Right).
/// @param str std::string to be trimmed.
/// @return std::string after trimming.
std::string rtrim(const std::string& str);

/// @brief Trimming the beginning(Left) and end(Right) of the character (Space, CR, LF, Tab, 0x0c, 0x0b).
/// @param str std::string to be trimmed.
/// @return std::string after trimming.
std::string trim(const std::string& str);

/// @brief Shifts bit to the left by the specified bits.
/// @param bit Traget bits.
/// @param shift Number of shifts.
/// @return Result bit.
uint8_t uint8_bitShiftLeft(uint8_t &bit, uint8_t &shift);

/// @brief Compares two (A and B) std::strings.
/// @param a Target std::string A.
/// @param b Target std::string B.
/// @param ignoreCase Case sensitivity. (true:Compare case insensitivitely, false:Compare case sensitivively)
/// @return Compare result.
bool strCompare(const std::string &a, const std::string &b, const bool &ignoreCase);

/// @brief Compares two (A and B) std::strings.
/// @param a Target std::string A.
/// @param b Target std::string B.
/// @return Compare result.
bool strCompare(const std::string &a, const std::string &b);


/// @brief Convert string to uint
/// @param str convert string
/// @return uint
int conv_uint(const std::string &str);

/// @brief String split.
/// @param str Target string.
/// @param delim Split charactor
/// @return vector
std::vector<std::string> split(const std::string &str, char delim);
