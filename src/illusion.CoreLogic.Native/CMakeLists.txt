cmake_minimum_required(VERSION 3.29)
project(illusion.CoreLogic.Native VERSION 0.1.0 LANGUAGES C CXX)

set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)
set(CMAKE_CXX_EXTENSIONS OFF)

set(CMAKE_RUNTIME_OUTPUT_DIRECTORY ${CMAKE_BINARY_DIR}/bin)
set(CMAKE_LIBRARY_OUTPUT_DIRECTORY ${CMAKE_BINARY_DIR}/lib)
set(CMAKE_ARCHIVE_OUTPUT_DIRECTORY ${CMAKE_BINARY_DIR}/lib)

if(WIN32)
    add_definitions(-D_WIN32 -DUNICODE -D_UNICODE)
    set(CMAKE_WINDOWS_EXPORT_ALL_SYMBOLS ON)
endif()

set(SOURCES
    src/ipk_common.cpp
    src/ipk_ast.cpp
    src/ipk_substitution.cpp
    src/ipk_unification.cpp
    src/ipk_rule.cpp
    src/ipk_context.cpp
    src/ipk_kernel.cpp
)

set(HEADERS
    include/ipk_common.h
    include/ipk_ast.h
    include/ipk_substitution.h
    include/ipk_unification.h
    include/ipk_rule.h
    include/ipk_context.h
    include/ipk_kernel.h
)

add_library(illusion.CoreLogic.Native SHARED ${SOURCES} ${HEADERS})

target_include_directories(illusion.CoreLogic.Native PUBLIC
    ${CMAKE_CURRENT_SOURCE_DIR}/include
)

install(TARGETS illusion.CoreLogic.Native
    RUNTIME DESTINATION bin
    LIBRARY DESTINATION lib
    ARCHIVE DESTINATION lib
)

install(FILES ${HEADERS} DESTINATION include)