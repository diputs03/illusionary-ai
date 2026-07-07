#include "ipk_ast.h"
#include "ipk_parser.h"
#include "ipk_unification.h"
#include <cstdio>

int main() {
    IPK_AST_Handle ast1 = nullptr, ast2 = nullptr;
    IPK_RESULT res = IPK_ParseStatement(ITEXT("implies P (and Q R)"), &ast1);
    res = IPK_ParseStatement(ITEXT("A"), &ast2);
    IPK_String rendered = nullptr;
    res = IPK_AST_ToString(ast2, &rendered);
    ast2 = ast2->data.list.elements[0];
    std::iprintf(ITEXT("Parsed successfully!\n%s"), rendered != nullptr ? rendered : ITEXT(""));
    IPK_Sub sub;
    res = IPK_Unify(ast1, ast2, &sub);
    if (res == IPK_SUCCESS) {
        rendered = nullptr;
        res = IPK_AST_ToString(ast1, &rendered);
        std::iprintf(ITEXT("Parsed successfully!\n%s"), rendered != nullptr ? rendered : ITEXT(""));
        for (auto i : sub.pairs) {
            IPK_AST_ToString(i.replacement, &rendered);
            std::iprintf(ITEXT("%s, %s"), rendered, i.var_name);
        }
        IPK_FreeString(rendered);
        IPK_AST_Destroy(ast1);
    } else {
        std::iprintf(ITEXT("Failed to parse: %s\n"), IPK_GetErrorMessage(res));
    }
    return res == IPK_SUCCESS ? 0 : 1;
}
