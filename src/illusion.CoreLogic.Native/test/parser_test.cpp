#include "ipk_ast.h"
#include "ipk_parser.h"
int main() {
	String_Handle s = "(implies P (and Q R))";
	AST_Handle ast;
	IPK_RESULT res = IPK_ParseStatement(s, &ast);
	if (res == IPK_SUCCESS) {
		printf("Parsed successfully!\n");
		IPK_PrintAST(ast, 0);
		IPK_FreeAST(ast);
	} else {
		printf("Failed to parse: %s\n", IPK_GetErrorMessage(res));
	}
	return 0;
}