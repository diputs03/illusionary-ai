#include "ipk_substitution.h"

IPK_RESULT IPK_CreateSubstitution(Substitution_Handle* out_sub) {
    if (!out_sub) {
        return IPK_ERROR_NULL_POINTER;
    }
	*out_sub = nullptr;

    Substitution_Handle sub = Entity<IPK_Substitution>();
    if (!sub) {
        return IPK_ERROR_OUT_OF_MEMORY;
    }
    sub->count = 0;
    sub->pairs = nullptr;
    *out_sub = sub;
    return IPK_SUCCESS;
}

IPK_RESULT IPK_AddSubstitution(Substitution_Handle sub, String_Handle var_name, AST_Handle replacement) {
    if (!sub || !var_name) {
        return IPK_ERROR_NULL_POINTER;
    }

    for (size_t i = 0; i < sub->count; i++) {
        if (strcmp(sub->pairs[i].var_name, var_name) == 0) {
            if (IPK_EqualAST(sub->pairs[i].replacement, replacement)) {
                return IPK_SUCCESS;
            }
            else {
                return IPK_ERROR_UNIFICATION_FAILED;
            }
        }
    }

    IPK_SubstitutionPair* new_pairs = Entity<IPK_SubstitutionPair>(sub->count + 1);
    if (!new_pairs) {
        return IPK_ERROR_OUT_OF_MEMORY;
    }

    for (uint32_t i = 0; i < sub->count; i++) {
        new_pairs[i] = sub->pairs[i];
    }

    new_pairs[sub->count].var_name = strdup(var_name);
    IPK_RESULT res = IPK_CloneAST(replacement, &new_pairs[sub->count].replacement);

    if (!new_pairs[sub->count].var_name || !new_pairs[sub->count].replacement) {
        free(new_pairs[sub->count].var_name);
        IPK_FreeAST(new_pairs[sub->count].replacement);
        FreeEntity(new_pairs);
        return IPK_ERROR_OUT_OF_MEMORY;
    }

    FreeEntity(sub->pairs);
    sub->pairs = new_pairs;
    sub->count++;

    return IPK_SUCCESS;
}

IPK_RESULT IPK_LookupSubstitution(Substitution_Handle sub, String_Handle var_name, AST_Handle* out_replacement) {
    if (!sub || !var_name || !out_replacement) {
        return IPK_ERROR_NULL_POINTER;
    }
	*out_replacement = nullptr;

    for (size_t i = 0; i < sub->count; i++) {
        if (strcmp(sub->pairs[i].var_name, var_name) == 0) {
            *out_replacement = sub->pairs[i].replacement;
            return IPK_SUCCESS;
        }
    }

    return IPK_ERROR_NOT_FOUND;
}

IPK_RESULT IPK_ApplySubstitution(Substitution_Handle sub, AST_Handle node, AST_Handle* out_result) {
    if (!sub || !node || !out_result) {
        return IPK_ERROR_NULL_POINTER;
    }
	*out_result = nullptr;

	IPK_RESULT res;
    SwitchASTNodeType(node->type, {
		AST_Handle replacement;
        res = IPK_LookupSubstitution(sub, node->data.symbol, &replacement);
        if (res == IPK_SUCCESS) {
			return IPK_CloneAST(replacement, out_result);
        } else if (res == IPK_ERROR_NOT_FOUND) {
            return IPK_CloneAST(node, out_result);
        } else {
            return res;
		}
    }, {
		uint16_t length = node->data.list.length;
        AST_Handle* new_elements = Entity<AST_Handle>(length);
        if (!new_elements) {
            return IPK_ERROR_OUT_OF_MEMORY;
        }
        for (size_t i = 0; i < length; i++) {
            res = IPK_ApplySubstitution(sub, node->data.list.elements[i], &new_elements[i]);
            if (!res) {
                for (size_t j = 0; j < i; j++) {
                    IPK_FreeAST(new_elements[j]);
                }
                FreeEntity(new_elements);
                return res;
            }
        }
		res = IPK_CreateListAST(length, new_elements, out_result);
        for (size_t i = 0; i < length; i++) {
            IPK_FreeAST(new_elements[i]);
        }
        FreeEntity(new_elements);
        return res;
    });
    return IPK_ERROR;
}

void IPK_FreeSubstitution(Substitution_Handle sub) {
    if (!sub) {
        return;
    }

    for (size_t i = 0; i < sub->count; i++) {
        free(sub->pairs[i].var_name);
        IPK_FreeAST(sub->pairs[i].replacement);
    }

    FreeEntity(sub->pairs);
    FreeEntity(sub);
}