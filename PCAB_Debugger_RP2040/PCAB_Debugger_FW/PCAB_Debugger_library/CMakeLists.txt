set(ProjectName "PCAB_Debugger_library")
add_library(${ProjectName} INTERFACE)
target_sources(${ProjectName} INTERFACE ${CMAKE_CURRENT_SOURCE_DIR}/${ProjectName}.cpp)

target_link_libraries(${ProjectName} INTERFACE
	MWComLibCPP_library
    uart_library
)

# add the `binary` directory so that the generated headers are included in the project
#
target_include_directories(${ProjectName} INTERFACE
	${CMAKE_CURRENT_SOURCE_DIR}
	${CMAKE_CURRENT_BINARY_DIR}
	)
