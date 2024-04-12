#include "flash_library.hpp"

void flash::getID(uint8_t id_out[FLASH_UNIQUE_ID_SIZE_BYTES]) { flash_get_unique_id(id_out); }

bool flash::eraseROM(const uint32_t &address)
{
    if((address & (FLASH_SECTOR_SIZE - 1)) != 0) { return false; }
    if(PICO_FLASH_SIZE_BYTES < address + FLASH_SECTOR_SIZE) { return false; }
    uint32_t ints = save_and_disable_interrupts();
    flash_range_erase(address, FLASH_SECTOR_SIZE);
    restore_interrupts(ints);
    return true;
}
bool flash::eraseROM(const uint16_t &blockNum, const uint8_t &sectorNum)
{
    if(0x0Fu < sectorNum) { return false; }
    return flash::eraseROM(blockNum * FLASH_BLOCK_SIZE + sectorNum * FLASH_SECTOR_SIZE);
}

bool flash::writeROM(const uint32_t &address, const uint8_t pageDAT[FLASH_PAGE_SIZE])
{
    if((address & (FLASH_PAGE_SIZE - 1)) != 0) { return false; }
    if(PICO_FLASH_SIZE_BYTES < address + FLASH_PAGE_SIZE) { return false; }
    uint32_t ints = save_and_disable_interrupts();
    flash_range_program(address, pageDAT, FLASH_PAGE_SIZE);
    restore_interrupts(ints);
    return true;
}
bool flash::writeROM(const uint16_t &blockNum, const uint8_t &sectorpageNum, const uint8_t pageDAT[FLASH_PAGE_SIZE])
{return flash::writeROM(blockNum * FLASH_BLOCK_SIZE + sectorpageNum * FLASH_PAGE_SIZE, pageDAT);}
bool flash::writeROM(const uint16_t &blockNum, const uint8_t &sectorNum, const uint8_t &pageNum, const uint8_t pageDAT[FLASH_PAGE_SIZE])
{
    if(0x0Fu < sectorNum || 0x0Fu < pageNum) { return false; }
    return flash::writeROM(blockNum * FLASH_BLOCK_SIZE + sectorNum * FLASH_SECTOR_SIZE + pageNum * FLASH_PAGE_SIZE, pageDAT);
}

bool flash::readROM(const uint32_t &address, uint8_t pageDAT[FLASH_PAGE_SIZE])
{
    if((address & (FLASH_PAGE_SIZE - 1)) != 0) { return false; }
    if(PICO_FLASH_SIZE_BYTES < address + FLASH_PAGE_SIZE) { return false; }
    const uint8_t *flash_target_contents = (const uint8_t *) (XIP_BASE + address);
    for(uint i = 0 ; i < FLASH_PAGE_SIZE ; i++){pageDAT[i] = flash_target_contents[i];}
    return true;
}
bool flash::readROM(const uint16_t &blockNum, const uint8_t &sectorpageNum, uint8_t pageDAT[FLASH_PAGE_SIZE])
{return flash::readROM(blockNum * FLASH_BLOCK_SIZE + sectorpageNum * FLASH_PAGE_SIZE, pageDAT);}
bool flash::readROM(const uint16_t &blockNum, const uint8_t &sectorNum, const uint8_t &pageNum, uint8_t pageDAT[FLASH_PAGE_SIZE])
{
    if(0x0Fu < sectorNum || 0x0Fu < pageNum) { return false; }
    return flash::readROM(blockNum * FLASH_BLOCK_SIZE + sectorNum * FLASH_SECTOR_SIZE + pageNum * FLASH_PAGE_SIZE, pageDAT);
}


bool flash::overwriteROM(const uint8_t sectorDAT[PICO_FLASH_SIZE_BYTES], const uint32_t &address, const size_t &count)
{
    if((address & (FLASH_SECTOR_SIZE - 1)) != 0) { return false; }
    if(PICO_FLASH_SIZE_BYTES < address + count) { return false; }
    if(count % FLASH_SECTOR_SIZE != 0) { return false; }
    uint32_t ints = save_and_disable_interrupts();
    flash_range_erase(address, count);
    flash_range_program(address, sectorDAT, count);
    restore_interrupts(ints);
    return true;
}
bool flash::overwriteROM(const uint8_t sectorDAT[PICO_FLASH_SIZE_BYTES], const uint16_t &blockNum, const uint8_t &sectorNum, const size_t &count)
{
    if(0x0Fu < sectorNum) { return false; }
    return flash::overwriteROM(sectorDAT, blockNum * FLASH_BLOCK_SIZE + sectorNum * FLASH_SECTOR_SIZE, count);
}

bool flash::overwriteROMsector(const uint32_t &address, const uint8_t sectorDAT[FLASH_SECTOR_SIZE])
{
    if((address & (FLASH_SECTOR_SIZE - 1)) != 0) { return false; }
    if(PICO_FLASH_SIZE_BYTES < address + FLASH_SECTOR_SIZE) { return false; }
    uint32_t ints = save_and_disable_interrupts();
    flash_range_erase(address, FLASH_SECTOR_SIZE);
    for(uint i = 0; i < FLASH_SECTOR_SIZE / FLASH_PAGE_SIZE; i++)
    { flash_range_program(address + i * FLASH_PAGE_SIZE, sectorDAT + i * FLASH_PAGE_SIZE, FLASH_PAGE_SIZE); }
    restore_interrupts(ints);
    return true;
}
bool flash::overwriteROMsector(const uint16_t &blockNum, const uint8_t &sectorNum, const uint8_t sectorDAT[FLASH_SECTOR_SIZE])
{
    if(0x0Fu < sectorNum) { return false; }
    return flash::overwriteROMsector(blockNum * FLASH_BLOCK_SIZE + sectorNum * FLASH_SECTOR_SIZE, sectorDAT);
}

bool flash::overwriteROMpage(const uint32_t &address, const uint8_t pageDAT[FLASH_PAGE_SIZE])
{
    if((address & (FLASH_PAGE_SIZE - 1)) != 0) { return false; }
    if(PICO_FLASH_SIZE_BYTES < address + FLASH_PAGE_SIZE) { return false; }
    uint32_t sectorAddr = address - address % FLASH_SECTOR_SIZE;
    uint8_t pageBF[FLASH_PAGE_SIZE];
    uint8_t sectBF[FLASH_SECTOR_SIZE];
    for(uint i = 0; i < FLASH_SECTOR_SIZE / FLASH_PAGE_SIZE; i++)
    {
        flash::readROM(sectorAddr + i * FLASH_PAGE_SIZE, pageBF);
        for(uint j = 0; j < FLASH_PAGE_SIZE; j++) { sectBF[i * FLASH_PAGE_SIZE + j] = pageBF[j]; }
    }
    for(uint i = 0; i < FLASH_PAGE_SIZE; i++) { sectBF[address % FLASH_SECTOR_SIZE + i] = pageDAT[i]; }
    flash::overwriteROMsector(sectorAddr, sectBF);
    return true;
}
bool flash::overwriteROMpage(const uint16_t &blockNum, const uint8_t &sectorpageNum, const uint8_t pageDAT[FLASH_PAGE_SIZE])
{return flash::overwriteROMpage(blockNum * FLASH_BLOCK_SIZE + sectorpageNum * FLASH_PAGE_SIZE, pageDAT);}
bool flash::overwriteROMpage(const uint16_t &blockNum, const uint8_t &sectorNum, const uint8_t &pageNum, const uint8_t pageDAT[FLASH_PAGE_SIZE])
{
    if(0x0Fu < sectorNum || 0x0Fu < pageNum) { return false; }
    return flash::overwriteROMpage(blockNum * FLASH_BLOCK_SIZE + sectorNum * FLASH_SECTOR_SIZE + pageNum * FLASH_PAGE_SIZE, pageDAT);
}