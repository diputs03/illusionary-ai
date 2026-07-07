#ifndef IPK_PARSER
#define IPK_PARSER

#include "ipk_ast.h"
#include <cstring>
#include <cctype>
#include <vector>
#include <stack>

#ifdef __cplusplus
extern "C" {
#endif

    IAPI_EXPORT IPK_RESULT IPK_ParseStatement(IPK_String s_expression, IPK_AST_Handle* out_ast);

#ifdef __cplusplus
}
#endif


#endif // !IPK_PARSER