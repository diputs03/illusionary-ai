cmake_minimum_required(VERSION 3.29)
project(illusion.CoreLogic.Native LANGUAGES C CXX)

set(CMAKE_C_STANDARD 11)
set(CMAKE_C_STANDARD_REQUIRED ON)
set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

set(CSHARP_PROJECTS
    "illusion.CoreLogic"
    "illusion.CoreLogic.Tests"
)

set(CMAKE_RUNTIME_OUTPUT_DIRECTORY 
    "${CMAKE_SOURCE_DIR}/../../artifacts/bin/illusion.CoreLogic.Native/$<CONFIG>/"
)
set(CMAKE_RUNTIME_OUTPUT_DIRECTORY_DEBUG ${CMAKE_RUNTIME_OUTPUT_DIRECTORY})
set(CMAKE_RUNTIME_OUTPUT_DIRECTORY_RELEASE ${CMAKE_RUNTIME_OUTPUT_DIRECTORY})


include_directories(${CMAKE_SOURCE_DIR}/include)

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

add_library(${PROJECT_NAME} SHARED ${SOURCES} ${HEADERS})

target_compile_definitions(${PROJECT_NAME}
    PRIVATE
        $<$<PLATFORM_ID:Windows>:_WIN32;UNICODE;_UNICODE>
)

target_include_directories(${PROJECT_NAME} PUBLIC
    ${CMAKE_CURRENT_SOURCE_DIR}/include
)

foreach(PROJECT_NAME ${CSHARP_PROJECTS})
    set(CSHARP_OUTPUT_DIR
        "${CMAKE_SOURCE_DIR}/../../artifacts/bin/${PROJECT_NAME}/$<CONFIG>/net8.0/"
    )
    add_custom_command(TARGET illusion.CoreLogic.Native POST_BUILD
        COMMAND ${CMAKE_COMMAND} -E make_directory ${CSHARP_OUTPUT_DIR}
        COMMAND ${CMAKE_COMMAND} -E copy
            $<TARGET_FILE:illusion.CoreLogic.Native>
            ${CSHARP_OUTPUT_DIR}
    )
endforeach()

install(TARGETS ${PROJECT_NAME}
    RUNTIME DESTINATION bin
    LIBRARY DESTINATION lib
    ARCHIVE DESTINATION lib
)

install(FILES ${HEADERS} DESTINATION include)