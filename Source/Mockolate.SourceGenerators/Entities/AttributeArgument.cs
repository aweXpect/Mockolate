using Microsoft.CodeAnalysis;

namespace Mockolate.SourceGenerators.Entities;

internal record AttributeArgument
{
	public AttributeArgument(TypedConstant value)
	{
		if (value.Value is null)
		{
			Value = "null";
		}
		else if (value.Type?.SpecialType == SpecialType.System_String)
		{
			Value = $"\"{value.Value.ToString()?.Replace("\"", "\\\"")}\"";
		}
		else
		{
			Value = value.Value.ToString();
		}
	}
	public string? Value { get; }
}
