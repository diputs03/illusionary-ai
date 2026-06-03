#include "ipk_ast.h"
#include <sstream>
#include <iostream>
#include <string>

API_EXPORT IPK_RESULT IPK_CreateSymbolAST(String_Handle name, AST_Handle* out_ast) {
    if (!name || !out_ast) {
        return IPK_ERROR_INVALID_ARGUMENT;
    }
    
    AST_Handle node = Entity<AST_Node>();
    if (!node) {
        return IPK_ERROR_OUT_OF_MEMORY;
    }

    node->type = AST_NODE_SYMBOL;
    node->data.symbol = strdup(name);

    if (!node->data.symbol) {
        FreeEntity(node);
        return IPK_ERROR_OUT_OF_MEMORY;
    }
    *out_ast = node;
    return IPK_SUCCESS;
}

API_EXPORT IPK_RESULT IPK_CreateListAST(size_t length, AST_Handle* elements, AST_Handle* out_ast) {
    if ((length > 0 && !elements) || !out_ast) {
        return IPK_ERROR_INVALID_ARGUMENT;
    }

    AST_Handle node = Entity<AST_Node>();
    if (!node) {
        return IPK_ERROR_OUT_OF_MEMORY;
    }

    node->type = AST_NODE_LIST;
    node->data.list.length = length;
    if (length > 0) {
        node->data.list.elements = Entity<AST_Handle>(length);
        if (!node->data.list.elements) {
            FreeEntity(node);
            node = nullptr;
            return IPK_ERROR_OUT_OF_MEMORY;
        }
        for (size_t i = 0; i < length; i++) {
            node->data.list.elements[i] = elements[i];
        }
    } else {
        node->data.list.elements = nullptr;
	}

    *out_ast = node;
    return IPK_SUCCESS;
}

API_EXPORT IPK_RESULT IPK_GetTypeAST(AST_Handle ast, AST_NodeType* out_type)
{
    if (!ast || !out_type) {
		return IPK_ERROR_INVALID_ARGUMENT;
    }
    *out_type = ast->type;
    return IPK_SUCCESS;
}

API_EXPORT IPK_RESULT IPK_GetSymbolAST(AST_Handle ast, String_Handle* out_name)
{
    if (!ast || !out_name) {
        return IPK_ERROR_INVALID_ARGUMENT;
	}
    if (ast->type != AST_NODE_SYMBOL) {
        return IPK_ERROR_INVALID_ARGUMENT;
	}
    *out_name = ast->data.symbol;
    return IPK_SUCCESS;
}

API_EXPORT IPK_RESULT IPK_GetListAST(AST_Handle ast, size_t index, AST_Handle* out_element)
{
    if (!ast || !out_element) {
        return IPK_ERROR_INVALID_ARGUMENT;
    }
    if (ast->type != AST_NODE_LIST) {
        return IPK_ERROR_INVALID_ARGUMENT;
    }
	*out_element = ast->data.list.elements[index];
    return IPK_SUCCESS;
}

API_EXPORT IPK_RESULT IPK_CloneAST(AST_Handle ast, AST_Handle* out_ast) {
    if (!ast || !out_ast) {
        return IPK_ERROR_INVALID_ARGUMENT;
    }

    SwitchASTNodeType(ast->type, {
        return IPK_CreateSymbolAST(ast->data.symbol, out_ast);
    }, {
        size_t length = ast->data.list.length;
        AST_Handle* elements = nullptr;
        if (length > 0) {
            elements = Entity<AST_Handle>(length);
            if (!elements) {
                return IPK_ERROR_OUT_OF_MEMORY;
            }
            for (size_t i = 0; i < length; i++) {
                res = IPK_CloneAST(ast->data.list.elements[i], &elements[i]);
                if (res != IPK_SUCCESS) {
                    for (size_t j = 0; j < i; j++) {
                        IPK_FreeAST(elements[j]);
                    }
                    FreeEntity(elements);
                    return res;
                }
            }
        }

        res = IPK_CreateListAST(length, elements, out_ast);
        if (res != IPK_SUCCESS) {
            for (size_t i = 0; i < length; i++) {
                IPK_FreeAST(elements[i]);
            }
            FreeEntity(elements);
            return res;
        }
        return IPK_SUCCESS;
    });
}

API_EXPORT bool IPK_EqualAST(AST_Handle a, AST_Handle b) {
    if (!a && !b) {
        return true;
    }
	if (!a || !b) {
        return false;
    }
	if (a->type != b->type) {
        return false;
    }

    SwitchASTNodeType(a->type, {
        return strcmp(a->data.symbol, b->data.symbol) == 0;
    }, {
        if (a->data.list.length != b->data.list.length) {
            return false;
        }
        for (size_t i = 0; i < a->data.list.length; i++) {
            if (!IPK_EqualAST(a->data.list.elements[i], b->data.list.elements[i])) {
                return false;
            }
        }
        return true;
    });
}

API_EXPORT void IPK_FreeAST(AST_Handle ast) {
    if (!ast) {
        return;
    }
    SwitchASTNodeType(ast->type, {
        //free(ast->data.symbol);
        FreeEntity(ast);
    }, {
        for (size_t i = 0; i < ast->data.list.length; i++) {
            IPK_FreeAST(ast->data.list.elements[i]);
        }
        FreeEntity(ast->data.list.elements);
        FreeEntity(ast);
        return;
    });
}

void IPK_MakeString(AST_Handle ast, size_t indent, std::ostream& os) {
    if (!ast) {
        return;
    }
    for (size_t i = 0; i < indent; i++) {
        os << "  ";
	}
    SwitchASTNodeType(ast->type, {
        os << "Symbol: " << ast->data.symbol << std::endl;
    }, {
		os << "List: length=" << ast->data.list.length << std::endl;
        for (size_t i = 0; i < ast->data.list.length; i++) {
            IPK_MakeString(ast->data.list.elements[i], indent + 1, os);
        }
    });
}

API_EXPORT void IPK_ToStringAST(AST_Handle ast, String_Handle* out_str) {
    std::ostringstream oss{};
    IPK_MakeString(ast, 0, oss);
    *out_str = strdup(oss.str().c_str());
}

API_EXPORT void IPK_PrintAST(AST_Handle ast) {
    String_Handle str;
    IPK_ToStringAST(ast, &str);
    printf("%s", str);
}
