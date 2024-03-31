set(ProjectName "PCAB_Debugger_FW")

add_subdirectory(MWComLibCPP_library)
add_subdirectory(PCAB_Debugger_library)
add_subdirectory(MWComLibRaspico_library/uart_library)
add_subdirectory(MWComLibRaspico_library/flash_library)

add_executable(${ProjectName}
    ${ProjectName}.cpp
)

# Add pico_stdlib library which aggregates commonly used features
target_link_libraries(${ProjectName}
    pico_stdlib
    hardware_flash
    hardware_sync
    hardware_gpio hardware_spi hardware_pio hardware_adc
    MWComLibCPP_library
    PCAB_Debugger_library
    flash_library
)

# create map/bin/hex/uf2 file in addition to ELF.
pico_add_extra_outputs(${ProjectName})
