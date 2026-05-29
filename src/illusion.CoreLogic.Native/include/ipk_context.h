#ifndef IPK_CONTEXT_H
#define IPK_CONTEXT_H

#include "ipk_ast.h"
#include "ipk_rule.h"
#include <unordered_map>

typedef struct IPK_Context IPK_Context;
typedef IPK_Context* Context_Handle;

#ifdef __cplusplus
extern "C" {
#endif

    IPK_RESULT IPK_CreateContext(Context_Handle* out_context);

    void IPK_DestroyContext(Context_Handle context);

    IPK_RESULT IPK_ContextAddAxiom(Context_Handle context,
        String_Handle name,
        AST_Handle statement,
        index_t* out_axiom_id);

    IPK_RESULT IPK_ContextGetAxiom(Context_Handle context, index_t axiom_id, AST_Handle* out_statement);

    IPK_RESULT IPK_ContextAddRule(Context_Handle context, Rule_Handle rule);

    IPK_RESULT IPK_ContextGetRule(Context_Handle context, String_Handle rule_name, Rule_Handle* out_rule);
#ifdef __cplusplus
}
#endif

#endif // IPK_CONTEXT_H