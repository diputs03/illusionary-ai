namespace illusion.Gateway.Tests;

public class IllusionGatewayServiceTests
{
    [Fact]
    public void ProveDirect_ReturnsProofTraceResponse()
    {
        var service = new IllusionGatewayService();

        var response = service.ProveDirect(new ProofRequest("Safe", "module-a", "ModuleA"));

        Assert.True(response.IsSuccess);
        Assert.NotEmpty(response.TraceId);
        Assert.Contains(response.Steps, step => step.Contains("Safe(module-a:ModuleA)", StringComparison.Ordinal));
    }

    [Fact]
    public void GenerateDirect_ReturnsGeneratedCode()
    {
        var service = new IllusionGatewayService();

        var response = service.GenerateDirect(new ProofRequest("Safe", "module-a", "ModuleA"));

        Assert.Contains("GeneratedProofArtifact", response.Code);
    }
}
