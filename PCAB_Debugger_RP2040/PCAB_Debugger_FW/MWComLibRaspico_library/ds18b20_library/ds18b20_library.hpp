#pragma once
#include <vector>
#include <string>
#include "onewire_library.h"

class ds18b20
{
    private:
    OW ow;
    std::vector<uint64_t> SENS_TMP;  //Temp Senser IDs

    public:

    /// @brief Constructor
    /// @param pioID Pio id to use.
    /// @param gpioNumber gpio number to use.
    /// @param senser_count_max  Specifies the maximum number of sensors to communicate with.
    ds18b20(PIO pioID, uint gpioNumber, uint8_t senser_count_max);

    /// @brief Constructor ( senser_count_max = 255 )
    /// @param pioID Pio id to use.
    /// @param gpioNumber gpio number to use.
    ds18b20(PIO pioID, uint gpioNumber);

    /// @brief Constructor ( senser_count_max = 255 / pioID = pio0)
    /// @param gpioNumber gpio number to use.
    ds18b20(uint gipoNumber);

    /// @brief Constructor ( senser_count_max = 255 / pioID = pio0 / gpioNumber = 0)
    ds18b20();

    /// @brief Destructor
    ~ds18b20();
    
    /// @brief Get all sensor readings.
    /// @return Senser reading data.
    std::vector<int16_t> readSENS();

    /// @brief Get the measurement value of the specified sensor.
    /// @param sensNUM Senser number.
    /// @return Senser reading data.
    int16_t readSENS(uint sensNUM);

    /// @brief Get temperature of all sensors.
    /// @return Temperature.
    std::vector<float> readTEMP();

    /// @brief Get the temperature of the specified sensor.
    /// @param sensNUM Senser number
    /// @return Temperature.
    float readTEMP(uint sensNUM);

    /// @brief Get number of sensors.
    /// @return Sensors number.
    int16_t getNumberOfSenser();

    /// @brief Get all sensor IDs.
    /// @return Senser IDs.
    std::vector<uint64_t> getSENS_ROMCODE();

    /// @brief Get specified sensor ID.
    /// @param sensNUM Senser number.
    /// @return Senser ID.
    uint64_t getSENS_ROMCODE(uint sensNUM);
    
};