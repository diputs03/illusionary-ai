using illusion.Common.Types;

namespace illusion.Generator;

/// <summary>
/// constructive prover interface, based on the Curry-Howard correspondence
/// </summary>
public interface IConstructiveProver
{
    string GenerateCode(Common.Types.Expression proposition, string targetLanguage = "C#");
}