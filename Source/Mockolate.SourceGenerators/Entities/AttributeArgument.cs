using Microsoft.CodeAnalysis;

namespace Mockolate.SourceGenerators.Entities;

internal record AttributeArgument
{
	public AttributeArgument(TypedConstant value)
	{
		string? valueString = value.Value?.ToString();
		if (valueString is not null &&
		    value.Type?.SpecialType == SpecialType.System_String)
		{
			Value = $"\"{valueString.Replace("\"", "\\\"")}\"";
		}
		else
		{
			Value = valueString ?? "null";
		}
	}

	public string? Value { get; }
}
