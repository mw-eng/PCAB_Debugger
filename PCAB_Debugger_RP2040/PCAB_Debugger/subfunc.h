#include "enum.h"
std::string ltrim(const std::string &s);
std::string rtrim(const std::string &s);
std::string trim(const std::string &s);
uint8_t uint8_bitLeftShift(uint8_t bit, uint8_t shift);
bool strCompare(const std::string &a, const std::string &b, const bool ignoreCase);
bool strCompare(const std::string &a, const std::string &b);
int conv_uint(const std::string &str);
cmd getCMD(const std::string &strCMD);
std::string getCMDLINE(const bool retchar);