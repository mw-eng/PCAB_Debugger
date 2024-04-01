// UART Configure
#define UART_ID uart0
//#define UART_BAUD_RATE 9600
 #define UART_BAUD_RATE 115200
#define UART_DATA_BITS 8
#define UART_STOP_BITS 1
#define UART_PARITY UART_PARITY_NONE
// #define UART_PARITY UART_PARITY_EVEN
// #define UART_PARITY UART_PARITY_ODD
#define UART_CTS false
#define UART_RTS false
#define UART_TX_PIN 0
#define UART_RX_PIN 1

// SPI Configure
#define SPI0_MODE 3
#define SPI0_CLK 1000000
#define SPI0_BITS 6
#define SPI0_ORDER 0 // 0:LSB / 1:MSB
#define SPI0_TX_PIN 7
#define SPI0_RX_PIN 4
#define SPI0_LE_PIN 5
#define SPI0_CLK_PIN 6

// IO Configure
#define LPW_MOD_PIN 18
#define STB_DRA_PIN 19
#define STB_AMP_PIN 20
#define STB_LNA_PIN 21

#define SNS_TEMP_PIN 24
#define SNS_VD_PIN 26
#define SNS_VD_SELIN 0
#define SNS_ID_PIN 27
#define SNS_ID_SELIN 1

#define SW_1_PIN 8
#define SW_2_PIN 9
#define SW_3_PIN 10
#define SW_4_PIN 11
#define SW_5_PIN 12
#define SW_6_PIN 13

#define DAT_ADDRESS 0x1F0000 // Block31(W25Q16)
//#define DAT_ADDRESS 0xFF0000    //Block255(W25Q128)
#define CMD_MAX 256
#define OW_MAX 15