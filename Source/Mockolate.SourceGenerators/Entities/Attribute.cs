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
			Arguments = new EquatableArray<AttributeArgument>(attributeData.ConstructorArguments
				.Select(arg => new AttributeArgument(arg)).ToArray());
		}
	}

	public string Name { get; }

	public EquatableArray<AttributeArgument>? Arguments { get; }

	private static string ToName(string className)
	{
		if (className.EndsWith("Attribute"))
		{
			return className.Substring(0, className.Length - "Attribute".Length);
		}

		return className;
	}
}
