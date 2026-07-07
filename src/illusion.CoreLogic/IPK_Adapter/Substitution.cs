using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace illusion.CoreLogic.IPK_Adapter;
public class Substitution
{
    internal nint NativeHandle { get; private set; }
    public Substitution(nint handle = default)
    {
        NativeHandle = handle;
    }
    public static Substitution Create()
    {
        IPK_BaseMethods.CheckResult(IPK_BaseMethods.IPK_Sub_Create(out nint subst));
        return new Substitution(subst);
    }
    public void Add(string var, AST value)
    {
        IPK_BaseMethods.CheckResult(IPK_BaseMethods.IPK_Sub_Add(NativeHandle, Utils.CreateNativeString(var), value.NativeHandle));
    }
    public AST Lookup(string var) {
        IPK_BaseMethods.CheckResult(IPK_BaseMethods.IPK_Sub_Lookup(NativeHandle, Utils.CreateNativeString(var), out nint result));
        return new AST(result);
    }
}