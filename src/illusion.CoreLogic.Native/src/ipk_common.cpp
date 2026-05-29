#include "ipk_common.h"

void IPK_GetErrorMessage(IPK_RESULT result, String_Handle* out_res) {
    switch (result) {
    case IPK_SUCCESS:                    *out_res = "Success";                  return;
    case IPK_ERROR:                      *out_res = "General error";            return;
    case IPK_ERROR_RUNTIME_ERROR:        *out_res = "Runtime error";            return;
    case IPK_ERROR_INVALID_ARGUMENT:     *out_res = "Invalid argument";         return;
    case IPK_ERROR_SYNTAX_ERROR:         *out_res = "Syntax error";             return;
    case IPK_ERROR_UNSUPPORTED_OPERATION:*out_res = "Unsupported operation";    return;
    case IPK_ERROR_AXIOM_NOT_FOUND:      *out_res = "Axiom not found";          return;
    case IPK_ERROR_INTERNET_ERROR:       *out_res = "Internet error";           return;
    case IPK_ERROR_MEMORY_ERROR:         *out_res = "Memory error";             return;
    case IPK_ERROR_NULL_POINTER:         *out_res = "Null pointer error";       return;
    default:                             *out_res = "Unknown error code";       return;
    }
}