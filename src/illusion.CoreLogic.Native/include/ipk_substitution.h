#ifndef IPK_SUBSTITUTION_H
#define IPK_SUBSTITUTION_H

#include "ipk_ast.h"

typedef struct IPK_SubPair IPK_SubPair;
typedef IPK_SubPair* IPK_SubPair_Handle;

struct IPK_SubPair {
    Char* var_name;
    IPK_AST_Handle replacement;
};

typedef struct IPK_Sub IPK_Sub;
typedef IPK_Sub* IPK_Sub_Handle;

#ifdef __cplusplus
extern "C" {
#endif

    API_EXPORT IPK_RESULT IPK_Sub_Create(IPK_Sub_Handle* out_sub);
    API_EXPORT IPK_RESULT IPK_Sub_Add(IPK_Sub_Handle sub, IPK_String var_name, IPK_AST_Handle replacement);
    API_EXPORT IPK_RESULT IPK_Sub_Lookup(IPK_Sub_Handle sub, IPK_String var_name, IPK_AST_Handle* out_replacement);
    API_EXPORT IPK_RESULT IPK_Sub_Apply(IPK_Sub_Handle sub, IPK_AST_Handle node, IPK_AST_Handle* out_result);
    API_EXPORT void IPK_Sub_Destroy(IPK_Sub_Handle sub);
#ifdef __cplusplus
}
#endif

#endif // IPK_SUBSTITUTION_H