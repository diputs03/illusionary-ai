cmake_minimum_required(VERSION 3.29)
project(illusion.CoreLogic.Native LANGUAGES C CXX)

set(CMAKE_C_STANDARD 11)
set(CMAKE_C_STANDARD_REQUIRED ON)
set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

set(CMAKE_RUNTIME_OUTPUT_DIRECTORY "${CMAKE_SOURCE_DIR}/../../artifacts/bin/${PROJECT_NAME}")
set(CMAKE_LIBRARY_OUTPUT_DIRECTORY "${CMAKE_SOURCE_DIR}/../../artifacts/lib/${PROJECT_NAME}")
set(CMAKE_ARCHIVE_OUTPUT_DIRECTORY "${CMAKE_SOURCE_DIR}/../../artifacts/lib/${PROJECT_NAME}")

foreach(config IN ITEMS Debug Release RelWithDebInfo MinSizeRel)
    string(TOUPPER ${config} config_upper)
    set(CMAKE_RUNTIME_OUTPUT_DIRECTORY_${config_upper} "${CMAKE_RUNTIME_OUTPUT_DIRECTORY}/${config}")
    set(CMAKE_LIBRARY_OUTPUT_DIRECTORY_${config_upper} "${CMAKE_LIBRARY_OUTPUT_DIRECTORY}/${config}")
    set(CMAKE_ARCHIVE_OUTPUT_DIRECTORY_${config_upper} "${CMAKE_ARCHIVE_OUTPUT_DIRECTORY}/${config}")
endforeach()

include_directories(${CMAKE_SOURCE_DIR}/include)

set(SOURCES
    src/ipk_ast.cpp
    src/ipk_common.cpp
    src/ipk_context.cpp
    src/ipk_kernel.cpp
    src/ipk_parser.cpp
    src/ipk_rule.cpp
    src/ipk_substitution.cpp
    src/ipk_unification.cpp
)

set(HEADERS
    include/ipk_ast.h
    include/ipk_common.h
    include/ipk_context.h
    include/ipk_kernel.h
    include/ipk_parser.h
    include/ipk_rule.h
    include/ipk_substitution.h
    include/ipk_unification.h
)

add_library(${PROJECT_NAME} SHARED ${SOURCES} ${HEADERS})

target_compile_definitions(${PROJECT_NAME}
    PRIVATE
        $<$<PLATFORM_ID:Windows>:_WIN32;UNICODE;_UNICODE>
)

target_include_directories(${PROJECT_NAME} PUBLIC
    ${CMAKE_CURRENT_SOURCE_DIR}/include
)

install(TARGETS ${PROJECT_NAME}
    RUNTIME DESTINATION bin
    LIBRARY DESTINATION lib
    ARCHIVE DESTINATION lib
)

install(FILES ${HEADERS} DESTINATION include)
add_subdirectory(test)