#include "ipk_substitution.h"
#include <vector>

IAPI_EXPORT IPK_RESULT IPK_Sub_Create(IPK_Sub_Handle* out_sub) {
    if (!out_sub) {
        return IPK_ERROR_NULL_POINTER;
    }

    IPK_Sub_Handle sub = INewObject<IPK_Sub>();
    if (!sub) {
        return IPK_ERROR_OUT_OF_MEMORY;
    }
    *out_sub = sub;
    return IPK_SUCCESS;
}

IAPI_EXPORT IPK_RESULT IPK_Sub_Add(IPK_Sub_Handle sub, IPK_String var_name, IPK_AST_Handle replacement) {
    if (!sub || !var_name) {
        return IPK_ERROR_NULL_POINTER;
    }

    for (size_t i = 0; i < sub->pairs.size(); i++) {
        if (istrcmp(sub->pairs[i].var_name, var_name) == 0) {
            if (IPK_AST_Equal(sub->pairs[i].replacement, replacement)) {
                return IPK_SUCCESS;
            }
            else {
                return IPK_ERROR_UNIFICATION_FAILED;
            }
        }
    }

    sub->pairs.push_back(IPK_SubPair());
	sub->pairs.back().var_name = istrdup(var_name);
	IPK_RESULT res = IPK_AST_Clone(replacement, &sub->pairs.back().replacement);

    if (!sub->pairs.back().var_name || res != IPK_SUCCESS) {
        free(sub->pairs.back().var_name);
        IPK_AST_Destroy(sub->pairs.back().replacement);
        sub->pairs.pop_back();
        return res == IPK_SUCCESS ? IPK_ERROR_OUT_OF_MEMORY : res;
    }

    return IPK_SUCCESS;
}

IAPI_EXPORT IPK_RESULT IPK_Sub_Lookup(IPK_Sub_Handle sub, IPK_String var_name, IPK_AST_Handle* out_replacement) {
    if (!sub || !var_name || !out_replacement) {
        return IPK_ERROR_NULL_POINTER;
    }

    for (size_t i = 0; i < sub->pairs.size(); i++) {
        if (istrcmp(sub->pairs[i].var_name, var_name) == 0) {
            *out_replacement = sub->pairs[i].replacement;
            return IPK_SUCCESS;
        }
    }

    return IPK_ERROR_NOT_FOUND;
}

IAPI_EXPORT IPK_RESULT IPK_Sub_Apply(IPK_Sub_Handle sub, IPK_AST_Handle node, IPK_AST_Handle* out_result) {
    if (!sub || !node || !out_result) {
        return IPK_ERROR_NULL_POINTER;
    }

    IPK_AST_SwitchNodeType(node->type, {
		IPK_AST_Handle replacement;
        IPK_RESULT res = IPK_Sub_Lookup(sub, node->data.symbol, &replacement);
        if (res == IPK_SUCCESS) {
			return IPK_AST_Clone(replacement, out_result);
        } else if (res == IPK_ERROR_NOT_FOUND) {
            return IPK_AST_Clone(node, out_result);
        } else {
            return res;
		}
    }, {
		size_t length = node->data.list.length;
        IPK_AST_Handle* new_elements = INewObject<IPK_AST_Handle>(length);
        if (!new_elements) {
            return IPK_ERROR_OUT_OF_MEMORY;
        }
        for (size_t i = 0; i < length; i++) {
            IPK_RESULT res = IPK_Sub_Apply(sub, node->data.list.elements[i], &new_elements[i]);
            if (res != IPK_SUCCESS) {
                for (size_t j = 0; j < i; j++) {
                    IPK_AST_Destroy(new_elements[j]);
                }
                IDestroyObject(new_elements);
                return res;
            }
        }
		IPK_RESULT res = IPK_AST_CreateList(length, new_elements, out_result);
        for (size_t i = 0; i < length; i++) {
            IPK_AST_Destroy(new_elements[i]);
        }
        IDestroyObject(new_elements);
        return res;
    });
    return IPK_ERROR;
}

IAPI_EXPORT void IPK_Sub_Destroy(IPK_Sub_Handle sub) {
    if (!sub) {
        return;
    }

    for (size_t i = 0; i < sub->pairs.size(); i++) {
        free(sub->pairs[i].var_name);
        IPK_AST_Destroy(sub->pairs[i].replacement);
    }
    IDestroyObject(sub);
}