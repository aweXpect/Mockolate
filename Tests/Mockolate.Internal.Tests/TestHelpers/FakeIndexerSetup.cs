using System.Diagnostics.CodeAnalysis;
using Mockolate.Interactions;
using Mockolate.Parameters;
using Mockolate.Setup;

namespace Mockolate.Internal.Tests.TestHelpers;

internal sealed class FakeIndexerSetup : IndexerSetup
{
	private readonly bool _match;
	internal FakeIndexerSetup(bool match) { _match = match; }
	protected override bool IsMatch(NamedParameterValue[] parameters) => _match;
	protected override T ExecuteGetterCallback<T>(IndexerGetterAccess indexerGetterAccess, T value, MockBehavior behavior) => value;
	protected override void ExecuteSetterCallback<T>(IndexerSetterAccess indexerSetterAccess, T value, MockBehavior behavior) { }
	protected override bool? GetSkipBaseClass() => null;

	protected override void GetInitialValue<T>(MockBehavior behavior, Func<T> defaultValueGenerator, NamedParameterValue[] parameters, [NotNullWhen(true)] out T value)
		=> value = defaultValueGenerator();
}
