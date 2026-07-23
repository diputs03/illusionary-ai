using System.Runtime.InteropServices;
using System.Diagnostics;
using illusion.Common.Types;
using illusion.Common.Utils;
using System.Text.Json;
using illusion.CoreLogic.Kernel;

namespace illusion.CoreLogic.IPK_Adapter;

/// <summary>
/// LEAN kernel adapter
/// </summary>
public class IPK_Kernel : IKernel<AST>
{
    private nint _context;
    public bool Init()
    {
        _context = nint.Zero;
        IPK_BaseMethods.CheckResult(IPK_BaseMethods.IPK_CreateContext(out _context));
        return true;
    }
    public bool IsInit => _context != nint.Zero;
    public void Shutdown()
    {
        Dispose();
    }
    public void Dispose()
    {
        if (_context != nint.Zero)
        {
            IPK_BaseMethods.IPK_DestroyContext(_context);
            _context = nint.Zero;
        }
        return;
    }

    public void LoadContext()
    {
        throw new NotImplementedException();
    }

    public void ClearContext()
    {
        IPK_BaseMethods.IPK_DestroyContext(_context);
        _context = nint.Zero;
    }

    public bool Verify()
    {
        throw new NotImplementedException();
    }

    public long AddAxiom(string name, string statement)
    {
        throw new NotImplementedException();
    }

    public string GetAxiom(long axiomId)
    {
        throw new NotImplementedException();
    }

    public long AddRule(string name, string statement, nint[] premiseIds)
    {
        throw new NotImplementedException();
    }

    public string GetRule(string ruleName)
    {
        throw new NotImplementedException();
    }
}