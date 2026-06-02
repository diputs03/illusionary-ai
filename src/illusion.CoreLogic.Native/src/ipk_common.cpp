#include "ipk_common.h"

API_EXPORT String_Handle IPK_GetErrorMessage(IPK_RESULT result) {
    switch (result) {
    case IPK_SUCCESS:                    return (String_Handle)"Success";              
    case IPK_ERROR:                      return (String_Handle)"General error";        
    case IPK_ERROR_RUNTIME_ERROR:        return (String_Handle)"Runtime error";        
    case IPK_ERROR_INVALID_ARGUMENT:     return (String_Handle)"Invalid argument";     
    case IPK_ERROR_SYNTAX_ERROR:         return (String_Handle)"Syntax error";         
    case IPK_ERROR_UNSUPPORTED_OPERATION:return (String_Handle)"Unsupported operation";
    case IPK_ERROR_AXIOM_NOT_FOUND:      return (String_Handle)"Axiom not found";      
    case IPK_ERROR_INTERNET_ERROR:       return (String_Handle)"Internet error";       
    case IPK_ERROR_MEMORY_ERROR:         return (String_Handle)"Memory error";         
    case IPK_ERROR_NULL_POINTER:         return (String_Handle)"Null pointer error";   
    default:                             return (String_Handle)"Unknown error code";   
    }
}