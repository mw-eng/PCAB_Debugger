#include "hardware/flash.h"
#include "hardware/sync.h"


void saveMEMORYblock(uint32_t address, const uint8_t blockDAT[FLASH_PAGE_SIZE]);
void readMEMORYblock(uint32_t address, uint8_t blockDAT[FLASH_PAGE_SIZE]);
void eraseMEMORYblock(uint32_t address);