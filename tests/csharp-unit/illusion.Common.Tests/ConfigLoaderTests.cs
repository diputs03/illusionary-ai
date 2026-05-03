using illusion.Common.Utils;
using Xunit;

namespace illusion.Common.Tests;

public class ConfigLoaderTests
{
    [Fact]
    public void GetSystemName_ReturnsCorrectValue()
    {
        // Act
        var name = ConfigLoader.GetSystemName();

        // Assert
        Assert.Equal("Illusionary-AI", name);
    }

    [Fact]
    public void GetLogLevel_ReturnsDebug()
    {
        // Act
        var logLevel = ConfigLoader.GetLogLevel();

        // Assert
        Assert.Equal("DEBUG", logLevel);
    }
}