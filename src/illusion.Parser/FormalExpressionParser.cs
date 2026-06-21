using System.Text.RegularExpressions;
using illusion.Common.Types;
using illusion.CoreLogic.Prover;

namespace illusion.Parser;

/// <summary>
/// Deterministic parser for the canonical Illusionary-AI predicate form
/// <c>Predicate(object-id:object-name)</c>. The parser performs only structural
/// parsing; it never guesses missing fields.
/// </summary>
public sealed class FormalExpressionParser
{
    private static readonly Regex CanonicalExpression = new(
        @"^\s*(?<predicate>[A-Za-z_?][A-Za-z0-9_?\-.]*)\((?<id>[^:\s][^:]*)\:(?<name>[^\)\s][^\)]*)\)\s*$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public Expression ParseExpression(string input)
    {
        if (!TryParseExpression(input, out var expression, out var error))
            throw new FormatException(error);

        return expression;
    }

    public bool TryParseExpression(string input, out Expression expression, out string error)
    {
        expression = null!;
        error = string.Empty;

        if (string.IsNullOrWhiteSpace(input))
        {
            error = "expression cannot be empty";
            return false;
        }

        var match = CanonicalExpression.Match(input);
        if (!match.Success)
        {
            error = "expected canonical expression form: Predicate(object-id:object-name)";
            return false;
        }

        expression = new Expression(
            match.Groups["predicate"].Value.Trim(),
            new illusion.Common.Types.Object(match.Groups["id"].Value.Trim(), match.Groups["name"].Value.Trim()));
        return true;
    }

    public FormalRule ParseRule(string name, IEnumerable<string> premises, string conclusion, string description = "")
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("rule name cannot be empty", nameof(name));
        ArgumentNullException.ThrowIfNull(premises);

        return new FormalRule(
            name,
            premises.Select(ParseExpression),
            ParseExpression(conclusion),
            description);
    }
}
