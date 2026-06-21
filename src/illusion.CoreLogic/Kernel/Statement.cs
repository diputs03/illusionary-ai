namespace illusion.CoreLogic.Kernel;

/// <summary>
/// Managed handle for a kernel statement. The handle keeps the canonical text
/// separate from any native pointer so callers can parse, compare, serialize,
/// and safely destroy statements without depending on a specific prover backend.
/// </summary>
public sealed class Statement : IDisposable
{
    private Action<Statement>? _destroy;

    internal Statement(string text, nint nativeHandle = 0, Action<Statement>? destroy = null)
    {
        Text = string.IsNullOrWhiteSpace(text) ? throw new ArgumentException("statement text cannot be empty", nameof(text)) : text.Trim();
        NativeHandle = nativeHandle;
        _destroy = destroy;
    }

    public string Text { get; }
    public nint NativeHandle { get; internal set; }
    public bool IsDestroyed { get; private set; }

    public override string ToString() => Text;

    public void Dispose()
    {
        if (IsDestroyed)
            return;

        _destroy?.Invoke(this);
        _destroy = null;
        NativeHandle = 0;
        IsDestroyed = true;
    }
}
