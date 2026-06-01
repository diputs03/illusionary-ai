#include "ipk_ast.h"

API_EXPORT IPK_RESULT IPK_CreateSymbolAST(String_Handle name, AST_Handle* out_ast) {
    if (!name || !out_ast) {
        return IPK_ERROR_INVALID_ARGUMENT;
    }
	*out_ast = nullptr;
    
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
	*out_ast = nullptr;

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

API_EXPORT IPK_RESULT IPK_CloneAST(AST_Handle ast, AST_Handle* out_ast) {
    if (!ast || !out_ast) {
        return IPK_ERROR_INVALID_ARGUMENT;
    }
	*out_ast = nullptr;

	IPK_RESULT res;
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
                if (!res) {
                    for (size_t j = 0; j < i; j++) {
                        IPK_FreeAST(elements[j]);
                    }
                    FreeEntity(elements);
                    return res;
                }
            }
        }

        res = IPK_CreateListAST(length, elements, out_ast);
        if (!res) {
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

API_EXPORT void IPK_PrintAST(AST_Handle ast, size_t indent) {
    if (!ast) {
        return;
    }
    for (size_t i = 0; i < indent; i++) {
        printf("  ");
    }
    SwitchASTNodeType(ast->type, {
        printf("Symbol: %s\n", ast->data.symbol);
    }, {
        printf("List: length=%llu\n", ast->data.list.length);
        for (size_t i = 0; i < ast->data.list.length; i++) {
            IPK_PrintAST(ast->data.list.elements[i], indent + 1);
        }
        return;
    });
}
