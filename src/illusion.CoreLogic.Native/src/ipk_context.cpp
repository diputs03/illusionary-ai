#include "ipk_context.h"
#include <unordered_map>

struct IPK_Context {
	std::unordered_map<index_t, AST_Handle> axioms;
	std::unordered_map<String_Handle, Rule_Handle> rules;
	index_t next_id;
};

API_EXPORT IPK_RESULT IPK_CreateContext(Context_Handle* out_context) {
	if (!out_context) {
		return IPK_ERROR_INVALID_ARGUMENT;
	}
	*out_context = Entity<IPK_Context>();
	if (!*out_context) {
		return IPK_ERROR_OUT_OF_MEMORY;
	}
	out_context[0]->next_id = 0;
	return IPK_SUCCESS;
}

API_EXPORT void IPK_DestroyContext(Context_Handle context) {
	if (!context) {
		return;
	}
	for (auto& pair : context->axioms) {
		IPK_FreeAST(pair.second);
	}
	for (auto& pair : context->rules) {
		IPK_FreeRule(pair.second);
	}
	FreeEntity(context);	
}

API_EXPORT IPK_RESULT IPK_ContextAddAxiom(Context_Handle context,
	String_Handle name,
	AST_Handle statement,
	index_t* out_axiom_id) {
	if (!context || !name || !statement || !out_axiom_id) {
		return IPK_ERROR_INVALID_ARGUMENT;
	}
	index_t axiom_id = context->next_id++;
	context->axioms[axiom_id] = statement;
	*out_axiom_id = axiom_id;
	return IPK_SUCCESS;
}

API_EXPORT IPK_RESULT IPK_ContextAddRule(Context_Handle context, Rule_Handle rule) {
	if (!context || !rule) {
		return IPK_ERROR_INVALID_ARGUMENT;
	}
	context->rules[rule->name] = rule;
	return IPK_SUCCESS;
}

API_EXPORT IPK_RESULT IPK_ContextGetAxiom(Context_Handle context, index_t axiom_id, AST_Handle* out_statement) {
	if (!context || !out_statement) {
		return IPK_ERROR_INVALID_ARGUMENT;
	}
	auto it = context->axioms.find(axiom_id);
	if (it == context->axioms.end()) {
		return IPK_ERROR_NOT_FOUND;
	}
	*out_statement = it->second;
	return IPK_SUCCESS;
}

API_EXPORT IPK_RESULT IPK_ContextGetRule(Context_Handle context, String_Handle rule_name, Rule_Handle* out_rule) {
	if (!context || !rule_name || !out_rule) {
		return IPK_ERROR_INVALID_ARGUMENT;
	}
	auto it = context->rules.find(rule_name);
	if (it == context->rules.end()) {
		return IPK_ERROR_NOT_FOUND;
	}
	*out_rule = it->second;
	return IPK_SUCCESS;
}