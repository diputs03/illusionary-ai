using illusion.Common.Types;
using illusion.CoreLogic.Kernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;


namespace illusion.CoreLogic.IPK_Adapter;
public class AST : IStatement<AST>
{
    public enum NodeType
    {
        Symbol,
        List
    }
    internal nint NativeHandle { get; private set; }
    public AST(nint handle = default)
    {
        NativeHandle = handle;
    }
    public NodeType Type()
    {
        IPK_BaseMethods.CheckResult(IPK_BaseMethods.IPK_AST_GetType(NativeHandle, out NodeType type));
        return type;
    }
    public string Symbol()
    {
        IPK_BaseMethods.CheckResult(IPK_BaseMethods.IPK_AST_GetSymbol(NativeHandle, out nint symbol));
        return Utils.GetNativeString(symbol);
    }
    public AST List(Size_t index)
    {
        IPK_BaseMethods.CheckResult(IPK_BaseMethods.IPK_AST_GetList(NativeHandle, index, out nint element));
        return new AST(element);
    }
    public static AST CreateSymbol(string name)
    {
        IPK_BaseMethods.CheckResult(IPK_BaseMethods.IPK_AST_CreateSymbol(Utils.CreateNativeString(name), out nint ast));
        return new AST(ast);
    }
    public static AST CreateList(Size_t len, AST[] elements)
    {
        IPK_BaseMethods.CheckResult(IPK_BaseMethods.IPK_AST_CreateList(len, elements.Select(e => e.NativeHandle).ToArray(), out nint ast));
        return new AST(ast);
    }

    public bool Equals(AST? other)
    {
        switch (other)
        {
            case null:
                return false;
            default:
                return IPK_BaseMethods.IPK_AST_Equal(NativeHandle, other.NativeHandle);
        }
        
    }

    public object Clone()
    {
        IPK_BaseMethods.CheckResult(IPK_BaseMethods.IPK_AST_Clone(NativeHandle, out nint ast));
        return new AST(ast);
    }

    public static AST Parse(string line)
    {
        IPK_BaseMethods.CheckResult(IPK_BaseMethods.IPK_ParseStatement(line, out nint ast));
        return new AST(ast);
    }

    public override string ToString()
    {
        IPK_BaseMethods.CheckResult(IPK_BaseMethods.IPK_AST_ToString(NativeHandle, out nint str));
        return Utils.GetNativeString(str);
    }

    public void Dispose()
    {
        IPK_BaseMethods.IPK_AST_Destroy(NativeHandle);
    }
}