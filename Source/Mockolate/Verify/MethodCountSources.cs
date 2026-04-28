using Mockolate.Interactions;
using Mockolate.Parameters;

namespace Mockolate.Verify;

/// <summary>
///     Allocation-free count source backed by a parameterless method buffer.
/// </summary>
internal sealed class Method0CountSource : IFastMethodCountSource
{
	private readonly FastMethod0Buffer _buffer;

	public Method0CountSource(FastMethod0Buffer buffer)
	{
		_buffer = buffer;
	}

	public int Count() => _buffer.ConsumeMatching();
	public int CountAll() => _buffer.ConsumeMatching();
}

/// <summary>
///     Allocation-free count source backed by a 1-parameter method buffer and a typed matcher.
/// </summary>
internal sealed class Method1CountSource<T1> : IFastMethodCountSource
{
	private readonly FastMethod1Buffer<T1> _buffer;
	private readonly IParameterMatch<T1> _match1;

	public Method1CountSource(FastMethod1Buffer<T1> buffer, IParameterMatch<T1> match1)
	{
		_buffer = buffer;
		_match1 = match1;
	}

	public int Count() => _buffer.ConsumeMatching(_match1);
	public int CountAll() => _buffer.ConsumeAll();
}

/// <summary>
///     Allocation-free count source backed by a 2-parameter method buffer.
/// </summary>
internal sealed class Method2CountSource<T1, T2> : IFastMethodCountSource
{
	private readonly FastMethod2Buffer<T1, T2> _buffer;
	private readonly IParameterMatch<T1> _match1;
	private readonly IParameterMatch<T2> _match2;

	public Method2CountSource(FastMethod2Buffer<T1, T2> buffer,
		IParameterMatch<T1> match1, IParameterMatch<T2> match2)
	{
		_buffer = buffer;
		_match1 = match1;
		_match2 = match2;
	}

	public int Count() => _buffer.ConsumeMatching(_match1, _match2);
	public int CountAll() => _buffer.ConsumeAll();
}

/// <summary>
///     Allocation-free count source backed by a 3-parameter method buffer.
/// </summary>
internal sealed class Method3CountSource<T1, T2, T3> : IFastMethodCountSource
{
	private readonly FastMethod3Buffer<T1, T2, T3> _buffer;
	private readonly IParameterMatch<T1> _match1;
	private readonly IParameterMatch<T2> _match2;
	private readonly IParameterMatch<T3> _match3;

	public Method3CountSource(FastMethod3Buffer<T1, T2, T3> buffer,
		IParameterMatch<T1> match1, IParameterMatch<T2> match2, IParameterMatch<T3> match3)
	{
		_buffer = buffer;
		_match1 = match1;
		_match2 = match2;
		_match3 = match3;
	}

	public int Count() => _buffer.ConsumeMatching(_match1, _match2, _match3);
	public int CountAll() => _buffer.ConsumeAll();
}

/// <summary>
///     Allocation-free count source backed by a 4-parameter method buffer.
/// </summary>
internal sealed class Method4CountSource<T1, T2, T3, T4> : IFastMethodCountSource
{
	private readonly FastMethod4Buffer<T1, T2, T3, T4> _buffer;
	private readonly IParameterMatch<T1> _match1;
	private readonly IParameterMatch<T2> _match2;
	private readonly IParameterMatch<T3> _match3;
	private readonly IParameterMatch<T4> _match4;

	public Method4CountSource(FastMethod4Buffer<T1, T2, T3, T4> buffer,
		IParameterMatch<T1> match1, IParameterMatch<T2> match2,
		IParameterMatch<T3> match3, IParameterMatch<T4> match4)
	{
		_buffer = buffer;
		_match1 = match1;
		_match2 = match2;
		_match3 = match3;
		_match4 = match4;
	}

	public int Count() => _buffer.ConsumeMatching(_match1, _match2, _match3, _match4);
	public int CountAll() => _buffer.ConsumeAll();
}

/// <summary>
///     Allocation-free count source backed by a property getter buffer.
/// </summary>
internal sealed class PropertyGetterCountSource : IFastCountSource
{
	private readonly FastPropertyGetterBuffer _buffer;

	public PropertyGetterCountSource(FastPropertyGetterBuffer buffer)
	{
		_buffer = buffer;
	}

	public int Count() => _buffer.ConsumeMatching();
}

/// <summary>
///     Allocation-free count source backed by a property setter buffer and a typed value matcher.
/// </summary>
internal sealed class PropertySetterCountSource<T> : IFastCountSource
{
	private readonly FastPropertySetterBuffer<T> _buffer;
	private readonly IParameterMatch<T> _match;

	public PropertySetterCountSource(FastPropertySetterBuffer<T> buffer, IParameterMatch<T> match)
	{
		_buffer = buffer;
		_match = match;
	}

	public int Count() => _buffer.ConsumeMatching(_match);
}

/// <summary>
///     Allocation-free count source backed by a 1-key indexer getter buffer.
/// </summary>
internal sealed class IndexerGetter1CountSource<T1> : IFastCountSource
{
	private readonly FastIndexerGetterBuffer<T1> _buffer;
	private readonly IParameterMatch<T1> _match1;

	public IndexerGetter1CountSource(FastIndexerGetterBuffer<T1> buffer, IParameterMatch<T1> match1)
	{
		_buffer = buffer;
		_match1 = match1;
	}

	public int Count() => _buffer.ConsumeMatching(_match1);
}

/// <summary>
///     Allocation-free count source backed by a 2-key indexer getter buffer.
/// </summary>
internal sealed class IndexerGetter2CountSource<T1, T2> : IFastCountSource
{
	private readonly FastIndexerGetterBuffer<T1, T2> _buffer;
	private readonly IParameterMatch<T1> _match1;
	private readonly IParameterMatch<T2> _match2;

	public IndexerGetter2CountSource(FastIndexerGetterBuffer<T1, T2> buffer,
		IParameterMatch<T1> match1, IParameterMatch<T2> match2)
	{
		_buffer = buffer;
		_match1 = match1;
		_match2 = match2;
	}

	public int Count() => _buffer.ConsumeMatching(_match1, _match2);
}

/// <summary>
///     Allocation-free count source backed by a 3-key indexer getter buffer.
/// </summary>
internal sealed class IndexerGetter3CountSource<T1, T2, T3> : IFastCountSource
{
	private readonly FastIndexerGetterBuffer<T1, T2, T3> _buffer;
	private readonly IParameterMatch<T1> _match1;
	private readonly IParameterMatch<T2> _match2;
	private readonly IParameterMatch<T3> _match3;

	public IndexerGetter3CountSource(FastIndexerGetterBuffer<T1, T2, T3> buffer,
		IParameterMatch<T1> match1, IParameterMatch<T2> match2, IParameterMatch<T3> match3)
	{
		_buffer = buffer;
		_match1 = match1;
		_match2 = match2;
		_match3 = match3;
	}

	public int Count() => _buffer.ConsumeMatching(_match1, _match2, _match3);
}

/// <summary>
///     Allocation-free count source backed by a 4-key indexer getter buffer.
/// </summary>
internal sealed class IndexerGetter4CountSource<T1, T2, T3, T4> : IFastCountSource
{
	private readonly FastIndexerGetterBuffer<T1, T2, T3, T4> _buffer;
	private readonly IParameterMatch<T1> _match1;
	private readonly IParameterMatch<T2> _match2;
	private readonly IParameterMatch<T3> _match3;
	private readonly IParameterMatch<T4> _match4;

	public IndexerGetter4CountSource(FastIndexerGetterBuffer<T1, T2, T3, T4> buffer,
		IParameterMatch<T1> match1, IParameterMatch<T2> match2,
		IParameterMatch<T3> match3, IParameterMatch<T4> match4)
	{
		_buffer = buffer;
		_match1 = match1;
		_match2 = match2;
		_match3 = match3;
		_match4 = match4;
	}

	public int Count() => _buffer.ConsumeMatching(_match1, _match2, _match3, _match4);
}

/// <summary>
///     Allocation-free count source backed by a 1-key indexer setter buffer.
/// </summary>
internal sealed class IndexerSetter1CountSource<T1, TValue> : IFastCountSource
{
	private readonly FastIndexerSetterBuffer<T1, TValue> _buffer;
	private readonly IParameterMatch<T1> _match1;
	private readonly IParameterMatch<TValue> _matchValue;

	public IndexerSetter1CountSource(FastIndexerSetterBuffer<T1, TValue> buffer,
		IParameterMatch<T1> match1, IParameterMatch<TValue> matchValue)
	{
		_buffer = buffer;
		_match1 = match1;
		_matchValue = matchValue;
	}

	public int Count() => _buffer.ConsumeMatching(_match1, _matchValue);
}

/// <summary>
///     Allocation-free count source backed by a 2-key indexer setter buffer.
/// </summary>
internal sealed class IndexerSetter2CountSource<T1, T2, TValue> : IFastCountSource
{
	private readonly FastIndexerSetterBuffer<T1, T2, TValue> _buffer;
	private readonly IParameterMatch<T1> _match1;
	private readonly IParameterMatch<T2> _match2;
	private readonly IParameterMatch<TValue> _matchValue;

	public IndexerSetter2CountSource(FastIndexerSetterBuffer<T1, T2, TValue> buffer,
		IParameterMatch<T1> match1, IParameterMatch<T2> match2, IParameterMatch<TValue> matchValue)
	{
		_buffer = buffer;
		_match1 = match1;
		_match2 = match2;
		_matchValue = matchValue;
	}

	public int Count() => _buffer.ConsumeMatching(_match1, _match2, _matchValue);
}

/// <summary>
///     Allocation-free count source backed by a 3-key indexer setter buffer.
/// </summary>
internal sealed class IndexerSetter3CountSource<T1, T2, T3, TValue> : IFastCountSource
{
	private readonly FastIndexerSetterBuffer<T1, T2, T3, TValue> _buffer;
	private readonly IParameterMatch<T1> _match1;
	private readonly IParameterMatch<T2> _match2;
	private readonly IParameterMatch<T3> _match3;
	private readonly IParameterMatch<TValue> _matchValue;

	public IndexerSetter3CountSource(FastIndexerSetterBuffer<T1, T2, T3, TValue> buffer,
		IParameterMatch<T1> match1, IParameterMatch<T2> match2,
		IParameterMatch<T3> match3, IParameterMatch<TValue> matchValue)
	{
		_buffer = buffer;
		_match1 = match1;
		_match2 = match2;
		_match3 = match3;
		_matchValue = matchValue;
	}

	public int Count() => _buffer.ConsumeMatching(_match1, _match2, _match3, _matchValue);
}

/// <summary>
///     Allocation-free count source backed by a 4-key indexer setter buffer.
/// </summary>
#pragma warning disable S2436 // Types and methods should not have too many generic parameters
internal sealed class IndexerSetter4CountSource<T1, T2, T3, T4, TValue> : IFastCountSource
#pragma warning restore S2436 // Types and methods should not have too many generic parameters
{
	private readonly FastIndexerSetterBuffer<T1, T2, T3, T4, TValue> _buffer;
	private readonly IParameterMatch<T1> _match1;
	private readonly IParameterMatch<T2> _match2;
	private readonly IParameterMatch<T3> _match3;
	private readonly IParameterMatch<T4> _match4;
	private readonly IParameterMatch<TValue> _matchValue;

	public IndexerSetter4CountSource(FastIndexerSetterBuffer<T1, T2, T3, T4, TValue> buffer,
		IParameterMatch<T1> match1, IParameterMatch<T2> match2,
		IParameterMatch<T3> match3, IParameterMatch<T4> match4, IParameterMatch<TValue> matchValue)
	{
		_buffer = buffer;
		_match1 = match1;
		_match2 = match2;
		_match3 = match3;
		_match4 = match4;
		_matchValue = matchValue;
	}

	public int Count() => _buffer.ConsumeMatching(_match1, _match2, _match3, _match4, _matchValue);
}

/// <summary>
///     Allocation-free count source backed by an event subscribe/unsubscribe buffer.
/// </summary>
internal sealed class EventCountSource : IFastCountSource
{
	private readonly FastEventBuffer _buffer;

	public EventCountSource(FastEventBuffer buffer)
	{
		_buffer = buffer;
	}

	public int Count() => _buffer.ConsumeMatching();
}
