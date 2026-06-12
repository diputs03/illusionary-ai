#include <sstream>
#include <iostream>
#include <string>
#include <functional>
#include "ipk_ast.h"

API_EXPORT IPK_RESULT IPK_AST_CreateSymbol(IPK_String name, IPK_AST_Handle* out_ast) {
    if (!name || !out_ast) {
        return IPK_ERROR_INVALID_ARGUMENT;
    }
    
    IPK_AST_Handle node = NewObject<IPK_AST_Node>();
    if (!node) {
        return IPK_ERROR_OUT_OF_MEMORY;
    }

    node->type = IPK_AST_NODE_SYMBOL;
    node->data.symbol = name;

    if (!node->data.symbol) {
        DestroyObject(node);
        return IPK_ERROR_OUT_OF_MEMORY;
    }
    *out_ast = node;
    return IPK_SUCCESS;
}

API_EXPORT IPK_RESULT IPK_AST_CreateList(size_t length, IPK_AST_Handle* elements, IPK_AST_Handle* out_ast) {
    if ((length > 0 && !elements) || !out_ast) {
        return IPK_ERROR_INVALID_ARGUMENT;
    }

    IPK_AST_Handle node = NewObject<IPK_AST_Node>();
    if (!node) {
        return IPK_ERROR_OUT_OF_MEMORY;
    }

    node->data.list.elements = NewObject<IPK_AST_Handle>(length);
    if (!node->data.list.elements) {
        DestroyObject(node);
        return IPK_ERROR_OUT_OF_MEMORY;
    }

    node->type = IPK_AST_NODE_LIST;
    node->data.list.length = length;
    for (size_t i = 0; i < length; i++) {
        IPK_AST_Clone(elements[i], &node->data.list.elements[i]);
        if (res != IPK_SUCCESS) {
            for (size_t j = 0; j < i; j++) {
                IPK_AST_Destroy(elements[j]);
            }
            DestroyObject(elements);
            DestroyObject(node);
            return res;
        }
    }

    *out_ast = node;
    return IPK_SUCCESS;
}

API_EXPORT IPK_RESULT IPK_AST_GetType(IPK_AST_Handle ast, IPK_AST_NodeType* out_type) {
    if (!ast || !out_type) {
		return IPK_ERROR_INVALID_ARGUMENT;
    }
    *out_type = ast->type;
    return IPK_SUCCESS;
}

API_EXPORT IPK_RESULT IPK_AST_GetSymbol(IPK_AST_Handle ast, IPK_String* out_name) {
    if (!ast || !out_name) {
        return IPK_ERROR_INVALID_ARGUMENT;
	}
    if (ast->type != IPK_AST_NODE_SYMBOL) {
        return IPK_ERROR_INVALID_ARGUMENT;
	}
    *out_name = ast->data.symbol;
    return IPK_SUCCESS;
}

API_EXPORT IPK_RESULT IPK_AST_GetList(IPK_AST_Handle ast, size_t index, IPK_AST_Handle* out_element) {
    if (!ast || !out_element) {
        return IPK_ERROR_INVALID_ARGUMENT;
    }
    if (ast->type != IPK_AST_NODE_LIST) {
        return IPK_ERROR_INVALID_ARGUMENT;
    }
    if (ast->data.list.length <= index) {
        return IPK_ERROR_INVALID_ARGUMENT;
    }
	*out_element = ast->data.list.elements[index];
    return IPK_SUCCESS;
}

API_EXPORT IPK_RESULT IPK_AST_Clone(IPK_AST_Handle ast, IPK_AST_Handle* out_ast) {
    if (!ast || !out_ast) {
        return IPK_ERROR_INVALID_ARGUMENT;
    }

    IPK_AST_SwitchNodeType(ast->type, {
        return IPK_AST_CreateSymbol(ast->data.symbol, out_ast);
    }, {
        size_t length = ast->data.list.length;

        IPK_AST_Handle* elements = NewObject<IPK_AST_Handle>(length);
        if (!elements) {
            return IPK_ERROR_OUT_OF_MEMORY;
        }

        for (size_t i = 0; i < length; i++) {
            res = IPK_AST_Clone(ast->data.list.elements[i], &elements[i]);
            if (res != IPK_SUCCESS) {
                for (size_t j = 0; j < i; j++) {
                    IPK_AST_Destroy(elements[j]);
                }
                DestroyObject(elements);
                return res;
            }
        }

        res = IPK_AST_CreateList(length, elements, out_ast);
        if (res != IPK_SUCCESS) {
            for (size_t i = 0; i < length; i++) {
                IPK_AST_Destroy(elements[i]);
            }
            DestroyObject(elements);
            return res;
        }
        return IPK_SUCCESS;
    });
}

API_EXPORT bool IPK_AST_Equal(IPK_AST_Handle a, IPK_AST_Handle b) {
    if (!a && !b) {
        return true;
    }
	if (!a || !b) {
        return false;
    }
	if (a->type != b->type) {
        return false;
    }

    IPK_AST_SwitchNodeType(a->type, {
        return strcmp(a->data.symbol, b->data.symbol) == 0;
    }, {
        if (a->data.list.length != b->data.list.length) {
            return false;
        }
        for (size_t i = 0; i < a->data.list.length; i++) {
            if (!IPK_AST_Equal(a->data.list.elements[i], b->data.list.elements[i])) {
                return false;
            }
        }
        return true;
    });
}

API_EXPORT void IPK_AST_Destroy(IPK_AST_Handle ast) {
    if (!ast) {
        return;
    }
    IPK_AST_SwitchNodeType(ast->type, {
        //free(ast->data.symbol);
        DestroyObject(ast);
    }, {
        for (size_t i = 0; i < ast->data.list.length; i++) {
            IPK_AST_Destroy(ast->data.list.elements[i]);
        }
        DestroyObject(ast->data.list.elements);
        DestroyObject(ast);
        return;
    });
}

API_EXPORT IPK_RESULT IPK_AST_ToString(IPK_AST_Handle ast, IPK_String* out_str) {
    if (!out_str) {
        return IPK_ERROR_NULL_POINTER;
    }

    std::ostringstream oss{};

    std::function<void(IPK_AST_Handle, size_t)> IPK_MakeString;
    IPK_MakeString = [&](IPK_AST_Handle ast, size_t indent) -> void {
        if (!ast) {
            return;
        }
        for (size_t i = 0; i < indent; i++) {
            oss << "  ";
        }
        IPK_AST_SwitchNodeType(ast->type, {
            oss << TEXT("Symbol: ") << ast->data.symbol << TEXT('\n');
            }, {
                oss << TEXT("List: length=") << ast->data.list.length << TEXT('\n');
                for (size_t i = 0; i < ast->data.list.length; i++) {
                    IPK_MakeString(ast->data.list.elements[i], indent + 1);
                }
            });
        };
    IPK_MakeString(ast, 0);
    *out_str = strdup(oss.str().c_str());
    return IPK_SUCCESS;
}