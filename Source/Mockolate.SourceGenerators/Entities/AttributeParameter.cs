using Microsoft.CodeAnalysis;

namespace Mockolate.SourceGenerators.Entities;

internal record AttributeParameter
{
	public AttributeParameter(TypedConstant value)
	{
		string? valueString = value.Value?.ToString();
		if (valueString is not null &&
		    value.Type?.SpecialType == SpecialType.System_String)
		{
			Value = $"\"{valueString.Replace("\"", "\\\"")}\"";
		}
		else if (valueString is not null &&
		    value.Type?.SpecialType == SpecialType.System_Boolean)
		{
			Value = valueString.ToLower();
		}
		else
		{
			Value = valueString ?? "null";
		}
	}

	public string? Value { get; }
}
