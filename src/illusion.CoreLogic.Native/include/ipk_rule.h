#ifndef IPK_RULE_H
#define IPK_RULE_H

#include "ipk_ast.h"
#include "ipk_substitution.h"

typedef struct IPK_Rule IPK_Rule;
typedef struct IPK_Rule* Rule_Handle;
struct IPK_Rule {
	String_Handle name;
	size_t premise_count;
	AST_Handle* premises;
	AST_Handle conclusion;
};

#ifdef __cplusplus
extern "C" {
#endif

	IPK_RESULT IPK_CreateRule(String_Handle name,
		size_t premise_count, AST_Handle* premises,
		AST_Handle conclusion, Rule_Handle* out_rule);
	IPK_RESULT IPK_ApplyRule(Rule_Handle rule, AST_Handle* premise_ins, AST_Handle* out_ins);
	void IPK_FreeRule(Rule_Handle rule);


#ifdef __cplusplus
}
#endif


#endif // IPK_RULE_H