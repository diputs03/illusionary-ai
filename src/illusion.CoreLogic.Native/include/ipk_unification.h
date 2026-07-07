#ifndef IPK_UNIFICATION_H
#define IPK_UNIFICATION_H

#include "ipk_ast.h"
#include "ipk_substitution.h"

#ifdef __cplusplus
extern "C" {
#endif

    IAPI_EXPORT bool IPK_IsMetaVariable(IPK_String symbol_name);

    IAPI_EXPORT IPK_RESULT IPK_Unify(IPK_AST_Handle pattern, IPK_AST_Handle term, IPK_Sub_Handle sub);

#ifdef __cplusplus
}
#endif

#endif // IPK_UNIFICATION_H