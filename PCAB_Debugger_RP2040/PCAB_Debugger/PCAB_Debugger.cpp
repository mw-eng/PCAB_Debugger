#include <stdio.h>
#include <string>
#include <iostream>
#include "pico/stdlib.h"
#include "hardware/uart.h"
#include "hardware/spi.h"
#include "hardware/flash.h"
#include "hardware/sync.h"
#include "hardware/gpio.h"
#include "config.h"
#include "subfunc.h"
#include "pico/binary_info.h"

uint8_t DAT[FLASH_PAGE_SIZE]; //MAX256BYTE
uint64_t SENS_TMP[15]; //Temp Senser IDs
uint64_t SENS_Vd[15]; //Vd Senser IDs
uint64_t SENS_Id[15]; //Id Senser IDs

void saveMEMORY()
{
    const uint32_t FLASH_TARGET_OFFSET = DAT_ADDRESS;
    uint32_t ints = save_and_disable_interrupts();
    flash_range_erase(FLASH_TARGET_OFFSET, FLASH_SECTOR_SIZE);
    flash_range_program(FLASH_TARGET_OFFSET, DAT, FLASH_PAGE_SIZE);
    restore_interrupts(ints);
}

void loadMEMORY(void)
{
    const uint32_t FLASH_TARGET_OFFSET = DAT_ADDRESS;
    const uint8_t *flash_target_contents = (const uint8_t *) (XIP_BASE + FLASH_TARGET_OFFSET);
    for(int i = 0 ; i < (int)FLASH_PAGE_SIZE ; i++){
        DAT[i] = flash_target_contents[i];
    }
}

void writePHASE(uint8_t count)
{
    uint8_t buffer[count];
    uint8_t writeDAT[count];
    int cnt = 0;
    for(int i = count; i > 0 ; i--)
    {
        //writeDAT[cnt] = uint8_bitLeftShift(DAT[i], 2);
        writeDAT[cnt] = DAT[i];
        cnt++;
    }
    gpio_put(SPI0_LE_PIN, 0);
    sleep_ms(1);
    spi_write_read_blocking(spi0, writeDAT, buffer, count);
    sleep_ms(1);
    gpio_put(SPI0_LE_PIN, 1);
}

void setup()
{
    //UART Setup
    uart_init(UART_ID, UART_BAUD_RATE);
    gpio_set_function(UART_TX_PIN, GPIO_FUNC_UART);
    gpio_set_function(UART_RX_PIN, GPIO_FUNC_UART);
    uart_set_hw_flow(UART_ID, UART_CTS, UART_RTS);
    uart_set_format(UART_ID, UART_DATA_BITS, UART_STOP_BITS, UART_PARITY);
    //SPI0 Setup
    spi_init(spi0, SPI0_CLK);
    gpio_set_function(SPI0_RX_PIN, GPIO_FUNC_SPI);
    gpio_set_function(SPI0_CLK_PIN, GPIO_FUNC_SPI);
    gpio_set_function(SPI0_TX_PIN, GPIO_FUNC_SPI);
    //gpio_set_function(SPI0_LE_PIN, GPIO_FUNC_SPI);
    gpio_init(SPI0_LE_PIN);
    gpio_set_dir(SPI0_LE_PIN , GPIO_OUT);
    gpio_put(SPI0_LE_PIN, 1);
    if(SPI0_MODE == 0 && SPI0_ORDER == 0){spi_set_format(spi0, SPI0_BITS, SPI_CPOL_0, SPI_CPHA_0, SPI_LSB_FIRST);}
    else if(SPI0_MODE == 1 && SPI0_ORDER == 0){spi_set_format(spi0, SPI0_BITS, SPI_CPOL_0, SPI_CPHA_1, SPI_LSB_FIRST);}
    else if(SPI0_MODE == 1 && SPI0_ORDER == 1){spi_set_format(spi0, SPI0_BITS, SPI_CPOL_0, SPI_CPHA_1, SPI_MSB_FIRST);}
    else if(SPI0_MODE == 2 && SPI0_ORDER == 0){spi_set_format(spi0, SPI0_BITS, SPI_CPOL_1, SPI_CPHA_0, SPI_LSB_FIRST);}
    else if(SPI0_MODE == 2 && SPI0_ORDER == 1){spi_set_format(spi0, SPI0_BITS, SPI_CPOL_1, SPI_CPHA_0, SPI_MSB_FIRST);}
    else if(SPI0_MODE == 3 && SPI0_ORDER == 0){spi_set_format(spi0, SPI0_BITS, SPI_CPOL_1, SPI_CPHA_1, SPI_LSB_FIRST);}
    else if(SPI0_MODE == 3 && SPI0_ORDER == 1){spi_set_format(spi0, SPI0_BITS, SPI_CPOL_1, SPI_CPHA_1, SPI_MSB_FIRST);}
    else{spi_set_format(spi0, SPI0_BITS, SPI_CPOL_0, SPI_CPHA_0, SPI_MSB_FIRST);}
    //GPIO Setup
    gpio_init(LPW_MOD_PIN);
    gpio_init(STB_DRA_PIN);
    gpio_init(STB_AMP_PIN);
    gpio_init(STB_LNA_PIN);
    gpio_init(SW_1_PIN);
    gpio_init(SW_2_PIN);
    gpio_init(SW_3_PIN);
    gpio_init(SW_4_PIN);
    gpio_init(SW_5_PIN);
    gpio_init(SW_6_PIN);
    gpio_set_dir(LPW_MOD_PIN , GPIO_OUT);
    gpio_set_dir(STB_DRA_PIN , GPIO_OUT);
    gpio_set_dir(STB_AMP_PIN , GPIO_OUT);
    gpio_set_dir(STB_LNA_PIN , GPIO_OUT);
    gpio_set_dir(SW_1_PIN , GPIO_IN);
    gpio_set_dir(SW_2_PIN , GPIO_IN);
    gpio_set_dir(SW_3_PIN , GPIO_IN);
    gpio_set_dir(SW_4_PIN , GPIO_IN);
    gpio_set_dir(SW_5_PIN , GPIO_IN);
    gpio_set_dir(SW_6_PIN , GPIO_IN);
    loadMEMORY();
    //Memory Config Auto Load
    if(DAT[0] == 1 && gpio_get(SW_1_PIN))
    {
        writePHASE(15);
        if(DAT[50] != 1){gpio_put(STB_AMP_PIN, 1);}
        else{gpio_put(STB_AMP_PIN, 0);}
        if(DAT[51] != 1){gpio_put(STB_DRA_PIN, 1);}
        else{gpio_put(STB_DRA_PIN, 0);}
        if(DAT[52] != 1){gpio_put(STB_LNA_PIN, 1);}
        else{gpio_put(STB_LNA_PIN, 0);}
        if(DAT[53] != 1){gpio_put(LPW_MOD_PIN, 1);}
        else{gpio_put(LPW_MOD_PIN, 0);}
    }
    uart_puts(UART_ID, "PCAB Debugger BOOT.\n>");
}

int main() {
    setup();
    while(1){
        std::string cmdline = getCMDLINE();
        if(trim(cmdline).empty()){uart_puts(UART_ID, ">");}
        else
        {
            uart_puts(UART_ID, ("CMD : " + cmdline).c_str());
            cmd cmdDAT = getCMD(cmdline);
            switch(cmdDAT.cmd_code)
            {
                case WrtPS:
                    writePHASE(15);
                    uart_puts(UART_ID,"Phase setting write.\n>");
                    break;
                case GetPS:
                    if(0 < cmdDAT.id && cmdDAT.id < 50){uart_puts(UART_ID,("PS" + std::to_string(cmdDAT.id) + ">" + std::to_string(DAT[cmdDAT.id] * 5.625) + "deg\n>").c_str());}
                    else{uart_puts(UART_ID,"ID error.\n>");}
                    break;
                case SetPS:
                    if(0 < cmdDAT.id && cmdDAT.id < 50){DAT[cmdDAT.id] = cmdDAT.arg;uart_puts(UART_ID,("Set Phase No," + std::to_string(cmdDAT.id) + " / Phase = " + std::to_string(cmdDAT.arg * 5.625) + "deg\n>").c_str());}
                    else{uart_puts(UART_ID,"ID error.\n>");}
                    break;
                case GetTMP:
                    uart_puts(UART_ID,"Command not implemented.\n>");
                    break;
                case GetId:
                    uart_puts(UART_ID,"Command not implemented.\n>");
                    break;
                case GetVd:
                    uart_puts(UART_ID,"Command not implemented.\n>");
                    break;
                case GetSTB_AMP:
                    if(DAT[50] == 0) {uart_puts(UART_ID,"AMP RUN MODE NOW.\n>");}
                    else if(DAT[50] == 1) {uart_puts(UART_ID,"AMP STANDBY MODE NOW.\n>");}
                    break;
                case SetSTB_AMP:
                    if(cmdDAT.arg == 0) {DAT[50] = 0;gpio_put(STB_AMP_PIN, 1);uart_puts(UART_ID,"AMP RUN MODE.\n>");}
                    else if(cmdDAT.arg == 1) {DAT[50] = 1;gpio_put(STB_AMP_PIN, 0);uart_puts(UART_ID,"AMP STANDBY MODE.\n>");}
                    else{uart_puts(UART_ID,"Argument error.\n>");}
                    break;
                case GetSTB_DRA:
                    if(DAT[51] == 0) {uart_puts(UART_ID,"DRA RUN MODE NOW.\n>");}
                    else if(DAT[51] == 1) {uart_puts(UART_ID,"DRA STANDBY MODE NOW.\n>");}
                    break;
                case SetSTB_DRA:
                    if(cmdDAT.arg == 0) {DAT[51] = 0;gpio_put(STB_DRA_PIN, 1);uart_puts(UART_ID,"DRA RUN MODE.\n>");}
                    else if(cmdDAT.arg == 1) {DAT[51] = 1;gpio_put(STB_DRA_PIN, 0);uart_puts(UART_ID,"DRA STANDBY MODE.\n>");}
                    else{uart_puts(UART_ID,"Argument error.\n>");}
                    break;
                case GetSTB_LNA:
                    if(DAT[52] == 0) {uart_puts(UART_ID,"LNA RUN MODE NOW.\n>");}
                    else if(DAT[52] == 1) {uart_puts(UART_ID,"LNA STANDBY MODE NOW.\n>");}
                    break;
                case SetSTB_LNA:
                    if(cmdDAT.arg == 0) {DAT[52] = 0;gpio_put(STB_LNA_PIN, 1);uart_puts(UART_ID,"LNA RUN MODE.\n>");}
                    else if(cmdDAT.arg == 1) {DAT[52] = 1;gpio_put(STB_LNA_PIN, 0);uart_puts(UART_ID,"LNA STANDBY MODE.\n>");}
                    else{uart_puts(UART_ID,"Argument error.\n>");}
                    break;
                case GetLPM:
                    if(DAT[53] == 0) {uart_puts(UART_ID,"FULL POWER MODE NOW.\n>");}
                    else if(DAT[53] == 1) {uart_puts(UART_ID,"LOW POWER MODE NOW.\n>");}
                    break;
                case SetLPM:
                    if(cmdDAT.arg == 0) {DAT[53] = 0;gpio_put(LPW_MOD_PIN, 1);uart_puts(UART_ID,"FULL POWER MODE.\n>");}
                    else if(cmdDAT.arg == 1) {DAT[53] = 1;gpio_put(LPW_MOD_PIN, 0);uart_puts(UART_ID,"LOW POWER MODE.\n>");}
                    else{uart_puts(UART_ID,"Argument error.\n>");}
                    break;
                case SetALD:
                    if(cmdDAT.arg == 0) {DAT[0] = 0;uart_puts(UART_ID,"NON-AUTO LOAD MODE.\n>");}
                    else if(cmdDAT.arg == 1) {DAT[0] = 1;uart_puts(UART_ID,"AUTO LOAD MODE.\n>");}
                    else{uart_puts(UART_ID,"Argument error.\n>");}
                    break;
                case GetALD:
                    if(DAT[0] == 1 && gpio_get(SW_1_PIN)){uart_puts(UART_ID,"AUTO LOAD MODE.\n>");}
                    else{uart_puts(UART_ID,"NON-AUTO LOAD MODE.\n>");}
                    break;
                case SaveMEM:
                    saveMEMORY();
                    uart_puts(UART_ID,"Save Memory DONE.\n>");
                    break;
                case LoadMEM:
                    loadMEMORY();
                    uart_puts(UART_ID,"ReLoad Memory DONE.\n>");
                    break;
                case RST:
                    for(int i = 1 ; i < 50 ; i++){DAT[i] = 0;}writePHASE(15);
                    uart_puts(UART_ID, "ALL Phase 0deg\n");
                    DAT[50] = 0;gpio_put(STB_AMP_PIN, 1);uart_puts(UART_ID,"AMP RUN MODE.\n");
                    DAT[51] = 0;gpio_put(STB_DRA_PIN, 1);uart_puts(UART_ID,"DRA RUN MODE.\n");
                    DAT[52] = 0;gpio_put(STB_LNA_PIN, 1);uart_puts(UART_ID,"LNA RUN MODE.\n");
                    DAT[53] = 0;gpio_put(LPW_MOD_PIN, 1);uart_puts(UART_ID,"FULL POWER MODE.\n");
                    DAT[0] = 1;uart_puts(UART_ID,"AUTO LOAD MODE.\n>");
                    uart_puts(UART_ID,"Preset DONE.\n>");
                    break;
                default:
                    uart_puts(UART_ID,"Command not found.\n>");
                    break;
            }
        }
    }
    return 0;
}

/*
uint8_t DAT[FLASH_PAGE_SIZE]; //MAX256BYTE
DAT[0]:Set auto load date
DAT[1-49]:PS1 to PS49 StateBin
DAT[50]:AMP STBY
DAT[51]:DRA STBY
DAT[52]:LNA STBY
DAT[53]:Low Power Mode
*/