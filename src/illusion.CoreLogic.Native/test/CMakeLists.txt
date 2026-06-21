cmake_minimum_required(VERSION 3.22)
project(illusion.CoreLogic.Tests.Native)

set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

add_executable(test
    parser_test.cpp
)

target_link_libraries(test
    PRIVATE
        illusion.CoreLogic.Native
)
set(CMAKE_RUNTIME_OUTPUT_DIRECTORY 
    "${CMAKE_SOURCE_DIR}/../../artifacts/bin/illusion.CoreLogic.Tests.Native/$<CONFIG>/"
)
set(CMAKE_RUNTIME_OUTPUT_DIRECTORY_DEBUG ${CMAKE_RUNTIME_OUTPUT_DIRECTORY})
set(CMAKE_RUNTIME_OUTPUT_DIRECTORY_RELEASE ${CMAKE_RUNTIME_OUTPUT_DIRECTORY})
if(WIN32)
    add_custom_command(TARGET test POST_BUILD
        COMMAND ${CMAKE_COMMAND} -E copy
            $<TARGET_FILE:illusion.CoreLogic.Native>
            $<TARGET_FILE_DIR:test>
        COMMENT "Copying native DLL to test output directory"
    )
endif()