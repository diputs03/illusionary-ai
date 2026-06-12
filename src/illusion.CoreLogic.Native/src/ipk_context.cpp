#include "ipk_context.h"
#include <unordered_map>

struct IPK_Context {
	std::unordered_map<index_t, IPK_AST_Handle> axioms;
	std::unordered_map<index_t, IPK_Rule_Handle> rules;
};

API_EXPORT IPK_RESULT IPK_Context_Create(IPK_Context_Handle* out_context) {
	if (!out_context) {
		return IPK_ERROR_INVALID_ARGUMENT;
	}
	*out_context = NewObject<IPK_Context>();
	if (!*out_context) {
		return IPK_ERROR_OUT_OF_MEMORY;
	}
	return IPK_SUCCESS;
}

API_EXPORT void IPK_Context_Destroy(IPK_Context_Handle context) {
	if (!context) {
		return;
	}
	for (auto& pair : context->axioms) {
		IPK_AST_Destroy(pair.second);
	}
	for (auto& pair : context->rules) {
		IPK_Rule_Destroy(pair.second);
	}
	DestroyObject(context);
}

API_EXPORT IPK_RESULT IPK_Context_AddAxiom(IPK_Context_Handle context, index_t axiom_id, IPK_AST_Handle statement) {
	if (!context || !statement || context->axioms.find(axiom_id) != context->axioms.end()) {
		return IPK_ERROR_INVALID_ARGUMENT;
	}
	context->axioms[axiom_id] = statement;
	return IPK_SUCCESS;
}

API_EXPORT IPK_RESULT IPK_Context_AddRule(IPK_Context_Handle context, index_t rule_id, IPK_Rule_Handle rule) {
	if (!context || !rule || context->rules.find(rule_id) != context->rules.end()) {
		return IPK_ERROR_INVALID_ARGUMENT;
	}
	context->rules[rule_id] = rule;
	return IPK_SUCCESS;
}