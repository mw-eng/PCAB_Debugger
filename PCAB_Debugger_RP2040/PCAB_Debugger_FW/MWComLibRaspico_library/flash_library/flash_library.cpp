#include "flash_library.hpp"

void flash::getID(uint8_t id_out[FLASH_UNIQUE_ID_SIZE_BYTES]) { flash_get_unique_id(id_out); }

bool flash::eraseROM(const uint32_t &address)
{
    if((address & (FLASH_SECTOR_SIZE - 1)) != 0) { return false; }
    if(PICO_FLASH_SIZE_BYTES < address) { return false; }
    uint32_t ints = save_and_disable_interrupts();
    flash_range_erase(address, FLASH_SECTOR_SIZE);
    restore_interrupts(ints);
    return true;
}
bool flash::eraseROM(const uint16_t &blockNum, const uint8_t &sectorNum)
{
    if(0x10u < sectorNum) { return false; }
    return flash::eraseROM(blockNum * FLASH_BLOCK_SIZE + sectorNum * FLASH_SECTOR_SIZE);
}

bool flash::writeROM(const uint32_t &address, const uint8_t pageDAT[FLASH_PAGE_SIZE])
{
    if((address & (FLASH_PAGE_SIZE - 1)) != 0) { return false; }
    if(PICO_FLASH_SIZE_BYTES < address) { return false; }
    uint32_t ints = save_and_disable_interrupts();
    flash_range_program(address, pageDAT, FLASH_PAGE_SIZE);
    restore_interrupts(ints);
    return true;
}
bool flash::writeROM(const uint16_t &blockNum, const uint8_t &sectorpageNum, const uint8_t pageDAT[FLASH_PAGE_SIZE])
{return flash::writeROM(blockNum * FLASH_BLOCK_SIZE + sectorpageNum * FLASH_PAGE_SIZE, pageDAT);}
bool flash::writeROM(const uint16_t &blockNum, const uint8_t &sectorNum, const uint8_t &pageNum, const uint8_t pageDAT[FLASH_PAGE_SIZE])
{
    if(0x10u < sectorNum || 0x10u < pageNum) { return false; }
    return flash::writeROM(blockNum * FLASH_BLOCK_SIZE + sectorNum * FLASH_SECTOR_SIZE + pageNum * FLASH_PAGE_SIZE, pageDAT);
}

bool flash::ReadROM(const uint32_t &address, uint8_t pageDAT[FLASH_PAGE_SIZE])
{
    if((address & (FLASH_PAGE_SIZE - 1)) != 0) { return false; }
    if(PICO_FLASH_SIZE_BYTES < address) { return false; }
    const uint8_t *flash_target_contents = (const uint8_t *) (XIP_BASE + address);
    for(int i = 0 ; i < (int)FLASH_PAGE_SIZE ; i++){pageDAT[i] = flash_target_contents[i];}
    return true;
}
bool flash::ReadROM(const uint16_t &blockNum, const uint8_t &sectorpageNum, uint8_t pageDAT[FLASH_PAGE_SIZE])
{return flash::ReadROM(blockNum * FLASH_BLOCK_SIZE + sectorpageNum * FLASH_PAGE_SIZE, pageDAT);}
bool flash::ReadROM(const uint16_t &blockNum, const uint8_t &sectorNum, const uint8_t &pageNum, uint8_t pageDAT[FLASH_PAGE_SIZE])
{
    if(0x10u < sectorNum || 0x10u < pageNum) { return false; }
    return flash::ReadROM(blockNum * FLASH_BLOCK_SIZE + sectorNum * FLASH_SECTOR_SIZE + pageNum * FLASH_PAGE_SIZE, pageDAT);
}

bool flash::overwriteROM(const uint32_t &address, const uint8_t sectorDAT[FLASH_SECTOR_SIZE])
{
    if((address & (FLASH_SECTOR_SIZE - 1)) != 0) { return false; }
    if(PICO_FLASH_SIZE_BYTES < address) { return false; }
    uint32_t ints = save_and_disable_interrupts();
    flash_range_erase(address, FLASH_SECTOR_SIZE);
    for(uint i = 0 ; i < FLASH_SECTOR_SIZE ; i+= FLASH_PAGE_SIZE)
    { flash_range_program(address, &sectorDAT[i], FLASH_PAGE_SIZE); }
    restore_interrupts(ints);
    return true;
}
bool flash::overwriteROM(const uint16_t &blockNum, const uint8_t &sectorNum, const uint8_t sectorDAT[FLASH_SECTOR_SIZE])
{
    if(0x10u < sectorNum) { return false; }
    return flash::overwriteROM(blockNum * FLASH_BLOCK_SIZE + sectorNum * FLASH_SECTOR_SIZE, sectorDAT);
}
