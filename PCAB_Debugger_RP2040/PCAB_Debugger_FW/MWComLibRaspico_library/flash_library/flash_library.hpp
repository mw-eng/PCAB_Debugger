#include "hardware/flash.h"
#include "hardware/sync.h"

class flash
{
    public:
    static void getID(uint8_t id_out[FLASH_UNIQUE_ID_SIZE_BYTES]);
    static bool eraseROM(const uint32_t &address);
    static bool eraseROM(const uint16_t &blockNum, const uint8_t &sectorNum);
    static bool writeROM(const uint32_t &address, const uint8_t pageDAT[FLASH_PAGE_SIZE]);
    static bool writeROM(const uint16_t &blockNum, const uint8_t &sectorpageNum, const uint8_t pageDAT[FLASH_PAGE_SIZE]);
    static bool writeROM(const uint16_t &blockNum, const uint8_t &sectorNum, const uint8_t &pageNum, const uint8_t pageDAT[FLASH_PAGE_SIZE]);
    static bool ReadROM(const uint32_t &address, uint8_t pageDAT[FLASH_PAGE_SIZE]);
    static bool ReadROM(const uint16_t &blockNum, const uint8_t &sectorpageNum, uint8_t pageDAT[FLASH_PAGE_SIZE]);
    static bool ReadROM(const uint16_t &blockNum, const uint8_t &sectorNum, const uint8_t &pageNum, uint8_t pageDAT[FLASH_PAGE_SIZE]);
    static bool overwriteROM(const uint32_t &address, const uint8_t sectorDAT[8 * FLASH_PAGE_SIZE]);

    /// @brief 
    /// @param blockNum 
    /// @param sectorpageNum 
    /// @param sectorDAT 
    /// @return 
    static bool overwriteROM(const uint16_t &blockNum, const uint8_t &sectorpageNum, const uint8_t sectorDAT[8 * FLASH_PAGE_SIZE]);
};