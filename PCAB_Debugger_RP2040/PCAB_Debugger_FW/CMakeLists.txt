set(ProjectName "PCAB_Debugger_FW")

add_subdirectory(MWComLibRaspico_library)
add_subdirectory(uart_library)

add_executable(${ProjectName}
    ${ProjectName}.cpp
)

# Add pico_stdlib library which aggregates commonly used features
target_link_libraries(${ProjectName}
    pico_stdlib
    MWComLibRaspico_library
    uart_library
)

# create map/bin/hex/uf2 file in addition to ELF.
pico_add_extra_outputs(${ProjectName})
