using Mockolate.Interactions;
using Mockolate.Setup;

namespace Mockolate.Internal.Tests.TestHelpers;

internal sealed class FakeIndexerSetup : IndexerSetup
{
	internal FakeIndexerSetup(bool match) : base(new MockRegistry(MockBehavior.Default))
	{
		ShouldMatch = match;
	}

	internal bool ShouldMatch { get; }

	public static bool InvokeTryCast<T>(object? value, out T result, MockBehavior behavior)
		=> TryCast(value, out result, behavior);

	protected override bool MatchesAccess(IndexerAccess access) => ShouldMatch;

	public override bool? SkipBaseClass() => null;

	public override TResult GetResult<TResult>(IndexerAccess access, MockBehavior behavior, TResult baseValue)
		=> baseValue;

	public override TResult GetResult<TResult>(IndexerAccess access, MockBehavior behavior)
		=> default!;

	public override TResult GetResult<TResult>(IndexerAccess access, MockBehavior behavior,
		Func<TResult> defaultValueGenerator)
		=> defaultValueGenerator();

	public override void SetResult<TResult>(IndexerAccess access, MockBehavior behavior, TResult value)
	{
	}
}
