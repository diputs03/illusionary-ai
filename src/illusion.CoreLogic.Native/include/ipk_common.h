#ifndef IPK_COMMON_H
#define IPK_COMMON_H

#include <stdint.h>
#include <stdbool.h>
#include <stdlib.h>
#include <new>
#include <memory>
#include <cctype>
#include <cstring>

#ifdef _WIN32
#define API_IMPORT __declspec(dllimport)
#define API_EXPORT __declspec(dllexport)
#else
#define API_IMPORT
#define API_EXPORT __attribute__((visibility("default")))
#endif

/*
* ERROR CODE RANGE
* RES & 0x01 -> SUCCESS/FAILURE
* RES & 0x10 -> RUNTIME  ERROR
* RES & 0x20 -> INTERNET ERROR
* RES & 0x40 -> MEMORY   ERROR
*/
typedef enum IPK_RESULT {
    IPK_SUCCESS                     =0x0000,
    IPK_ERROR                       =0x0001,

    IPK_ERROR_RUNTIME_ERROR         =0x0011,
    IPK_ERROR_INVALID_ARGUMENT      =0x0012,
    IPK_ERROR_SYNTAX_ERROR          =0x0013,
    IPK_ERROR_UNSUPPORTED_OPERATION =0x0014,
    IPK_ERROR_AXIOM_NOT_FOUND       =0x0015,
    IPK_ERROR_UNIFICATION_FAILED    =0x0016,
    IPK_ERROR_NOT_FOUND             =0x0017,

    IPK_ERROR_INTERNET_ERROR        =0x0101,

    IPK_ERROR_MEMORY_ERROR          =0x0201,
    IPK_ERROR_NULL_POINTER          =0x0202,
    IPK_ERROR_OUT_OF_MEMORY         =0x0203,
} IPK_RESULT;

#ifdef LONG_CHAR
#define Char wchar_t
#define ipk_strdup wcsdup
#define ipk_strcmp wcscmp
#define ostringstream wostringstream
#define ipk_strlen wcslen
#define TEXT(x) L ## x
#else
#define Char char
#define ipk_strdup strdup
#define ipk_strcmp strcmp
#define TEXT(x) x
#endif

typedef Char* IPK_String;
#define CSHARP_String const Char*

#ifdef __cplusplus
extern "C" {
#endif

    API_EXPORT CSHARP_String IPK_GetErrorMessage(IPK_RESULT result);
    API_EXPORT IPK_RESULT IPK_MakeString(CSHARP_String in, IPK_String* out);
    API_EXPORT void IPK_FreeString(IPK_String str);

#ifdef __cplusplus
}
#endif

// TODO: Replace this with an arena allocator if AST allocation becomes a bottleneck.
template<typename T>
T* NewObject(size_t count = 1) noexcept {
    return new (std::nothrow) T[count]();
}

template<typename T>
void DestroyObject(T* ptr) noexcept {
    delete[] ptr;
}

#endif // IPK_COMMON_H
