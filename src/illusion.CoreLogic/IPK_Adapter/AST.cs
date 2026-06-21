using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using illusion.Common.Types;


namespace illusion.IPK_Adapter
{
    public enum AST_NodeType
    {
        Symbol,
        List
    }
    public class AST : IEquatable<AST>
    {
        internal nint NativeHandle { get; private set; }
        public AST(nint handle = default)
        {
            NativeHandle = handle;
        }
        public AST_NodeType Type()
        {
            AST_NodeType type;
            IPK_BaseMethods.CheckResult(IPK_BaseMethods.IPK_GetTypeAST(NativeHandle, out type));
            return type;
        }
        public string Symbol()
        {
            nint symbol;
            IPK_BaseMethods.CheckResult(IPK_BaseMethods.IPK_GetSymbolAST(NativeHandle, out symbol));
            return Marshal.PtrToStringAnsi(symbol) ?? string.Empty;
        }
        public AST List(Size_t index)
        {
            nint element;
            IPK_BaseMethods.CheckResult(IPK_BaseMethods.IPK_GetListAST(NativeHandle, index, out element));
            return new AST(element);
        }
        public static AST CreateSymbol(string name)
        {
            nint ast;
            IPK_BaseMethods.CheckResult(IPK_BaseMethods.IPK_CreateSymbolAST(name, out ast));
            return new AST(ast);
        }
        public static AST CreateList(Size_t len, nint[] elements)
        {
            nint ast;
            IPK_BaseMethods.CheckResult(IPK_BaseMethods.IPK_CreateListAST(len, elements, out ast));
            return new AST(ast);
        }

        public bool Equals(AST? other)
        {
            if (other is null) return false;
            return IPK_BaseMethods.IPK_EqualAST(NativeHandle, other.NativeHandle);
        }

        public AST Clone()
        {
            nint ast;
            IPK_BaseMethods.CheckResult(IPK_BaseMethods.IPK_CloneAST(NativeHandle, out ast));
            return new AST(ast);
        }

        public static AST Parse(string line)
        {
            nint ast;
            IPK_BaseMethods.CheckResult(IPK_BaseMethods.IPK_ParseStatement(line, out ast));
            return new AST(ast);
        }

        public override string ToString()
        {
            nint str;
            IPK_BaseMethods.CheckResult(IPK_BaseMethods.IPK_ToStringAST(NativeHandle, out str));
            try
            {
                return Marshal.PtrToStringAnsi(str) ?? string.Empty;
            }
            finally
            {
                IPK_BaseMethods.IPK_FreeString(str);
            }
        }

        ~AST()
        {
            if (NativeHandle != nint.Zero)
            {
                IPK_BaseMethods.IPK_FreeAST(NativeHandle);
                NativeHandle = nint.Zero;
            }
        }
    }
}