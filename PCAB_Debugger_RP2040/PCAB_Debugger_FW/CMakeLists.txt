set(ProjectName "PCAB_Debugger_FW")

add_subdirectory(MWComLibCPP_library)
add_subdirectory(PCAB_Debugger_library)
add_subdirectory(MWComLibRaspico_library/flash_library)
add_subdirectory(MWComLibRaspico_library/uart_library)
add_subdirectory(MWComLibRaspico_library/spi_library)
add_subdirectory(MWComLibRaspico_library/adc_library)
add_subdirectory(MWComLibRaspico_library/ds18b20_library)
add_subdirectory(MWComLibRaspico_library/onewire_library)

add_executable(${ProjectName}
    ${ProjectName}.cpp
)

# Add pico_stdlib library which aggregates commonly used features
target_link_libraries(${ProjectName}
    pico_stdlib
    hardware_flash
    hardware_sync
    hardware_gpio
    hardware_spi
    hardware_pio
    hardware_adc
    flash_library
    spi_library
    adc_library
    MWComLibCPP_library
    PCAB_Debugger_library
    ds18b20_library
)

add_compile_definitions(PICO_FLASH_SIZE_BYTES=2*1024*1024) #16M (Raspberry Pi Pico)
#add_compile_definitions(PICO_FLASH_SIZE_BYTES=4*1024*1024) #32M
#add_compile_definitions(PICO_FLASH_SIZE_BYTES=8*1024*1024) #64M
#add_compile_definitions(PICO_FLASH_SIZE_BYTES=16*1024*1024) #128M (PCAB)
#add_compile_definitions(PICO_FLASH_SIZE_BYTES=32*1024*1024) #256M

# create map/bin/hex/uf2 file in addition to ELF.
pico_add_extra_outputs(${ProjectName})
