using illusion.Common.Types;
using illusion.Types;
using Xunit;

namespace illusion.Common.Tests;

public class ConstraintTests
{
    [Fact]
    public void Constructor_WithValidParameters_CreatesInstance()
    {
        // Arrange
        var obj = new Types.Object("obj-001", "TestObject");
        var expr = new Types.Expression("IsValid", obj);

        // Act
        var constraint = new Types.Constraint(Types.Constraint.ConstraintType.Precondition, expr, "pretest conditions");

        // Assert
        Assert.NotNull(constraint);
        Assert.Equal(Types.Constraint.ConstraintType.Precondition, constraint.Type);
        Assert.Equal(expr, constraint.ConstraintExpression);
        Assert.Equal("pretest conditions", constraint.Description);
    }

    [Fact]
    public void Constructor_WithNullExpression_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<IAException<ArgumentNullException>>(() =>
            new Types.Constraint(Types.Constraint.ConstraintType.Precondition, null!));
    }

    [Fact]
    public void Verify_WithNullState_ThrowsArgumentNullException()
    {
        // Arrange
        var obj = new Types.Object("obj-001", "TestObject");
        var expr = new Types.Expression("IsValid", obj);
        var constraint = new Types.Constraint(Types.Constraint.ConstraintType.Precondition, expr);

        // Act & Assert
        Assert.Throws<IAException<ArgumentNullException>>(() => constraint.Verify(null!));
    }
}