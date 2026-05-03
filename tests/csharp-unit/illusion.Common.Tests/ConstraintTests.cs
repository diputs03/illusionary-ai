using illusion.Common.Types;
using Xunit;

namespace illusion.Common.Tests;

public class ConstraintTests
{
    [Fact]
    public void Constructor_WithValidParameters_CreatesInstance()
    {
        // Arrange
        var obj = new Entity("obj-001", "TestObject");
        var expr = new Expression("IsValid", obj);

        // Act
        var constraint = new Constraint(Constraint.ConstraintType.Precondition, expr, "pretest conditions");

        // Assert
        Assert.NotNull(constraint);
        Assert.Equal(Constraint.ConstraintType.Precondition, constraint.Type);
        Assert.Equal(expr, constraint.ConstraintExpression);
        Assert.Equal("pretest conditions", constraint.Description);
    }

    [Fact]
    public void Constructor_WithNullExpression_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new Constraint(Constraint.ConstraintType.Precondition, null!));
    }

    [Fact]
    public void Verify_WithNullState_ThrowsArgumentNullException()
    {
        // Arrange
        var obj = new Entity("obj-001", "TestObject");
        var expr = new Expression("IsValid", obj);
        var constraint = new Constraint(Constraint.ConstraintType.Precondition, expr);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => constraint.Verify(null!));
    }
}