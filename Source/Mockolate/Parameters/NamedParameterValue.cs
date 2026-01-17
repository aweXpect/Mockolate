namespace Mockolate.Parameters;

/// <summary>
///     A named parameter value.
/// </summary>
/// <param name="Name">The name of the parameter.</param>
/// <param name="Value">The parameter value.</param>
public readonly record struct NamedParameterValue(
	string? Name,
	object? Value);
