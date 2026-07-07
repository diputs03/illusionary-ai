#ifndef IPK_COMMON_H
#define IPK_COMMON_H

#include <stdint.h>
#include <stdbool.h>
#include <stdlib.h>
#include <string.h>
#include <stdio.h>
#include <assert.h>

#ifdef _WIN32
#define IAPI_IMPORT __declspec(dllimport)
#define IAPI_EXPORT __declspec(dllexport)
#else
#define IAPI_IMPORT
#define IAPI_EXPORT __attribute__((visibility("default")))
#endif

// TODO: UHT like reflection or direct adoption of UHT
#define IMETA(...)
#define IFUNCTION(...)
#define IENUM(...)
#define ICLASS(...)
#define ISTRUCT(...)

#ifdef LONG_CHAR
    #include <wchar.h>
    #define IPK_Char wchar_t
    #define istrdup wcsdup
    #define istrcmp wcscmp
    #define istrlen wcslen
    #define iprintf wprintf
    #define _ITEXT(x) L ## x
#else
    #define IPK_Char char
    #define istrdup strdup
    #define istrcmp strcmp
    #define istrlen strlen
    #define iprintf printf
    #define _ITEXT(x) x
#endif

#define ITEXT(x) _ITEXT(x)
typedef IPK_Char const* CSHARP_String;
typedef IPK_Char* IPK_String;

/*
* ERROR CODE RANGE
* RES & 0xFFFF -> SUCCESS/FAILURE
* RES & 0x0010 -> RUNTIME  ERROR
* RES & 0x0020 -> INTERNET ERROR
* RES & 0x0040 -> MEMORY   ERROR
*/
IENUM()
enum IPK_RESULT
{
    IPK_SUCCESS                     = 0x0000,
    IPK_ERROR                       = 0x0001,
                                      
    IPK_ERROR_RUNTIME_ERROR         = 0x0011,
    IPK_ERROR_INVALID_ARGUMENT      = 0x0012,
    IPK_ERROR_SYNTAX_ERROR          = 0x0013,
    IPK_ERROR_UNSUPPORTED_OPERATION = 0x0014,
    IPK_ERROR_AXIOM_NOT_FOUND       = 0x0015,
    IPK_ERROR_UNIFICATION_FAILED    = 0x0016,
    IPK_ERROR_NOT_FOUND             = 0x0017,
                                      
    IPK_ERROR_INTERNET_ERROR        = 0x0021,
                                      
    IPK_ERROR_MEMORY_ERROR          = 0x0041,
    IPK_ERROR_NULL_POINTER          = 0x0042,
    IPK_ERROR_OUT_OF_MEMORY         = 0x0043,
};
typedef IPK_RESULT IPK_RESULT;

#ifdef __cplusplus
extern "C" {
#endif

    IAPI_EXPORT CSHARP_String IPK_GetErrorMessage(IPK_RESULT result);
    IAPI_EXPORT IPK_RESULT IPK_MakeString(CSHARP_String in, IPK_String* out);
    IAPI_EXPORT IPK_RESULT IPK_GetString(IPK_String in, CSHARP_String* out);
    IAPI_EXPORT void IPK_FreeString(IPK_String str);

#ifdef __cplusplus
}
#endif

#ifdef __cplusplus
#include <new>
#include <memory>
#include <cctype>
#include <cstring>
#include <cstddef>
#include <cstdint>
#include <sstream>
#include <type_traits>
#include <vector>
#ifdef LONG_CHAR
#define istring wstring
#define iostringstream wostringstream
#else
#define istring string
#define iostringstream ostringstream
#endif

#ifndef SPEEDUP
// this macro depends on align parameter
#define get_header_size() ((sizeof(size_t) + align - 1) & ~(align - 1))
template<typename T, bool identical = true, typename... Args>
// args should the pointer of initializer packs
T* INewObject(size_t count = 1, Args&&... args) {
    static_assert(!std::is_array_v<T>, "T must not be an array type");
    static_assert(std::is_nothrow_destructible_v<T>,
        "T's destructor must be non-throwing to ensure exception safety");

    using NonConstT = std::remove_cv_t<T>;
    constexpr size_t align = alignof(NonConstT);
    constexpr size_t header_size = get_header_size();

    if (count > (SIZE_MAX - header_size) / sizeof(T)) {
        return nullptr;
    }
    const size_t total_bytes = header_size + count * sizeof(T);

    void* raw;
    if constexpr (align > alignof(std::max_align_t)) {
        raw = ::operator new(total_bytes, std::align_val_t{ align }, std::nothrow);
    }
    else {
        raw = ::operator new(total_bytes, std::nothrow);
    }
    if (!raw) {
        return nullptr;
    }

    NonConstT* ptr = reinterpret_cast<NonConstT*>(
        reinterpret_cast<char*>(raw) + header_size
        );

    size_t i = 0;
    
    try {
        if constexpr (identical) {
            static_assert(std::is_constructible_v<NonConstT, std::remove_reference_t<Args>&...>,
                "T cannot be constructed with the provided argument types");

            std::tuple<std::decay_t<Args>...> stored_args(std::forward<Args>(args)...);
            for (; i < count; i++) {
                std::apply([&](auto&... arg) {
                    ::new (ptr + i) NonConstT(arg...);
                    }, stored_args);
            }
        }
        else {
            static_assert(sizeof...(args) == 1,
                "identical=false mode requires exactly one argument: a pointer to the initializer array");

            auto&& src_array = std::get<0>(std::forward_as_tuple(std::forward<Args>(args)...));

            static_assert(std::is_constructible_v<NonConstT,
                std::remove_reference_t<decltype(src_array[0])>&&>,
                "T cannot be constructed from the array element type");

            for (; i < count; i++) {
                ::new (ptr + i) NonConstT(std::move(src_array[i]));
            }
        }

        *static_cast<size_t*>(raw) = count;
        return static_cast<T*>(ptr);
    }
    catch (...) {
        for (size_t j = i; j > 0; j--) {
            ptr[j - 1].~NonConstT();
        }
        if constexpr (align > alignof(std::max_align_t)) {
            ::operator delete(raw, std::align_val_t{ align });
        }
        else {
            ::operator delete(raw);
        }
        return nullptr;
    }
}
template<typename T>
size_t IObjectSize(T* ptr) {
    if (!ptr) return 0;

    using NonConstT = std::remove_cv_t<T>;
    constexpr size_t align = alignof(NonConstT);
    constexpr size_t header_size = get_header_size();
    void* raw = reinterpret_cast<char*>(const_cast<NonConstT*>(ptr)) - header_size;

    size_t count = *static_cast<size_t*>(raw);
    return count;
}
template<typename T>
void IDestroyObject(T* ptr) {
    if (!ptr) return;

    using NonConstT = std::remove_cv_t<T>;
    constexpr size_t align = alignof(NonConstT);
    constexpr size_t header_size = get_header_size();
    void* raw = reinterpret_cast<char*>(const_cast<NonConstT*>(ptr)) - header_size;

    size_t count = *static_cast<size_t*>(raw);
    for (size_t i = count; i > 0; i--) {
        const_cast<NonConstT*>(ptr)[i - 1].~NonConstT();
    }
    if constexpr (align > alignof(std::max_align_t)) {
        ::operator delete(raw, std::align_val_t{ align });
    }
    else {
        ::operator delete(raw);
    }
}
#else
#include <bitset>
// TODO: Replace this with an link-list allocator if allocation speed becomes a bottleneck.
template<size_t size>
class MemoryPool {
    std::bitset<size> pool;
    size_t next[size];
    size_t cur;
public:
    template<typename T, typename... Args>
    static T* INewObject(size_t count = 1) {
        int type_size = sizeof(T);
        return dynamic_cast<T*>(cur);
    }

    template<typename T>
    static T GetObject(T* ptr) {
        realptr = dynamic_cast<size_t>(ptr);
    }

    template<typename T>
    static void IDestroyObject(T* ptr) {

    }
};
MemoryPool<1000> _m;
#define INewObject _m.INewObject
#define IDestroyObject _m.IDestroyObject
#endif
#endif

#endif // IPK_COMMON_H
