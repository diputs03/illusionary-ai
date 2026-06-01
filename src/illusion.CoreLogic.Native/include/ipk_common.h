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
#define API_EXPORT
#endif

/*
* ERROR CODE RANGE
* RES & -1 -> SUCCESS/FAILURE
* RES & 0x01 -> RUNTIME  ERROR
* RES & 0x02 -> INTERNET ERROR
* RES & 0x03 -> MEMORY   ERROR
*/

typedef enum IPK_RESULT IPK_RESULT;
enum IPK_RESULT {
    IPK_SUCCESS						 =0x000,
    IPK_ERROR						 =0x001,
    IPK_ERROR_RUNTIME_ERROR			 =0x011,
    IPK_ERROR_INVALID_ARGUMENT		 =0x111,
    IPK_ERROR_SYNTAX_ERROR			 =0x211,
    IPK_ERROR_UNSUPPORTED_OPERATION	 =0x311,
    IPK_ERROR_AXIOM_NOT_FOUND		 =0x411,
    IPK_ERROR_UNIFICATION_FAILED     =0x511,
    IPK_ERROR_NOT_FOUND              =0x611,
    IPK_ERROR_INTERNET_ERROR		 =0x021,
    IPK_ERROR_MEMORY_ERROR			 =0x031,
    IPK_ERROR_NULL_POINTER			 =0x131,
    IPK_ERROR_OUT_OF_MEMORY          =0x231,
};

typedef const char* String_Handle;
typedef uint64_t index_t;

#ifdef __cplusplus
extern "C" {
#endif

    API_EXPORT String_Handle IPK_GetErrorMessage(IPK_RESULT result);

#ifdef __cplusplus
}
#endif

// there should be a memory pool to efficiently create entities.
template<typename T>
T* Entity(size_t count = 1) {
    return new (std::nothrow) T[count];
}
template<typename T>
void FreeEntity(T* ptr) {
    delete[] ptr;
}
#endif // IPK_COMMON_H