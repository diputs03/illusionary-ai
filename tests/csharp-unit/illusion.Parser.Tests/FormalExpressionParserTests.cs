namespace illusion.Parser.Tests;

public class FormalExpressionParserTests
{
    [Fact]
    public void ParseExpression_ParsesCanonicalExpression()
    {
        var parser = new FormalExpressionParser();

        var expression = parser.ParseExpression("Verified(module-a:ModuleA)");

        Assert.Equal("Verified", expression.PredicateName);
        Assert.Equal("module-a", expression.TargetObject.Id);
        Assert.Equal("ModuleA", expression.TargetObject.Name);
    }

    [Fact]
    public void ParseExpression_RejectsAmbiguousInput()
    {
        var parser = new FormalExpressionParser();

        Assert.False(parser.TryParseExpression("Verified module-a", out _, out var error));
        Assert.Contains("expected canonical expression", error);
    }
}
