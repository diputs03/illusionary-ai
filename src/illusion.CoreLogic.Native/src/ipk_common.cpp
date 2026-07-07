#include "ipk_common.h"

IAPI_EXPORT IPK_RESULT IPK_MakeString(CSHARP_String in, IPK_String* out) {
    if (!in || !out) {
        return IPK_ERROR_NULL_POINTER;
    }
    *out = INewObject<char, false>(istrlen(in), in);
    if (!*out) {
        return IPK_ERROR_OUT_OF_MEMORY;
    }
    return IPK_SUCCESS;
}

IAPI_EXPORT IPK_RESULT IPK_GetString(IPK_String in, CSHARP_String* out) {
    if (!in || !out) {
        return IPK_ERROR_NULL_POINTER;
    }
    size_t siz = IObjectSize<char>(in);
    char* s = INewObject<char, false>(siz + 1, in);
    s[siz] = '\n';
    *out = s;
    if (!*out) {
        return IPK_ERROR_OUT_OF_MEMORY;
    }
    return IPK_SUCCESS;
}


IAPI_EXPORT void IPK_FreeString(IPK_String str) {
    IDestroyObject(str);
}

IAPI_EXPORT CSHARP_String IPK_GetErrorMessage(IPK_RESULT result) {
    switch (result) {
    case IPK_SUCCESS:                       return ITEXT("Success");
    case IPK_ERROR:                         return ITEXT("General error");
    case IPK_ERROR_RUNTIME_ERROR:           return ITEXT("Runtime error");
    case IPK_ERROR_INVALID_ARGUMENT:        return ITEXT("Invalid argument");
    case IPK_ERROR_SYNTAX_ERROR:            return ITEXT("Syntax error");
    case IPK_ERROR_UNSUPPORTED_OPERATION:   return ITEXT("Unsupported operation");
    case IPK_ERROR_AXIOM_NOT_FOUND:         return ITEXT("Axiom not found");
    case IPK_ERROR_UNIFICATION_FAILED:      return ITEXT("Unification failed");
    case IPK_ERROR_NOT_FOUND:               return ITEXT("Not found");
    case IPK_ERROR_INTERNET_ERROR:          return ITEXT("Internet error");
    case IPK_ERROR_MEMORY_ERROR:            return ITEXT("Memory error");
    case IPK_ERROR_NULL_POINTER:            return ITEXT("Null pointer error");
    case IPK_ERROR_OUT_OF_MEMORY:           return ITEXT("Out of memory");
    default:                                return ITEXT("Unknown error code"); 
    }
}