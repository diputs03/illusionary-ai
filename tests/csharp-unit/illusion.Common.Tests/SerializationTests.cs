using illusion.Common.Types;
using illusion.Common.Utils;
using Xunit;

namespace illusion.Common.Tests;

public class SerializationTests
{
    [Fact]
    public void SerializationUtils_RoundTripsExpression()
    {
        var expression = new Types.Expression("Verified", new Types.Object("module-a", "ModuleA"));

        var bytes = SerializationUtils.Serialize(expression);
        var restored = SerializationUtils.Deserialize<Types.Expression>(bytes);

        Assert.Equal(expression, restored);
    }
}
