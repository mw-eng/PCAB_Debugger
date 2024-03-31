#include "flash_library.hpp"


void saveMEMORYblock(uint32_t address, const uint8_t blockDAT[FLASH_PAGE_SIZE])
{
    const uint32_t FLASH_TARGET_OFFSET = address;
    uint32_t ints = save_and_disable_interrupts();
    flash_range_erase(FLASH_TARGET_OFFSET, FLASH_SECTOR_SIZE);
    flash_range_program(FLASH_TARGET_OFFSET, blockDAT, FLASH_PAGE_SIZE);
    restore_interrupts(ints);
}

void readMEMORYblock(uint32_t address, uint8_t blockDAT[FLASH_PAGE_SIZE])
{
    const uint32_t FLASH_TARGET_OFFSET = address;
    const uint8_t *flash_target_contents = (const uint8_t *) (XIP_BASE + FLASH_TARGET_OFFSET);
    for(int i = 0 ; i < (int)FLASH_PAGE_SIZE ; i++){
        blockDAT[i] = flash_target_contents[i];
    }
}

void eraseMEMORYblock(uint32_t address)
{
    const uint32_t FLASH_TARGET_OFFSET = address;
    uint32_t ints = save_and_disable_interrupts();
    flash_range_erase(FLASH_TARGET_OFFSET, FLASH_SECTOR_SIZE);
    restore_interrupts(ints);
}