using System;

namespace Mockolate.Parameters;

/// <summary>
///     A named <see cref="Parameter" />.
/// </summary>
/// <param name="Name">The name of the <paramref name="Parameter" />.</param>
/// <param name="Parameter">The actual <see cref="IParameter" />.</param>
public record NamedParameter(string Name, IParameter Parameter)
{
	/// <inheritdoc cref="object.ToString()" />
	public override string? ToString() => Parameter.ToString();

	/// <summary>
	///     Checks if the name and value of the given <see cref="NamedParameterValue" /> matches this named parameter.
	/// </summary>
	public bool Matches(NamedParameterValue value)
		=> (string.IsNullOrEmpty(value.Name) || Name.Equals(value.Name, StringComparison.Ordinal)) &&
		   Parameter.Matches(value.Value);
}
