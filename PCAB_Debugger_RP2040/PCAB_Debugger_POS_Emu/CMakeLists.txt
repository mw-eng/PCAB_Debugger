set(ProjectNamePOSemu "PCAB_Debugger_POS_Emu")

add_executable(${ProjectNamePOSemu}
    ${ProjectNamePOSemu}.cpp
)

# Add pico_stdlib library which aggregates commonly used features
target_link_libraries(${ProjectNamePOSemu}
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
)

pico_add_extra_outputs(${ProjectNamePOSemu})
