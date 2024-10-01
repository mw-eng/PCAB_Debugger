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