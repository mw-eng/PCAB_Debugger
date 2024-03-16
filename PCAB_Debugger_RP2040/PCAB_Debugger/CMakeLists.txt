set(ProjectName "PCAB_Debugger")
project(${ProjectName} C CXX ASM)

add_executable(${ProjectName}
    PCAB_Debugger.cpp
    subfunc.cpp
)

# Add pico_stdlib library which aggregates commonly used features
target_link_libraries(${ProjectName} pico_stdlib hardware_gpio hardware_spi hardware_flash hardware_sync hardware_pio onewire_library)

# create map/bin/hex/uf2 file in addition to ELF.
pico_add_extra_outputs(${ProjectName})
add_compile_options(-Wall -g3 -O0)
