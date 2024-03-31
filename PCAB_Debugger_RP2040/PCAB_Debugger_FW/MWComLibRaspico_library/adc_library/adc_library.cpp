#include "adc_library.hpp"



//    // ADC
//    adc_init();
//    adc_gpio_init(SNS_ID_PIN);
//    adc_gpio_init(SNS_VD_PIN);
//                case GetId:
//                    adc_select_input(SNS_ID_SELIN);
//                    result = adc_read();
//                    if(DAT[100] != 1){uart_puts(UART_ID,(std::to_string(((result * conversion_factor) - 1.65) / 0.09) + "A\n>").c_str());}
//                    else{uart_puts(UART_ID, (std::to_string(result * conversion_factor) + "\n").c_str());}
//                    break;
//                case GetVd:
//                    adc_select_input(SNS_VD_SELIN);
//                    result = adc_read();
//                    if(DAT[100] != 1){uart_puts(UART_ID,(std::to_string(result * conversion_factor * 10.091) + "V\n>").c_str());}
//                    else{uart_puts(UART_ID, (std::to_string(result * conversion_factor) + "\n").c_str());}
//                    break;