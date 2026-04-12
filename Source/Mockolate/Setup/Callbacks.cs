using System;
using System.Collections.Generic;

namespace Mockolate.Setup;

/// <summary>
///     A wrapper for the list of callbacks.
/// </summary>
#if !DEBUGe
[System.Diagnostics.DebuggerNonUserCode]
#endif
public class Callbacks<T> : List<Callback<T>> where T : Delegate
{
	/// <summary>
	///    The index of the currently executing callback.
	/// </summary>
	public int CurrentIndex;
	
	public Callback<T>? Active { get; set; }
}
