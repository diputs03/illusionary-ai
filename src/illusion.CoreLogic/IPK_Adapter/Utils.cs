namespace illusion.CoreLogic.IPK_Adapter;
using CPP_String = nint;
public static class Utils
{
    static public string GetNativeString(CPP_String s)
    {
        IPK_BaseMethods.CheckResult(IPK_BaseMethods.IPK_GetString(s, out string nativestr));
        return nativestr;
    }
    static public CPP_String CreateNativeString(string nativestr)
    {
        IPK_BaseMethods.CheckResult(IPK_BaseMethods.IPK_MakeString(nativestr, out CPP_String s));
        return s;
    }
}