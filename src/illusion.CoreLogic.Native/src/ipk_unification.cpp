#include "ipk_unification.h"

API_EXPORT bool IPK_IsMetaVariable(IPK_String symbol_name) {
	if (!symbol_name || !*symbol_name) {
		return false;
	}
	return isupper(symbol_name[0]);
}

API_EXPORT IPK_RESULT IPK_Unify(IPK_AST_Handle pattern, IPK_AST_Handle term, IPK_Sub_Handle sub) {
	if (!pattern || !term || !sub) {
		return IPK_ERROR_NULL_POINTER;
	}
	if (pattern->type == IPK_AST_NODE_SYMBOL && IPK_IsMetaVariable(pattern->data.symbol)) {
		return IPK_Sub_Add(sub, pattern->data.symbol, term);
	}
	if (term->type == IPK_AST_NODE_SYMBOL && IPK_IsMetaVariable(term->data.symbol)) {
		return IPK_Sub_Add(sub, term->data.symbol, pattern);
	}
	if (pattern->type == IPK_AST_NODE_SYMBOL && term->type == IPK_AST_NODE_SYMBOL) {
		if (ipk_strcmp(pattern->data.symbol, term->data.symbol) == 0) {
			return IPK_SUCCESS;
		} else {
			return IPK_ERROR_UNIFICATION_FAILED;
		}
	}
	if (pattern->type == IPK_AST_NODE_LIST && term->type == IPK_AST_NODE_LIST) {
		if (pattern->data.list.length != term->data.list.length) {
			return IPK_ERROR_UNIFICATION_FAILED;
		}
		for (size_t i = 0; i < pattern->data.list.length; i++) {
			IPK_RESULT res = IPK_Unify(pattern->data.list.elements[i], term->data.list.elements[i], sub);
			if (res != IPK_SUCCESS) {
				return res;
			}
		}
		return IPK_SUCCESS;
	}
	return IPK_ERROR_UNIFICATION_FAILED;
}