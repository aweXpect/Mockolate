using System.Diagnostics;
using Mockolate.Setup;

namespace Mockolate.Interactions;

/// <summary>
///     An access of an indexer getter with a single typed parameter.
/// </summary>
[DebuggerDisplay("{ToString()}")]
#if !DEBUG
[DebuggerNonUserCode]
#endif
public class IndexerGetterAccess<T1>(string parameterName1, T1 parameter1) : IndexerAccess
{
	/// <summary>
	///     The single parameter name of the indexer.
	/// </summary>
	public string ParameterName1 { get; } = parameterName1;

	/// <summary>
	///     The single parameter value of the indexer.
	/// </summary>
	public T1 Parameter1 { get; } = parameter1;

	/// <inheritdoc cref="IndexerAccess.IsSetter" />
	public override bool IsSetter => false;

	/// <inheritdoc cref="IndexerAccess.TryFindStoredValue{T}(ValueStorage, out T)" />
	public override bool TryFindStoredValue<T>(ValueStorage storage, out T value)
	{
		ValueStorage? child = storage.GetChild(Parameter1);
		if (child is not null && child.Value is T typedValue)
		{
			value = typedValue;
			return true;
		}

		value = default!;
		return false;
	}

	/// <inheritdoc cref="IndexerAccess.StoreValue{T}(ValueStorage, T)" />
	public override void StoreValue<T>(ValueStorage storage, T value)
	{
		ValueStorage child = storage.GetOrAddChild(Parameter1!);
		child.Value = value;
	}

	/// <inheritdoc cref="object.ToString()" />
	public override string ToString()
		=> $"get indexer [{Parameter1}]";
}

/// <summary>
///     An access of an indexer getter with two typed parameters.
/// </summary>
[DebuggerDisplay("{ToString()}")]
#if !DEBUG
[DebuggerNonUserCode]
#endif
public class IndexerGetterAccess<T1, T2>(
	string parameterName1, T1 parameter1,
	string parameterName2, T2 parameter2) : IndexerAccess
{
	/// <summary>
	///     The first parameter name of the indexer.
	/// </summary>
	public string ParameterName1 { get; } = parameterName1;

	/// <summary>
	///     The first parameter value of the indexer.
	/// </summary>
	public T1 Parameter1 { get; } = parameter1;

	/// <summary>
	///     The second parameter name of the indexer.
	/// </summary>
	public string ParameterName2 { get; } = parameterName2;

	/// <summary>
	///     The second parameter value of the indexer.
	/// </summary>
	public T2 Parameter2 { get; } = parameter2;

	/// <inheritdoc cref="IndexerAccess.IsSetter" />
	public override bool IsSetter => false;

	/// <inheritdoc cref="IndexerAccess.TryFindStoredValue{T}(ValueStorage, out T)" />
	public override bool TryFindStoredValue<T>(ValueStorage storage, out T value)
	{
		ValueStorage? level1 = storage.GetChild(Parameter1);
		if (level1 is not null)
		{
			ValueStorage? level2 = level1.GetChild(Parameter2);
			if (level2 is not null && level2.Value is T typedValue)
			{
				value = typedValue;
				return true;
			}
		}

		value = default!;
		return false;
	}

	/// <inheritdoc cref="IndexerAccess.StoreValue{T}(ValueStorage, T)" />
	public override void StoreValue<T>(ValueStorage storage, T value)
	{
		ValueStorage level1 = storage.GetOrAddChild(Parameter1!);
		ValueStorage level2 = level1.GetOrAddChild(Parameter2!);
		level2.Value = value;
	}

	/// <inheritdoc cref="object.ToString()" />
	public override string ToString()
		=> $"get indexer [{Parameter1}, {Parameter2}]";
}

/// <summary>
///     An access of an indexer getter with three typed parameters.
/// </summary>
[DebuggerDisplay("{ToString()}")]
#if !DEBUG
[DebuggerNonUserCode]
#endif
public class IndexerGetterAccess<T1, T2, T3>(
	string parameterName1, T1 parameter1,
	string parameterName2, T2 parameter2,
	string parameterName3, T3 parameter3) : IndexerAccess
{
	/// <summary>
	///     The first parameter name of the indexer.
	/// </summary>
	public string ParameterName1 { get; } = parameterName1;

	/// <summary>
	///     The first parameter value of the indexer.
	/// </summary>
	public T1 Parameter1 { get; } = parameter1;

	/// <summary>
	///     The second parameter name of the indexer.
	/// </summary>
	public string ParameterName2 { get; } = parameterName2;

	/// <summary>
	///     The second parameter value of the indexer.
	/// </summary>
	public T2 Parameter2 { get; } = parameter2;

	/// <summary>
	///     The third parameter name of the indexer.
	/// </summary>
	public string ParameterName3 { get; } = parameterName3;

	/// <summary>
	///     The third parameter value of the indexer.
	/// </summary>
	public T3 Parameter3 { get; } = parameter3;

	/// <inheritdoc cref="IndexerAccess.IsSetter" />
	public override bool IsSetter => false;

	/// <inheritdoc cref="IndexerAccess.TryFindStoredValue{T}(ValueStorage, out T)" />
	public override bool TryFindStoredValue<T>(ValueStorage storage, out T value)
	{
		ValueStorage? level1 = storage.GetChild(Parameter1);
		if (level1 is not null)
		{
			ValueStorage? level2 = level1.GetChild(Parameter2);
			if (level2 is not null)
			{
				ValueStorage? level3 = level2.GetChild(Parameter3);
				if (level3 is not null && level3.Value is T typedValue)
				{
					value = typedValue;
					return true;
				}
			}
		}

		value = default!;
		return false;
	}

	/// <inheritdoc cref="IndexerAccess.StoreValue{T}(ValueStorage, T)" />
	public override void StoreValue<T>(ValueStorage storage, T value)
	{
		ValueStorage level1 = storage.GetOrAddChild(Parameter1!);
		ValueStorage level2 = level1.GetOrAddChild(Parameter2!);
		ValueStorage level3 = level2.GetOrAddChild(Parameter3!);
		level3.Value = value;
	}

	/// <inheritdoc cref="object.ToString()" />
	public override string ToString()
		=> $"get indexer [{Parameter1}, {Parameter2}, {Parameter3}]";
}

/// <summary>
///     An access of an indexer getter with four typed parameters.
/// </summary>
[DebuggerDisplay("{ToString()}")]
#if !DEBUG
[DebuggerNonUserCode]
#endif
public class IndexerGetterAccess<T1, T2, T3, T4>(
	string parameterName1, T1 parameter1,
	string parameterName2, T2 parameter2,
	string parameterName3, T3 parameter3,
	string parameterName4, T4 parameter4) : IndexerAccess
{
	/// <summary>
	///     The first parameter name of the indexer.
	/// </summary>
	public string ParameterName1 { get; } = parameterName1;

	/// <summary>
	///     The first parameter value of the indexer.
	/// </summary>
	public T1 Parameter1 { get; } = parameter1;

	/// <summary>
	///     The second parameter name of the indexer.
	/// </summary>
	public string ParameterName2 { get; } = parameterName2;

	/// <summary>
	///     The second parameter value of the indexer.
	/// </summary>
	public T2 Parameter2 { get; } = parameter2;

	/// <summary>
	///     The third parameter name of the indexer.
	/// </summary>
	public string ParameterName3 { get; } = parameterName3;

	/// <summary>
	///     The third parameter value of the indexer.
	/// </summary>
	public T3 Parameter3 { get; } = parameter3;

	/// <summary>
	///     The fourth parameter name of the indexer.
	/// </summary>
	public string ParameterName4 { get; } = parameterName4;

	/// <summary>
	///     The fourth parameter value of the indexer.
	/// </summary>
	public T4 Parameter4 { get; } = parameter4;

	/// <inheritdoc cref="IndexerAccess.IsSetter" />
	public override bool IsSetter => false;

	/// <inheritdoc cref="IndexerAccess.TryFindStoredValue{T}(ValueStorage, out T)" />
	public override bool TryFindStoredValue<T>(ValueStorage storage, out T value)
	{
		ValueStorage? level1 = storage.GetChild(Parameter1);
		if (level1 is not null)
		{
			ValueStorage? level2 = level1.GetChild(Parameter2);
			if (level2 is not null)
			{
				ValueStorage? level3 = level2.GetChild(Parameter3);
				if (level3 is not null)
				{
					ValueStorage? level4 = level3.GetChild(Parameter4);
					if (level4 is not null && level4.Value is T typedValue)
					{
						value = typedValue;
						return true;
					}
				}
			}
		}

		value = default!;
		return false;
	}

	/// <inheritdoc cref="IndexerAccess.StoreValue{T}(ValueStorage, T)" />
	public override void StoreValue<T>(ValueStorage storage, T value)
	{
		ValueStorage level1 = storage.GetOrAddChild(Parameter1!);
		ValueStorage level2 = level1.GetOrAddChild(Parameter2!);
		ValueStorage level3 = level2.GetOrAddChild(Parameter3!);
		ValueStorage level4 = level3.GetOrAddChild(Parameter4!);
		level4.Value = value;
	}

	/// <inheritdoc cref="object.ToString()" />
	public override string ToString()
		=> $"get indexer [{Parameter1}, {Parameter2}, {Parameter3}, {Parameter4}]";
}
