#ifndef IPK_CONTEXT_H
#define IPK_CONTEXT_H

#include "ipk_ast.h"
#include "ipk_rule.h"

typedef int32_t index_t;

typedef struct IPK_Context IPK_Context;
typedef IPK_Context* IPK_Context_Handle;

#ifdef __cplusplus
extern "C" {
#endif

    IAPI_EXPORT IPK_RESULT IPK_Context_Create(IPK_Context_Handle* out_context);
    IAPI_EXPORT IPK_RESULT IPK_Context_AddAxiom(IPK_Context_Handle context, index_t axiom_id, IPK_AST_Handle statement);
    IAPI_EXPORT IPK_RESULT IPK_Context_AddRule(IPK_Context_Handle context, index_t rule_id, IPK_Rule_Handle rule);
    IAPI_EXPORT void IPK_Context_Destroy(IPK_Context_Handle context);

#ifdef __cplusplus
}
#endif

#endif // IPK_CONTEXT_H