using illusion.Common.Types;
using illusion.CoreLogic.Kernel;
using System.Linq.Expressions;
using Xunit;

namespace illusion.CoreLogic.Tests;

public class KernelAdapterTests : IDisposable
{
    private readonly KernelAdapter _kernelAdapter;

    public KernelAdapterTests()
    {
        Console.Write(System.Reflection.Assembly.GetExecutingAssembly().Location);
        _kernelAdapter = new KernelAdapter();
        _kernelAdapter.Initialize();
    }

    [Fact]
    public void Initialize_KernelStartsSuccessfully()
    {
        // Assert
        Assert.True(_kernelAdapter.IsInitialized());
    }

    [Fact]
    public void ASTCreationCloneEquality_Test()
    {
        // Act
        IntPtr ast1 = _kernelAdapter.CreateSymbolAST("symbol");
        IntPtr ast2 = _kernelAdapter.CloneAST(ast1);
        // Assert
        Assert.NotEqual(IntPtr.Zero, ast1);
        Assert.NotEqual(IntPtr.Zero, ast2);
        Assert.True(_kernelAdapter.EqualAST(ast1, ast2));
        _kernelAdapter.FreeAST(ast1);
        _kernelAdapter.FreeAST(ast2);
    }
    [Fact]
    public void ASTCreationPrint_Test()
    {
        // Act
        IntPtr ast1 = _kernelAdapter.CreateSymbolAST("symbol1");
        IntPtr ast2 = _kernelAdapter.CreateSymbolAST("symbol2");
        IntPtr ast = _kernelAdapter.CreateListAST(new IntPtr[] { ast1, ast2 });
        // Assert
        Assert.NotEqual(IntPtr.Zero, ast1);
        Assert.NotEqual(IntPtr.Zero, ast2);
        Assert.NotEqual(IntPtr.Zero, ast);

        _kernelAdapter.PrintAST(ast);
        _kernelAdapter.FreeAST(ast);
    }

    //[Fact]
    //public void IsProvable_WithValidProposition_ReturnsTrue()
    //{
    //    // Arrange
    //    var obj = new Common.Types.Object("num", "1+1");
    //    var proposition = new Common.Types.Expression("Equals2", obj);

    //    // Act
    //    var result = _kernelAdapter.IsProvable(proposition);

    //    // Assert
    //    Assert.True(result);
    //}

    //[Fact]
    //public void Prove_WithValidProposition_ReturnsSuccessTrace()
    //{
    //    // Arrange
    //    var obj = new Common.Types.Object("num", "1+1");
    //    var proposition = new Common.Types.Expression("Equals2", obj);

    //    // Act
    //    var trace = _kernelAdapter.Prove(proposition);

    //    // Assert
    //    Assert.NotNull(trace);
    //    Assert.True(trace.IsSuccess);
    //    Assert.NotEmpty(trace.Steps);
    //    Assert.Equal(proposition, trace.TargetProposition);
    //}

    public void Dispose()
    {
        _kernelAdapter.Dispose();
    }
}