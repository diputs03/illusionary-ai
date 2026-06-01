#ifndef IPK_AST_H
#define IPK_AST_H

#include "ipk_common.h"

typedef enum {
	AST_NODE_SYMBOL,
	AST_NODE_LIST,
} AST_NodeType;

#define SwitchASTNodeType(x, y, z) {if (x== AST_NODE_SYMBOL) y else z }

typedef struct AST_Node AST_Node;
typedef AST_Node* AST_Handle;

struct AST_Node {
    AST_NodeType type;
    union {
        String_Handle symbol;
        struct {
            size_t length;
            AST_Handle* elements;
        } list;
    } data;
    String_Handle preorder;
};


#ifdef __cplusplus
extern "C" {
#endif

    API_EXPORT IPK_RESULT IPK_CreateSymbolAST(String_Handle name, AST_Handle* out_ast);
    API_EXPORT IPK_RESULT IPK_CreateListAST(size_t length, AST_Handle* elements, AST_Handle* out_ast);
    API_EXPORT IPK_RESULT IPK_CloneAST(AST_Handle ast, AST_Handle* out_ast);
    API_EXPORT bool IPK_EqualAST(AST_Handle a, AST_Handle b);
    API_EXPORT void IPK_FreeAST(AST_Handle ast);
    API_EXPORT void IPK_PrintAST(AST_Handle ast, size_t indent);

#ifdef __cplusplus
}
#endif

#endif // IPK_AST_H