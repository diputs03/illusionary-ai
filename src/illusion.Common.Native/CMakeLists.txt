cmake_minimum_required(VERSION 3.25 FATAL_ERROR)

project(illusion.Common.Native
    VERSION 2.1.0
    LANGUAGES C CXX
)

set(CMAKE_C_STANDARD 11)
set(CMAKE_C_STANDARD_REQUIRED ON)
set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

set(CMAKE_RUNTIME_OUTPUT_DIRECTORY ${CMAKE_SOURCE_DIR}/../../artifacts/bin/native/${CMAKE_BUILD_TYPE})
set(CMAKE_LIBRARY_OUTPUT_DIRECTORY ${CMAKE_SOURCE_DIR}/../../artifacts/bin/native/${CMAKE_BUILD_TYPE})
set(CMAKE_ARCHIVE_OUTPUT_DIRECTORY ${CMAKE_SOURCE_DIR}/../../artifacts/bin/native/${CMAKE_BUILD_TYPE})

include_directories(${CMAKE_SOURCE_DIR}/include)

set(SOURCES
    src/illusion_crypto.c
    src/illusion_serialization.c
)

add_library(${PROJECT_NAME} SHARED ${SOURCES})

if(WIN32)
    target_compile_definitions(${PROJECT_NAME} PRIVATE ILLUSION_NATIVE_EXPORTS)
    if(NOT CMAKE_SIZEOF_VOID_P EQUAL 8)
        message(FATAL_ERROR "only supports x64")
    endif()
endif()

install(TARGETS ${PROJECT_NAME}
    RUNTIME DESTINATION bin
    LIBRARY DESTINATION lib
    ARCHIVE DESTINATION lib
)
install(FILES
    include/illusion_types.h
    include/illusion_crypto.h
    include/illusion_serialization.h
    DESTINATION include
)