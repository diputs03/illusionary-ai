using illusion.Common.Types;
using illusion.Types;
using Xunit;

namespace illusion.Common.Tests;

public class ExpressionTests
{
    [Fact]
    public void Constructor_WithValidParameters_CreatesInstance()
    {
        // Arrange
        var obj = new Types.Object("obj-001", "TestObject");
        const string predicate = "IsValid";

        // Act
        var expr = new Types.Expression(predicate, obj);

        // Assert
        Assert.NotNull(expr);
        Assert.Equal(predicate, expr.PredicateName);
        Assert.Equal(obj, expr.TargetObject);
    }

    [Fact]
    public void Constructor_WithEmptyPredicate_ThrowsArgumentException()
    {
        // Arrange
        var obj = new Types.Object("obj-001", "TestObject");

        // Act & Assert
        Assert.Throws<IAException<ArgumentException>>(() => new Types.Expression(string.Empty, obj));
    }
}