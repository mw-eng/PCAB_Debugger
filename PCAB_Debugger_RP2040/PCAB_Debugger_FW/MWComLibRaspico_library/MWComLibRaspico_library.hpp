#include <string>
#include <cstring>

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