#include <vector>
#include <string>
#include "onewire_library.h"

class ds18b20
{
    private:
    OW ow;
    std::vector<uint64_t> SENS_TMP;  //Temp Senser IDs

    public:

    /// @brief 
    /// @param pioID 
    /// @param gpioNumber 
    ds18b20(PIO pioID, uint gpioNumber);

    /// @brief 
    /// @param gipoNumber 
    ds18b20(uint gipoNumber);

    /// @brief 
    ds18b20();

    /// @brief 
    ~ds18b20();
    
    /// @brief 
    /// @return 
    std::vector<int16_t> readSENS();

    /// @brief 
    /// @param sensNUM 
    /// @return 
    int16_t readSENS(uint sensNUM);

    /// @brief 
    /// @return 
    std::vector<std::string> readTEMP();

    /// @brief 
    /// @param sensNUM 
    /// @return 
    double readTEMP(uint sensNUM);

    /// @brief 
    /// @return 
    int16_t getNumberOfSenser();

    /// @brief 
    /// @return 
    std::vector<uint64_t> getSENS_ROMCODE();

    /// @brief 
    /// @param sensNUM 
    /// @return 
    uint64_t getSENS_ROMCODE(uint sensNUM);
    
};