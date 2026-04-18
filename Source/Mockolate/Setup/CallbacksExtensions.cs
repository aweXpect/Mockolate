using System;

namespace Mockolate.Setup;

/// <summary>
///     Extension methods for <see cref="Callbacks{T}" />.
/// </summary>
#if !DEBUG
[System.Diagnostics.DebuggerNonUserCode]
#endif
public static class CallbacksExtensions
{
	/// <summary>
	///     Appends the <paramref name="callback" /> to the <paramref name="callbacks" /> list
	///     and marks it as the currently active callback. Creates the list if it is <see langword="null" />.
	/// </summary>
	public static Callbacks<T> Register<T>(this Callbacks<T>? callbacks, Callback<T> callback)
		where T : Delegate
	{
		callbacks ??= [];
		callbacks.Active = callback;
		callbacks.Add(callback);
		return callbacks;
	}
}
