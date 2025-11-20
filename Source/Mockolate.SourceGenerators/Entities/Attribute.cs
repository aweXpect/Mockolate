using Microsoft.CodeAnalysis;
using Mockolate.SourceGenerators.Internals;

namespace Mockolate.SourceGenerators.Entities;

internal record Attribute
{
	public Attribute(AttributeData attributeData)
	{
		Name = ToName(attributeData.AttributeClass?.ToDisplayString() ?? "");
		if (attributeData.ConstructorArguments.Length > 0)
		{
			Parameters = new EquatableArray<AttributeParameter>(attributeData.ConstructorArguments
				.Select(arg => new AttributeParameter(arg)).ToArray());
		}
	}

	public string Name { get; }

	public EquatableArray<AttributeParameter>? Parameters { get; }

	private static string ToName(string className)
		=> className.EndsWith("Attribute")
			? className.Substring(0, className.Length - "Attribute".Length)
			: className;
}
