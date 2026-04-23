using System;
using Mockolate.Parameters;

namespace Mockolate;

#pragma warning disable S3453 // This class can't be instantiated; make its constructor 'public'.
#pragma warning disable S3218 // Inner class members should not shadow outer class "static" or type members
public partial class It
{
	/// <summary>
	///     Matches any <see langword="bool" /> parameter whose value is <see langword="false" />.
	/// </summary>
	/// <remarks>
	///     Shorthand for <c>It.Is(false)</c> with a nicer failure-message rendering. Use <see cref="IsTrue" /> for the
	///     opposite.
	/// </remarks>
	public static IParameterWithCallback<bool> IsFalse()
		=> FalseParameterMatch.Shared;

#if !DEBUG
	[System.Diagnostics.DebuggerNonUserCode]
#endif
	private class FalseParameterMatch : TypedMatch<bool>
	{
		internal static readonly FalseParameterMatch Shared = new SharedFalseParameterMatch();

		/// <inheritdoc cref="TypedMatch{T}.Matches(T)" />
		protected override bool Matches(bool value) => !value;

		/// <inheritdoc cref="object.ToString()" />
		public override string ToString() => "It.IsFalse()";

#if !DEBUG
		[System.Diagnostics.DebuggerNonUserCode]
#endif
		private sealed class SharedFalseParameterMatch : FalseParameterMatch
		{
			protected override IParameterWithCallback<bool> AddCallback(Action<bool> callback)
				=> ((IParameterWithCallback<bool>)new FalseParameterMatch()).Do(callback);
		}
	}
}
#pragma warning restore S3218 // Inner class members should not shadow outer class "static" or type members
#pragma warning restore S3453 // This class can't be instantiated; make its constructor 'public'.
