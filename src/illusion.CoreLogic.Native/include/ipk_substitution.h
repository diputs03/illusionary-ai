#ifndef IPK_SUBSTITUTION_H
#define IPK_SUBSTITUTION_H

#include "ipk_ast.h"

typedef struct IPK_SubstitutionPair IPK_SubstitutionPair;
typedef IPK_SubstitutionPair* SubstitutionPair_Handle;

struct IPK_SubstitutionPair {
    char* var_name;
    AST_Handle replacement;
};

typedef struct IPK_Substitution IPK_Substitution;
typedef IPK_Substitution* Substitution_Handle;
struct IPK_Substitution {
    size_t count;
    IPK_SubstitutionPair* pairs;
};

#ifdef __cplusplus
extern "C" {
#endif

    API_EXPORT IPK_RESULT IPK_CreateSubstitution(Substitution_Handle* out_sub);
    API_EXPORT IPK_RESULT IPK_AddSubstitution(Substitution_Handle sub, String_Handle var_name, AST_Handle replacement);
    API_EXPORT IPK_RESULT IPK_LookupSubstitution(Substitution_Handle sub, String_Handle var_name, AST_Handle* out_replacement);
    API_EXPORT IPK_RESULT IPK_ApplySubstitution(Substitution_Handle sub, AST_Handle node, AST_Handle* out_result);
    API_EXPORT void IPK_FreeSubstitution(Substitution_Handle sub);
#ifdef __cplusplus
}
#endif

#endif // IPK_SUBSTITUTION_H