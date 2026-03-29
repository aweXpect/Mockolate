using Mockolate.Interactions;
using Mockolate.Setup;

namespace Mockolate.Internal.Tests.TestHelpers;

internal sealed class FakeMethodMatch : IMethodMatch
{
	private readonly bool _matches;
	internal FakeMethodMatch(bool matches) { _matches = matches; }
	public bool Matches(MethodInvocation methodInvocation) => _matches;
}

internal sealed class FakeMethodSetup : MethodSetup
{
	internal FakeMethodSetup() : base(new FakeMethodMatch(true)) { }
	protected override bool? GetSkipBaseClass() => null;
	protected override T SetOutParameter<T>(string parameterName, Func<T> defaultValueGenerator) => default!;
	protected override T SetRefParameter<T>(string parameterName, T value, MockBehavior behavior) => value;
	protected override void ExecuteCallback(MethodInvocation invocation, MockBehavior behavior) { }
	protected override TResult GetReturnValue<TResult>(MethodInvocation invocation, MockBehavior behavior, Func<TResult> defaultValueGenerator) => defaultValueGenerator();
	protected override void TriggerParameterCallbacks(object?[] parameters) { }
}
