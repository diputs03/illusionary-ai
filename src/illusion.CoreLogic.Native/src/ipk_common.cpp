#include "ipk_common.h"

API_EXPORT String_Handle IPK_GetErrorMessage(IPK_RESULT result) {
    switch (result) {
    case IPK_SUCCESS:                    return "Success";              
    case IPK_ERROR:                      return "General error";        
    case IPK_ERROR_RUNTIME_ERROR:        return "Runtime error";        
    case IPK_ERROR_INVALID_ARGUMENT:     return "Invalid argument";     
    case IPK_ERROR_SYNTAX_ERROR:         return "Syntax error";         
    case IPK_ERROR_UNSUPPORTED_OPERATION:return "Unsupported operation";
    case IPK_ERROR_AXIOM_NOT_FOUND:      return "Axiom not found";      
    case IPK_ERROR_INTERNET_ERROR:       return "Internet error";       
    case IPK_ERROR_MEMORY_ERROR:         return "Memory error";         
    case IPK_ERROR_NULL_POINTER:         return "Null pointer error";   
    default:                             return "Unknown error code";   
    }
}