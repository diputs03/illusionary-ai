#ifndef IPK_RULE_H
#define IPK_RULE_H

#include "ipk_ast.h"
#include "ipk_substitution.h"

typedef struct IPK_Rule IPK_Rule;
typedef struct IPK_Rule* IPK_Rule_Handle;
struct IPK_Rule {
	IPK_String name;
	size_t premise_count;
	IPK_AST_Handle* premises;
	IPK_AST_Handle conclusion;
};

#ifdef __cplusplus
extern "C" {
#endif

	API_EXPORT IPK_RESULT IPK_Rule_Create(IPK_String name,
		size_t premise_count, IPK_AST_Handle* premises,
		IPK_AST_Handle conclusion, IPK_Rule_Handle* out_rule);
	API_EXPORT IPK_RESULT IPK_Rule_Apply(IPK_Rule_Handle rule, IPK_AST_Handle* premise_ins, IPK_AST_Handle* out_ins);
	API_EXPORT void IPK_Rule_Destroy(IPK_Rule_Handle rule);


#ifdef __cplusplus
}
#endif


#endif // IPK_RULE_H