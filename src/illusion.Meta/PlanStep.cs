using illusion.Common.Types;

namespace illusion.Meta;

public sealed record PlanStep(int Order, illusion.Common.Types.Action Action, IReadOnlyList<Constraint> SatisfiedConstraints);
