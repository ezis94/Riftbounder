using Riftbounder.Core.Effects;
using Riftbounder.Engine.Cards;
using Riftbounder.Engine.Cards.Resolution;
using Riftbounder.Engine.Targets;

namespace Riftbounder.Engine.Effects;

public sealed class PartialSpellEffectExecutor :
    ISpellEffectExecutor
{
    private readonly TargetResolver _targetResolver;
    private readonly IReadOnlyDictionary<string, ISpellInstructionHandler>
        _handlers;

    public PartialSpellEffectExecutor(
        TargetResolver targetResolver,
        IEnumerable<ISpellInstructionHandler> handlers)
    {
        ArgumentNullException.ThrowIfNull(targetResolver);
        ArgumentNullException.ThrowIfNull(handlers);

        _targetResolver = targetResolver;
        _handlers = handlers.ToDictionary(
            handler => handler.InstructionId,
            StringComparer.Ordinal);
    }

    public SpellEffectExecutionReport? LastReport { get; private set; }

    public SpellExecutionResult Execute(
        PlayCardChainItem spell)
    {
        ArgumentNullException.ThrowIfNull(spell);

        List<InstructionResolutionRecord> records = [];

        foreach (SpellInstructionDefinition instruction
                 in spell.Card.Definition.Instructions)
        {
            if (!_handlers.TryGetValue(
                    instruction.Id,
                    out ISpellInstructionHandler? handler))
            {
                string reason =
                    $"No handler is registered for instruction '{instruction.Id}'.";

                LastReport = new SpellEffectExecutionReport(
                    records,
                    false,
                    reason);

                return SpellExecutionResult.Failure(reason);
            }

            List<ResolvedInstructionTarget> resolvedTargets = [];

            foreach (int targetIndex in instruction.TargetIndexes)
            {
                if (targetIndex >= spell.Targets.Count)
                {
                    string reason =
                        $"Instruction '{instruction.Id}' references missing target index {targetIndex}.";

                    LastReport = new SpellEffectExecutionReport(
                        records,
                        false,
                        reason);

                    return SpellExecutionResult.Failure(reason);
                }

                var snapshot = spell.Targets[targetIndex];
                var resolution = _targetResolver.Resolve(snapshot);

                resolvedTargets.Add(
                    new ResolvedInstructionTarget(
                        targetIndex,
                        snapshot,
                        resolution.Status,
                        resolution.Target));
            }

            InstructionExecutionContext context = new(
                spell,
                instruction,
                resolvedTargets);

            InstructionExecutionResult result =
                handler.Execute(context);

            records.Add(
                new InstructionResolutionRecord(
                    instruction,
                    resolvedTargets,
                    result));

            if (!result.Succeeded)
            {
                LastReport = new SpellEffectExecutionReport(
                    records,
                    false,
                    result.FailureReason);

                return SpellExecutionResult.Failure(
                    result.FailureReason
                    ?? "Instruction execution failed.");
            }
        }

        LastReport = new SpellEffectExecutionReport(
            records,
            true,
            null);

        return SpellExecutionResult.Success();
    }
}
