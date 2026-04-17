using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Mockolate.Setup;

/// <summary>
///     A wrapper for the list of callbacks.
/// </summary>
#if !DEBUG
[DebuggerNonUserCode]
#endif
public class Callbacks<T> : List<Callback<T>> where T : Delegate
{
	/// <summary>
	///     The index of the currently executing callback.
	/// </summary>
	public int CurrentIndex;

	/// <summary>
	///     The currently active callback.
	/// </summary>
	/// <remarks>
	///     This is used to fluently specify options like `Once` or `InParallel` on the currently active callback.
	/// </remarks>
	public Callback<T>? Active { get; set; }
}
