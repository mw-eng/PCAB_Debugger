#include "spi_library.hpp"


//    //SPI0 Setup
//    spi_init(spi0, SPI0_CLK);
//    gpio_set_function(SPI0_RX_PIN, GPIO_FUNC_SPI);
//    gpio_set_function(SPI0_CLK_PIN, GPIO_FUNC_SPI);
//    gpio_set_function(SPI0_TX_PIN, GPIO_FUNC_SPI);
//    //gpio_set_function(SPI0_LE_PIN, GPIO_FUNC_SPI);
//    gpio_init(SPI0_LE_PIN);
//    gpio_set_dir(SPI0_LE_PIN , GPIO_OUT);
//    gpio_put(SPI0_LE_PIN, 1);
//    if(SPI0_MODE == 0 && SPI0_ORDER == 0){spi_set_format(spi0, SPI0_BITS, SPI_CPOL_0, SPI_CPHA_0, SPI_LSB_FIRST);}
//    else if(SPI0_MODE == 1 && SPI0_ORDER == 0){spi_set_format(spi0, SPI0_BITS, SPI_CPOL_0, SPI_CPHA_1, SPI_LSB_FIRST);}
//    else if(SPI0_MODE == 1 && SPI0_ORDER == 1){spi_set_format(spi0, SPI0_BITS, SPI_CPOL_0, SPI_CPHA_1, SPI_MSB_FIRST);}
//    else if(SPI0_MODE == 2 && SPI0_ORDER == 0){spi_set_format(spi0, SPI0_BITS, SPI_CPOL_1, SPI_CPHA_0, SPI_LSB_FIRST);}
//    else if(SPI0_MODE == 2 && SPI0_ORDER == 1){spi_set_format(spi0, SPI0_BITS, SPI_CPOL_1, SPI_CPHA_0, SPI_MSB_FIRST);}
//    else if(SPI0_MODE == 3 && SPI0_ORDER == 0){spi_set_format(spi0, SPI0_BITS, SPI_CPOL_1, SPI_CPHA_1, SPI_LSB_FIRST);}
//    else if(SPI0_MODE == 3 && SPI0_ORDER == 1){spi_set_format(spi0, SPI0_BITS, SPI_CPOL_1, SPI_CPHA_1, SPI_MSB_FIRST);}
//    else{spi_set_format(spi0, SPI0_BITS, SPI_CPOL_0, SPI_CPHA_0, SPI_MSB_FIRST);}
//
//    void writePHASE(uint8_t count)
//{
//    uint8_t buffer[count];
//    uint8_t writeDAT[count];
//    int cnt = 0;
//    for(int i = count; i > 0 ; i--)
//    {
//        //writeDAT[cnt] = uint8_bitLeftShift(DAT[i], 2);
//        writeDAT[cnt] = DAT[i];
//        cnt++;
//    }
//    gpio_put(SPI0_LE_PIN, 0);
//    sleep_ms(1);
//    spi_write_read_blocking(spi0, writeDAT, buffer, count);
//    sleep_ms(1);
//    gpio_put(SPI0_LE_PIN, 1);
//}