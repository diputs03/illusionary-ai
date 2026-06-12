#include "ipk_rule.h"
#include "ipk_unification.h"

API_EXPORT IPK_RESULT IPK_Rule_Create(IPK_String name, size_t premise_count,
	IPK_AST_Handle* premises, IPK_AST_Handle conclusion, IPK_Rule_Handle* out_rule) {
	if (!name || !out_rule || !conclusion || (!premises && premise_count > 0)) {
		return IPK_ERROR_INVALID_ARGUMENT;
	}

	IPK_Rule_Handle rule = NewObject<IPK_Rule>();
	if (!rule) {
		return IPK_ERROR_OUT_OF_MEMORY;
	}

	rule->name = ipk_strdup(name);
	if (!rule->name) {
		DestroyObject(rule);
		return IPK_ERROR_OUT_OF_MEMORY;
	}
	
	rule->premise_count = premise_count;
	if (premise_count > 0) {
		rule->premises = NewObject<IPK_AST_Handle>(premise_count);
		if (!rule->premises) {
			free(rule->name);
			DestroyObject(rule);
			return IPK_ERROR_OUT_OF_MEMORY;
		}
		for (size_t i = 0; i < premise_count; i++) {
			IPK_AST_Clone(premises[i], &rule->premises[i]);
			if (!rule->premises[i]) {
				for (size_t j = 0; j < i; j++) {
					IPK_AST_Destroy(rule->premises[j]);
				}
				DestroyObject(rule->premises);
				free(rule->name);
				DestroyObject(rule);
				return IPK_ERROR_OUT_OF_MEMORY;
			}
		}
	} else {
		rule->premises = nullptr;
	}
	IPK_RESULT res = IPK_AST_Clone(conclusion, &rule->conclusion);
	if (res != IPK_SUCCESS) {
		for (size_t j = 0; j < premise_count; j++) {
			IPK_AST_Destroy(rule->premises[j]);
		}
		DestroyObject(rule->premises);
		free(rule->name);
		DestroyObject(rule);
		return res;
	}
	return IPK_SUCCESS;
}

API_EXPORT IPK_RESULT IPK_Rule_Apply(IPK_Rule_Handle rule, IPK_AST_Handle* premise_ins, IPK_AST_Handle* out_ins) {
	if (!rule || !premise_ins || !out_ins) {
		return IPK_ERROR_NULL_POINTER;
	}
	IPK_Sub_Handle sub;
	IPK_RESULT res = IPK_Sub_Create(&sub);
	if (res != IPK_SUCCESS) {
		return res;
	}
	for (size_t i = 0; i < rule->premise_count; i++) {
		res = IPK_Unify(rule->premises[i], premise_ins[i], sub);
		if (res != IPK_SUCCESS) {
			IPK_Sub_Destroy(sub);
			return res;
		}
	}
	res = IPK_Sub_Apply(sub, rule->conclusion, out_ins);
	IPK_Sub_Destroy(sub);
	return res;
}

API_EXPORT void IPK_Rule_Destroy(IPK_Rule_Handle rule) {
	if (!rule) {
		return;
	}
	free(rule->name);
	for (size_t i = 0; i < rule->premise_count; i++) {
		IPK_AST_Destroy(rule->premises[i]);
	}
	DestroyObject(rule->premises);
	IPK_AST_Destroy(rule->conclusion);
	DestroyObject(rule);
}