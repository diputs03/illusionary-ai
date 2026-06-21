#include "ipk_parser.h"

enum LexState {
    LEX_STATE_NORMAL,
    LEX_STATE_IN_SYMBOL
};
static IPK_String current_pos;
static LexState state;
static std::vector<Char> current_symbol;

static void lex_reset(IPK_String input) {
    current_pos = input;
    state = LEX_STATE_NORMAL;
    current_symbol.clear();
}
static int lex_next_token() {
    while (*current_pos != '\0') {
        Char c = *current_pos;

        switch (state) {
        case LEX_STATE_NORMAL:
            if (isspace(c)) {
                current_pos++;
            }
            else if (c == '(' || c == ')') {
                current_pos++;
                return c;
            }
            else {
                state = LEX_STATE_IN_SYMBOL;
                current_symbol.clear();
                current_symbol.push_back(c);
                current_pos++;
            }
            break;

        case LEX_STATE_IN_SYMBOL:
            if (isspace(c) || c == '(' || c == ')') {
                state = LEX_STATE_NORMAL;
                current_symbol.push_back('\0');
                return '$';
            }
            else {
                current_symbol.push_back(c);
                current_pos++;
            }
            break;
        }
    }

    if (state == LEX_STATE_IN_SYMBOL) {
        state = LEX_STATE_NORMAL;
        current_symbol.push_back('\0');
        return '$';
    }

    return 0; // EOF
}

API_EXPORT IPK_RESULT IPK_ParseStatement(IPK_String s_expression, IPK_AST_Handle* out_ast) {
    if (!s_expression || !out_ast) {
        return IPK_ERROR_NULL_POINTER;
    }

    *out_ast = nullptr;
    lex_reset(s_expression);

    std::stack<std::vector<IPK_AST_Handle>> parse_stack;
	int depth = 0;

    auto cleanup_error = [&]() {
        while (!parse_stack.empty()) {
            auto elements = parse_stack.top();
            parse_stack.pop();
            for (auto elem : elements) {
                IPK_AST_Destroy(elem);
            }
        }

        if (out_ast && *out_ast) {
            IPK_AST_Destroy(*out_ast);
            *out_ast = nullptr;
        }
    };

    auto makelist = [&](std::vector<IPK_AST_Handle>& elements) -> IPK_AST_Handle {
        IPK_AST_Handle* elements_array = nullptr;
        if (!elements.empty()) {
            elements_array = NewObject<IPK_AST_Handle>(elements.size());
            for (size_t i = 0; i < elements.size(); i++) {
                elements_array[i] = elements[i];
            }
        }

        IPK_AST_Handle list_node;
        IPK_RESULT res = IPK_AST_CreateList(elements.size(), elements_array, &list_node);

        delete[] elements_array;
        return list_node;
    };

    int token;
    while ((token = lex_next_token()) != 0) {
        if (token == '(') {
            parse_stack.push(std::vector<IPK_AST_Handle>());
			depth++;
        }
        else if (token == ')') {
            if (parse_stack.empty()) {
                cleanup_error();
                return IPK_ERROR_INVALID_ARGUMENT;
            }
			depth--;

            auto elements = parse_stack.top();
            parse_stack.pop();

            if (parse_stack.empty()) {
				parse_stack.push(std::vector<IPK_AST_Handle>());
            }
            parse_stack.top().push_back(makelist(elements));
        }
        else if (token == '$') {
            IPK_AST_Handle symbol_node;
            IPK_RESULT res = IPK_AST_CreateSymbol(current_symbol.data(), &symbol_node);
            if (res != IPK_SUCCESS) {
                cleanup_error();
                return IPK_ERROR_INVALID_ARGUMENT;
            }
            if (parse_stack.empty()) {
                parse_stack.push(std::vector<IPK_AST_Handle>());
            }
            parse_stack.top().push_back(symbol_node);
        }
        else {
            cleanup_error();
            return IPK_ERROR_INVALID_ARGUMENT;
        }
    }

    if (depth != 0) {
        cleanup_error();
        return IPK_ERROR_INVALID_ARGUMENT;
    }
    if (parse_stack.empty()) {
        return IPK_ERROR_SYNTAX_ERROR;
    }
    *out_ast = makelist(parse_stack.top());
    return *out_ast ? IPK_SUCCESS : IPK_ERROR_OUT_OF_MEMORY;
}