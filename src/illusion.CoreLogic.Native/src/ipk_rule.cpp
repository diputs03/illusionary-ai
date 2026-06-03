#include "ipk_rule.h"
#include "ipk_unification.h"

API_EXPORT IPK_RESULT IPK_CreateRule(String_Handle name, size_t premise_count,
	AST_Handle* premises, AST_Handle conclusion, Rule_Handle* out_rule) {
	if (!name || !out_rule || !conclusion || (!premises && premise_count > 0)) {
		return IPK_ERROR_INVALID_ARGUMENT;
	}

	Rule_Handle rule = Entity<IPK_Rule>();
	if (!rule) {
		return IPK_ERROR_OUT_OF_MEMORY;
	}

	rule->name = strdup(name);
	if (!rule->name) {
		FreeEntity(rule);
		return IPK_ERROR_OUT_OF_MEMORY;
	}
	
	rule->premise_count = premise_count;
	if (premise_count > 0) {
		rule->premises = Entity<AST_Handle>(premise_count);
		if (!rule->premises) {
			//free(rule->name);
			FreeEntity(rule);
			return IPK_ERROR_OUT_OF_MEMORY;
		}
		for (size_t i = 0; i < premise_count; i++) {
			IPK_CloneAST(premises[i], &rule->premises[i]);
			if (!rule->premises[i]) {
				for (size_t j = 0; j < i; j++) {
					IPK_FreeAST(rule->premises[j]);
				}
				FreeEntity(rule->premises);
				//free(rule->name);
				FreeEntity(rule);
				return IPK_ERROR_OUT_OF_MEMORY;
			}
		}
	} else {
		rule->premises = nullptr;
	}
	rule->conclusion = conclusion;
	return IPK_SUCCESS;
}

API_EXPORT IPK_RESULT IPK_ApplyRule(Rule_Handle rule, AST_Handle* premise_ins, AST_Handle* out_ins) {
	if (!rule || !premise_ins || !out_ins) {
		return IPK_ERROR_NULL_POINTER;
	}
	Substitution_Handle sub;
	res = IPK_CreateSubstitution(&sub);
	if (res != IPK_SUCCESS) {
		return res;
	}
	for (size_t i = 0; i < rule->premise_count; i++) {
		res = IPK_Unify(rule->premises[i], premise_ins[i], sub);
		if (res != IPK_SUCCESS) {
			IPK_DestroySubstitution(sub);
			return res;
		}
	}
	res = IPK_ApplySubstitution(sub, rule->conclusion, out_ins);
	IPK_DestroySubstitution(sub);
	return res;
}

API_EXPORT void IPK_FreeRule(Rule_Handle rule) {
	if (!rule) {
		return;
	}
	//free(rule->name);
	for (size_t i = 0; i < rule->premise_count; i++) {
		IPK_FreeAST(rule->premises[i]);
	}
	FreeEntity(rule);
}