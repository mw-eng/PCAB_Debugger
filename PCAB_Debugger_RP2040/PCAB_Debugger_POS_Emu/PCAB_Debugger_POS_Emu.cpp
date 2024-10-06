#define WRITE_MODE
//#define OUTPUT_MODE

#include "PCAB_Debugger_POS_Emu.hpp"

int main()
{
    uartSYNC uart = uartSYNC(uart0, UART_TX_PIN, UART_RX_PIN, UART_BAUD_RATE, "\r\n");

    std::vector<uint8_t> dat;
    uint8_t count = 0;
    uint16_t blockNum = PICO_FLASH_SIZE_BYTES / FLASH_BLOCK_SIZE - WRITE_BLOCK - 1;
    uint8_t sectorNum = 0;
    uint8_t pageNum = 0;
    uint8_t readBF[FLASH_PAGE_SIZE];
    

    #ifdef WRITE_MODE
    sleep_ms(OUTPUT_INTERVAL);
    while(uart_is_readable(uart0)){ uart_read_blocking(uart0, readBF, 1); }
    sleep_ms(OUTPUT_INTERVAL);
    while (1)
    {
        if(blockNum < PICO_FLASH_SIZE_BYTES / FLASH_BLOCK_SIZE - 1)
        {
            for(sectorNum = 0; sectorNum < FLASH_BLOCK_SIZE / FLASH_SECTOR_SIZE; sectorNum ++)
            {
                flash::eraseROM(blockNum,sectorNum);
                for(pageNum = 0; pageNum < FLASH_SECTOR_SIZE / FLASH_PAGE_SIZE; pageNum++)
                {
                    std::vector<uint8_t> dat = uart.read(false,FLASH_PAGE_SIZE);
                    flash::writeROM(blockNum, sectorNum, pageNum, dat.data());
                }
                count++;
                readBF[0] = count;
                flash::eraseROM(PICO_FLASH_SIZE_BYTES / FLASH_BLOCK_SIZE - 1, 0);
                flash::writeROM(PICO_FLASH_SIZE_BYTES / FLASH_BLOCK_SIZE - 1, 0, 0, readBF);
            }
            blockNum++;
        }
    }
    #endif
    #ifdef OUTPUT_MODE
    flash::readROM(PICO_FLASH_SIZE_BYTES / FLASH_BLOCK_SIZE - 1, 0, 0, readBF);
    uint8_t countMAX = readBF[0];
    while (1)
    {
        count = 0;
        blockNum = PICO_FLASH_SIZE_BYTES / FLASH_BLOCK_SIZE - WRITE_BLOCK - 1;
        while (count < countMAX && blockNum < PICO_FLASH_SIZE_BYTES / FLASH_BLOCK_SIZE - 1)
        {
            sectorNum = 0;
            while (sectorNum < FLASH_BLOCK_SIZE / FLASH_SECTOR_SIZE && count < countMAX)
            {
                for(pageNum = 0; pageNum < FLASH_SECTOR_SIZE / FLASH_PAGE_SIZE; pageNum++)
                {
                    flash::readROM(blockNum, sectorNum, pageNum, readBF);
                    dat.insert(dat.end(), readBF, readBF + FLASH_PAGE_SIZE);
                }
                for(uint32_t txCNT = 0; txCNT < dat.size(); txCNT += OUTPUT_BYTE)
                {
                    std::vector<uint8_t> txBF(dat.begin() + txCNT, dat.begin() + txCNT + OUTPUT_BYTE);
                    uart.write(txBF);
                    sleep_ms(OUTPUT_INTERVAL);
                }
                count++;
                sectorNum++;
            }
            blockNum++;
        }
    }
    #endif
    return 0;
}