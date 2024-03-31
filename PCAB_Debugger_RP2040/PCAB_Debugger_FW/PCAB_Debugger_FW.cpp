#include "PCAB_Debugger_FW.hpp"

void setup()
{
    stdio_init_all();
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
}

int main()
{
    setup();
    uint8_t dat[FLASH_PAGE_SIZE];
    readMEMORYblock(31 * (UINT16_MAX + 1), dat);
    eraseMEMORYblock(31 * (UINT16_MAX + 1));
    saveMEMORYblock(31 * (UINT16_MAX + 1), dat);

    ds18b20 *sens = new ds18b20(pio0, SNS_TEMP_PIN);
    std::vector<uint64_t> romCODE = sens->getSENS_ROMCODE();
    std::vector<std::string> temp = sens->readTEMP();

    pcabCMD *uart = new pcabCMD(UART_TX_PIN, UART_RX_PIN, UART_BAUD_RATE);

    for (uint64_t &x:romCODE)
    {
        uart->uart.writeLine(std::to_string(x));
    }
    for (std::string &x:temp)
    {
        uart->uart.writeLine(x);
    }


    uart->uart.writeLine("test");
    while(1){
        pcabCMD::CommandLine cmd = uart->readCMD(true);
        uart->uart.write("\r\n");
        uart->uart.writeLine(std::to_string(cmd.command));
        for(auto &num : cmd.argments){
            uart->uart.writeLine(num);
        }
    };
    delete uart;
    return 0;
}
