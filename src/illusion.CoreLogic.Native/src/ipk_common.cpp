#include "ipk_common.h"

API_EXPORT IPK_RESULT IPK_MakeString(CSHARP_String in, IPK_String* out) {
    if (!in || !out) {
        return IPK_ERROR_NULL_POINTER;
    }
    *out = ipk_strdup(in);
    if (!*out) {
        return IPK_ERROR_OUT_OF_MEMORY;
    }
    return IPK_SUCCESS;
}

template<typename T>
API_EXPORT IPK_RESULT IPK_MakeList(T* in, size_t length, T** out) {
    *out = NewObject<T>(length);
    if (!*out) {
        return IPK_ERROR_OUT_OF_MEMORY;
    }
    for (size_t i = 0; i < length; i++) {
        (*out)[i] = in[i];
    }
    return IPK_SUCCESS;
}

API_EXPORT void IPK_FreeString(IPK_String str) {
    free(str);
}

API_EXPORT CSHARP_String IPK_GetErrorMessage(IPK_RESULT result) {
    switch (result) {
    case IPK_SUCCESS:                    return TEXT("Success");              
    case IPK_ERROR:                      return TEXT("General error");        
    case IPK_ERROR_RUNTIME_ERROR:        return TEXT("Runtime error");        
    case IPK_ERROR_INVALID_ARGUMENT:     return TEXT("Invalid argument");
    case IPK_ERROR_SYNTAX_ERROR:         return TEXT("Syntax error");         
    case IPK_ERROR_UNSUPPORTED_OPERATION:return TEXT("Unsupported operation");
    case IPK_ERROR_AXIOM_NOT_FOUND:      return TEXT("Axiom not found");      
    case IPK_ERROR_UNIFICATION_FAILED: return TEXT("Unification failed");
    case IPK_ERROR_NOT_FOUND:          return TEXT("Not found");
    case IPK_ERROR_INTERNET_ERROR:       return TEXT("Internet error");       
    case IPK_ERROR_MEMORY_ERROR:         return TEXT("Memory error");         
    case IPK_ERROR_NULL_POINTER:         return TEXT("Null pointer error");   
    case IPK_ERROR_OUT_OF_MEMORY:       return TEXT("Out of memory");
    default:                             return TEXT("Unknown error code");   
    }
}