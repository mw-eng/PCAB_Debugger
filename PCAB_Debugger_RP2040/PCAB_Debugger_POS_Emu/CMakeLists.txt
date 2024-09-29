set(ProjectNamePOSemu "PCAB_Debugger_POS_Emu")

add_executable(${ProjectNamePOSemu}
    ${ProjectNamePOSemu}.cpp
)

# Add pico_stdlib library which aggregates commonly used features
target_link_libraries(${ProjectNamePOSemu}
    pico_stdlib
)

pico_add_extra_outputs(${ProjectNamePOSemu})
