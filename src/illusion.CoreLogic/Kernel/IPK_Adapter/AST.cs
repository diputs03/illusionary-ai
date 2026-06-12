using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using illusion.Common.Types;


namespace illusion.Kernel.IPK_Adapter
{
    public enum AST_NodeType
    {
        Symbol,
        List
    }
    public class AST : IEquatable<AST>
    {
        internal IntPtr NativeHandle { get; private set; }
        public AST(IntPtr handle = default)
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
            string symbol;
            IPK_BaseMethods.CheckResult(IPK_BaseMethods.IPK_GetSymbolAST(NativeHandle, out symbol));
            return symbol;
        }
        public AST List(Size_t index)
        {
            IntPtr element;
            IPK_BaseMethods.CheckResult(IPK_BaseMethods.IPK_GetListAST(NativeHandle, index, out element));
            return new AST(element);
        }
        public static AST CreateSymbol(string name)
        {
            IntPtr ast;
            IPK_BaseMethods.CheckResult(IPK_BaseMethods.IPK_CreateSymbolAST(name, out ast));
            return new AST(ast);
        }
        public static AST CreateList(Size_t len, IntPtr[] elements)
        {
            IntPtr ast;
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
            IntPtr ast;
            IPK_BaseMethods.CheckResult(IPK_BaseMethods.IPK_CloneAST(NativeHandle, out ast));
            return new AST(ast);
        }

        public static AST Parse(string line)
        {
            IntPtr ast;
            IPK_BaseMethods.CheckResult(IPK_BaseMethods.IPK_ParseStatement(line, out ast));
            return new AST(ast);
        }

        public override string ToString()
        {
            string str;
            IPK_BaseMethods.IPK_ToStringAST(NativeHandle, out str);
            return str;
        }

        ~AST()
        {
            IPK_BaseMethods.IPK_FreeAST(NativeHandle);
            NativeHandle = IntPtr.Zero;
        }
    }
}