#include "hardware/flash.h"
#include "hardware/sync.h"


void saveROMblock(uint32_t address, const uint8_t blockDAT[FLASH_PAGE_SIZE]);
void readROMblock(uint32_t address, uint8_t blockDAT[FLASH_PAGE_SIZE]);
void eraseROMblock(uint32_t address);