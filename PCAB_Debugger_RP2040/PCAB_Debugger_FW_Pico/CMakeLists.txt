set(ProjectNamePico "PCAB_Debugger_FW_Pico")

add_executable(${ProjectNamePico}
    ${ProjectNamePico}.cpp
)

# Add pico_stdlib library which aggregates commonly used features
target_link_libraries(${ProjectNamePico}
    pico_stdlib
    hardware_flash
    hardware_sync
    hardware_gpio
    hardware_spi
    hardware_pio
    hardware_adc
    MWComLibCPP_library
    flash_library
    uart_library
    spi_library
    adc_library
    ds18b20_library
    onewire_library
    PCAB_Debugger_library
)

pico_add_extra_outputs(${ProjectNamePico})
