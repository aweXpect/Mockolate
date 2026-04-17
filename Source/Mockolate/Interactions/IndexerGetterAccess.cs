using System.Diagnostics;

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

	/// <inheritdoc cref="IndexerAccess.ParameterCount" />
	public override int ParameterCount => 1;

	/// <inheritdoc cref="IndexerAccess.GetParameterValueAt(int)" />
	public override object? GetParameterValueAt(int index)
		=> index switch
		{
			0 => Parameter1,
			_ => null,
		};

	/// <inheritdoc cref="object.ToString()" />
	public override string ToString()
		=> $"get indexer [{Parameter1?.ToString() ?? "null"}]";
}

/// <summary>
///     An access of an indexer getter with two typed parameters.
/// </summary>
[DebuggerDisplay("{ToString()}")]
#if !DEBUG
[DebuggerNonUserCode]
#endif
public class IndexerGetterAccess<T1, T2>(
	string parameterName1,
	T1 parameter1,
	string parameterName2,
	T2 parameter2) : IndexerAccess
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

	/// <inheritdoc cref="IndexerAccess.ParameterCount" />
	public override int ParameterCount => 2;

	/// <inheritdoc cref="IndexerAccess.GetParameterValueAt(int)" />
	public override object? GetParameterValueAt(int index)
		=> index switch
		{
			0 => Parameter1,
			1 => Parameter2,
			_ => null,
		};

	/// <inheritdoc cref="object.ToString()" />
	public override string ToString()
		=> $"get indexer [{Parameter1?.ToString() ?? "null"}, {Parameter2?.ToString() ?? "null"}]";
}

/// <summary>
///     An access of an indexer getter with three typed parameters.
/// </summary>
[DebuggerDisplay("{ToString()}")]
#if !DEBUG
[DebuggerNonUserCode]
#endif
public class IndexerGetterAccess<T1, T2, T3>(
	string parameterName1,
	T1 parameter1,
	string parameterName2,
	T2 parameter2,
	string parameterName3,
	T3 parameter3) : IndexerAccess
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

	/// <inheritdoc cref="IndexerAccess.ParameterCount" />
	public override int ParameterCount => 3;

	/// <inheritdoc cref="IndexerAccess.GetParameterValueAt(int)" />
	public override object? GetParameterValueAt(int index)
		=> index switch
		{
			0 => Parameter1,
			1 => Parameter2,
			2 => Parameter3,
			_ => null,
		};

	/// <inheritdoc cref="object.ToString()" />
	public override string ToString()
		=> $"get indexer [{Parameter1?.ToString() ?? "null"}, {Parameter2?.ToString() ?? "null"}, {Parameter3?.ToString() ?? "null"}]";
}

/// <summary>
///     An access of an indexer getter with four typed parameters.
/// </summary>
[DebuggerDisplay("{ToString()}")]
#if !DEBUG
[DebuggerNonUserCode]
#endif
public class IndexerGetterAccess<T1, T2, T3, T4>(
	string parameterName1,
	T1 parameter1,
	string parameterName2,
	T2 parameter2,
	string parameterName3,
	T3 parameter3,
	string parameterName4,
	T4 parameter4) : IndexerAccess
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

	/// <inheritdoc cref="IndexerAccess.ParameterCount" />
	public override int ParameterCount => 4;

	/// <inheritdoc cref="IndexerAccess.GetParameterValueAt(int)" />
	public override object? GetParameterValueAt(int index)
		=> index switch
		{
			0 => Parameter1,
			1 => Parameter2,
			2 => Parameter3,
			3 => Parameter4,
			_ => null,
		};

	/// <inheritdoc cref="object.ToString()" />
	public override string ToString()
		=> $"get indexer [{Parameter1?.ToString() ?? "null"}, {Parameter2?.ToString() ?? "null"}, {Parameter3?.ToString() ?? "null"}, {Parameter4?.ToString() ?? "null"}]";
}
