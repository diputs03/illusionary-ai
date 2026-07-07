#ifndef IPK_AST_H
#define IPK_AST_H

#include "ipk_common.h"

IENUM()
typedef enum {
	IPK_AST_NODE_SYMBOL,
	IPK_AST_NODE_LIST,
} IPK_AST_NodeType;

#define IPK_AST_SwitchNodeType(x, y, z) {if (x== IPK_AST_NODE_SYMBOL) y else z }

typedef struct IPK_AST_Node IPK_AST_Node;
typedef IPK_AST_Node* IPK_AST_Handle;

ISTRUCT()
struct IPK_AST_Node {
    IPK_AST_NodeType type;
    union {
        IPK_String symbol;
        struct {
            size_t length;
            IPK_AST_Handle* elements;
        } list;
    } data;
};


#ifdef __cplusplus
extern "C" {
#endif

    IAPI_EXPORT IPK_RESULT IPK_AST_CreateSymbol(IPK_String name, IPK_AST_Handle* out_ast);
    IAPI_EXPORT IPK_RESULT IPK_AST_CreateList(size_t length, IPK_AST_Handle* elements, IPK_AST_Handle* out_ast);

	IAPI_EXPORT IPK_RESULT IPK_AST_GetType(IPK_AST_Handle ast, IPK_AST_NodeType* out_type);
	IAPI_EXPORT IPK_RESULT IPK_AST_GetSymbol(IPK_AST_Handle ast, IPK_String* out_name);
	IAPI_EXPORT IPK_RESULT IPK_AST_GetList(IPK_AST_Handle ast, size_t index, IPK_AST_Handle* out_element);
    
    IAPI_EXPORT IPK_RESULT IPK_AST_Clone(IPK_AST_Handle ast, IPK_AST_Handle* out_ast);
    
    IAPI_EXPORT bool IPK_AST_Equal(IPK_AST_Handle a, IPK_AST_Handle b);
    IAPI_EXPORT IPK_RESULT IPK_AST_ToString(IPK_AST_Handle ast, IPK_String* out_str);
    
    IAPI_EXPORT void IPK_AST_Destroy(IPK_AST_Handle ast);

#ifdef __cplusplus
}
#endif

#endif // IPK_AST_H