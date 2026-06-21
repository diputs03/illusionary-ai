#include "ipk_ast.h"
#include "ipk_parser.h"
#include <cstdio>

int main() {
    IPK_String s = const_cast<IPK_String>("implies P (and Q R)");
    IPK_AST_Handle ast = nullptr;
    IPK_RESULT res = IPK_ParseStatement(s, &ast);
    if (res == IPK_SUCCESS) {
        IPK_String rendered = nullptr;
        IPK_AST_ToString(ast, &rendered);
        std::printf("Parsed successfully!\n%s", rendered ? rendered : "");
        IPK_FreeString(rendered);
        IPK_AST_Destroy(ast);
    } else {
        std::printf("Failed to parse: %s\n", IPK_GetErrorMessage(res));
    }
    return res == IPK_SUCCESS ? 0 : 1;
}
