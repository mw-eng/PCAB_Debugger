#include "flash_library.hpp"
#include "uart_library.hpp"
#include "MWComLibCPP_library.hpp"

// UART PIN Configure
#define UART_TX_PIN 0
#define UART_RX_PIN 1

// UART Configure
#define UART_BAUD_RATE 9600
//#define UART_BAUD_RATE 115207
//#define UART_BAUD_RATE 1000000
#define UART_STOP_BIT 1
#define UART_DATA_BITS 8

//#define WRITE_BLOCK 1  //  64KB
//#define WRITE_BLOCK 2  // 128KB
//#define WRITE_BLOCK 3  // 192KB
//#define WRITE_BLOCK 4  // 256KB
//#define WRITE_BLOCK 5  // 320KB
//#define WRITE_BLOCK 6  // 384KB
//#define WRITE_BLOCK 7  // 448KB
#define WRITE_BLOCK 8  // 512KB
#define OUTPUT_BYTE 40
#define OUTPUT_INTERVAL 20