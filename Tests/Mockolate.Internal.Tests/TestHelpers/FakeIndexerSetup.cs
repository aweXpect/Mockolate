using System.Diagnostics.CodeAnalysis;
using Mockolate.Interactions;
using Mockolate.Parameters;
using Mockolate.Setup;

namespace Mockolate.Internal.Tests.TestHelpers;

internal sealed class FakeIndexerSetup : IndexerSetup
{
	internal FakeIndexerSetup(bool match) { ShouldMatch = match; }
	internal bool ShouldMatch { get; }

	protected override bool IsMatch(NamedParameterValue[] parameters) => ShouldMatch;
	protected override T ExecuteGetterCallback<T>(IndexerGetterAccess indexerGetterAccess, T value, MockBehavior behavior) => value;
	protected override void ExecuteSetterCallback<T>(IndexerSetterAccess indexerSetterAccess, T value, MockBehavior behavior) { }
	protected override bool? GetSkipBaseClass() => null;

	protected override void GetInitialValue<T>(MockBehavior behavior, Func<T> defaultValueGenerator, NamedParameterValue[] parameters, [NotNullWhen(true)] out T value)
		=> value = defaultValueGenerator();
}
