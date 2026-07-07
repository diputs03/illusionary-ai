#include <sstream>
#include <iostream>
#include <string>
#include <functional>
#include "ipk_ast.h"

IAPI_EXPORT IPK_RESULT IPK_AST_CreateSymbol(IPK_String name, IPK_AST_Handle* out_ast) {
    if (!name || !out_ast) {
        return IPK_ERROR_INVALID_ARGUMENT;
    }
    
    IPK_AST_Handle node = INewObject<IPK_AST_Node>();
    if (!node) {
        return IPK_ERROR_OUT_OF_MEMORY;
    }

    node->type = IPK_AST_NODE_SYMBOL;
    node->data.symbol = INewObject<IPK_Char, false>(istrlen(name), name);

    if (!node->data.symbol) {
        IDestroyObject(node);
        return IPK_ERROR_OUT_OF_MEMORY;
    }
    *out_ast = node;
    return IPK_SUCCESS;
}

IAPI_EXPORT IPK_RESULT IPK_AST_CreateList(size_t length, IPK_AST_Handle* elements, IPK_AST_Handle* out_ast) {
    if ((length > 0 && !elements) || !out_ast) {
        return IPK_ERROR_INVALID_ARGUMENT;
    }

    IPK_AST_Handle node = INewObject<IPK_AST_Node>();
    if (!node) {
        return IPK_ERROR_OUT_OF_MEMORY;
    }

    node->data.list.elements = INewObject<IPK_AST_Handle>(length);
    if (!node->data.list.elements) {
        IDestroyObject(node);
        return IPK_ERROR_OUT_OF_MEMORY;
    }

    node->type = IPK_AST_NODE_LIST;
    node->data.list.length = length;
    for (size_t i = 0; i < length; i++) {
        IPK_RESULT res = IPK_AST_Clone(elements[i], &node->data.list.elements[i]);
        if (res != IPK_SUCCESS) {
            for (size_t j = 0; j < i; j++) {
                IPK_AST_Destroy(node->data.list.elements[j]);
            }
            IDestroyObject(node->data.list.elements);
            IDestroyObject(node);
            return res;
        }
    }

    *out_ast = node;
    return IPK_SUCCESS;
}

IAPI_EXPORT IPK_RESULT IPK_AST_GetType(IPK_AST_Handle ast, IPK_AST_NodeType* out_type) {
    if (!ast || !out_type) {
		return IPK_ERROR_INVALID_ARGUMENT;
    }
    *out_type = ast->type;
    return IPK_SUCCESS;
}

IAPI_EXPORT IPK_RESULT IPK_AST_GetSymbol(IPK_AST_Handle ast, IPK_String* out_name) {
    if (!ast || !out_name) {
        return IPK_ERROR_INVALID_ARGUMENT;
	}
    if (ast->type != IPK_AST_NODE_SYMBOL) {
        return IPK_ERROR_INVALID_ARGUMENT;
	}
    *out_name = ast->data.symbol;
    return IPK_SUCCESS;
}

IAPI_EXPORT IPK_RESULT IPK_AST_GetList(IPK_AST_Handle ast, size_t index, IPK_AST_Handle* out_element) {
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

IAPI_EXPORT IPK_RESULT IPK_AST_Clone(IPK_AST_Handle ast, IPK_AST_Handle* out_ast) {
    if (!ast || !out_ast) {
        return IPK_ERROR_INVALID_ARGUMENT;
    }

    if (ast->type == IPK_AST_NODE_SYMBOL) {
        return IPK_AST_CreateSymbol(ast->data.symbol, out_ast);
    }

    IPK_AST_Handle node = INewObject<IPK_AST_Node>();
    if (!node) {
        return IPK_ERROR_OUT_OF_MEMORY;
    }

    node->type = IPK_AST_NODE_LIST;
    node->data.list.length = ast->data.list.length;
    node->data.list.elements = nullptr;

    if (node->data.list.length > 0) {
        node->data.list.elements = INewObject<IPK_AST_Handle>(node->data.list.length);
        if (!node->data.list.elements) {
            IDestroyObject(node);
            return IPK_ERROR_OUT_OF_MEMORY;
        }

        for (size_t i = 0; i < node->data.list.length; i++) {
            IPK_RESULT res = IPK_AST_Clone(ast->data.list.elements[i], &node->data.list.elements[i]);
            if (res != IPK_SUCCESS) {
                for (size_t j = 0; j < i; j++) {
                    IPK_AST_Destroy(node->data.list.elements[j]);
                }
                IDestroyObject(node->data.list.elements);
                IDestroyObject(node);
                return res;
            }
        }
    }

    *out_ast = node;
    return IPK_SUCCESS;
}

IAPI_EXPORT bool IPK_AST_Equal(IPK_AST_Handle a, IPK_AST_Handle b) {
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
        return istrcmp(a->data.symbol, b->data.symbol) == 0;
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

IAPI_EXPORT void IPK_AST_Destroy(IPK_AST_Handle ast) {
    if (!ast) {
        return;
    }
    IPK_AST_SwitchNodeType(ast->type, {
        IDestroyObject(ast->data.symbol);
        IDestroyObject(ast);
    }, {
        for (size_t i = 0; i < ast->data.list.length; i++) {
            IPK_AST_Destroy(ast->data.list.elements[i]);
        }
        IDestroyObject(ast->data.list.elements);
        IDestroyObject(ast);
        return;
    });
}

IAPI_EXPORT IPK_RESULT IPK_AST_ToString(IPK_AST_Handle ast, IPK_String* out_str) {
    if (!out_str) {
        return IPK_ERROR_NULL_POINTER;
    }

    std::iostringstream oss{};

    std::function<void(IPK_AST_Handle, size_t)> IPK_MakeString;
    IPK_MakeString = [&](IPK_AST_Handle ast, size_t indent) -> void {
        if (!ast) {
            return;
        }
        for (size_t i = 0; i < indent; i++) {
            oss << "  ";
        }
        IPK_AST_SwitchNodeType(ast->type, {
            oss << ITEXT("Symbol: ") << ast->data.symbol << ITEXT('\n');
            }, {
                oss << ITEXT("List: length=") << ast->data.list.length << ITEXT('\n');
                for (size_t i = 0; i < ast->data.list.length; i++) {
                    IPK_MakeString(ast->data.list.elements[i], indent + 1);
                }
            });
        };
    IPK_MakeString(ast, 0);
    std::istring str = oss.str();
    *out_str = INewObject<IPK_Char, false>(str.length(), oss.str().c_str());
    return IPK_SUCCESS;
}