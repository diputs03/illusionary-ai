global using Index_t = System.Int64;
global using Size_t = nuint;

namespace illusion.IPK_Adapter
{
    internal enum IPK_RESULT
    {
        IPK_SUCCESS = 0x0000,
        IPK_ERROR = 0x0001,

        IPK_ERROR_RUNTIME_ERROR = 0x0011,
        IPK_ERROR_INVALID_ARGUMENT = 0x0012,
        IPK_ERROR_SYNTAX_ERROR = 0x0013,
        IPK_ERROR_UNSUPPORTED_OPERATION = 0x0014,
        IPK_ERROR_AXIOM_NOT_FOUND = 0x0015,
        IPK_ERROR_UNIFICATION_FAILED = 0x0016,
        IPK_ERROR_NOT_FOUND = 0x0017,

        IPK_ERROR_INTERNET_ERROR = 0x0101,

        IPK_ERROR_MEMORY_ERROR = 0x0201,
        IPK_ERROR_NULL_POINTER = 0x0202,
        IPK_ERROR_OUT_OF_MEMORY = 0x0203,
    }
}
