using Mockolate.Parameters;

namespace Mockolate;

#pragma warning disable S3453 // This class can't be instantiated; make its constructor 'public'.
public partial class Match
{
	/// <summary>
	///     Signals the <c>Raise</c> surface to synthesize default values for every parameter of the event's delegate,
	///     instead of requiring them to be passed explicitly.
	/// </summary>
	/// <remarks>
	///     Useful when subscribers don't care about the event payload and you just need the signal to fire. The
	///     generated values follow the mock's <see cref="MockBehavior.DefaultValue" />.
	/// </remarks>
	/// <example>
	///     <code>
	///     sut.Mock.Raise.UsersChanged(Match.WithDefaultParameters());
	///     </code>
	/// </example>
	public static IDefaultEventParameters WithDefaultParameters()
		=> new DefaultEventParameters();

#if !DEBUG
	[System.Diagnostics.DebuggerNonUserCode]
#endif
	private sealed class DefaultEventParameters : IDefaultEventParameters
	{
		/// <inheritdoc cref="object.ToString()" />
		public override string ToString() => "Match.WithDefaultParameters()";
	}
}
#pragma warning restore S3453 // This class can't be instantiated; make its constructor 'public'.
