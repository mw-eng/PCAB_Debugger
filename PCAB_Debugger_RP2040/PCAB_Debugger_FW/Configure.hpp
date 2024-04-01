// IO Configure
// UART PIN Configure
#define UART_TX_PIN 0
#define UART_RX_PIN 1
// SPI PIN Configure
#define SPI0_TX_PIN 7
#define SPI0_RX_PIN 4
#define SPI0_LE_PIN 5
#define SPI0_CLK_PIN 6
#define SPI1_TX_PIN 7
#define SPI1_RX_PIN 4
#define SPI1_LE_PIN 5
#define SPI1_CLK_PIN 6
// Onewire PIN Configure
#define SNS_TEMP_PIN 24
// MODE PIN Configure
#define LPW_MOD_PIN 18
#define STB_DRA_PIN 19
#define STB_AMP_PIN 20
#define STB_LNA_PIN 21
// Switch PIN Configure
#define SW_1_PIN 8
#define SW_2_PIN 9
#define SW_3_PIN 10
#define SW_4_PIN 11
#define SW_5_PIN 12
#define SW_6_PIN 13

// UART Configure
#define UART_BAUD_RATE 9600
//#define UART_BAUD_RATE 115200

// SPI Configure
#define SPI_MODE 3
#define SPI_CLK 1000000
#define SPI_BITS 6
#define SPI_ORDER false // 0:LSB / 1:MSB

// ROM MAX Block Number
#define ROM_BLOCK_MAX 31    //16M (Raspberry Pi Pico)
//#define ROM_BLOCK_MAX 63    //32M
//#define ROM_BLOCK_MAX 127   //64M
//#define ROM_BLOCK_MAX 255   //128M (PCAB)
//#define ROM_BLOCK_MAX 511   //256M