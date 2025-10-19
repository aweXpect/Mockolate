using Microsoft.CodeAnalysis;
using Mockolate.SourceGenerators.Internals;

namespace Mockolate.SourceGenerators.Entities;

internal readonly record struct Property
{
	public Property(IPropertySymbol propertySymbol)
	{
		Accessibility = propertySymbol.DeclaredAccessibility;
		UseOverride = propertySymbol.IsVirtual || propertySymbol.IsAbstract;
		Name = propertySymbol.Name;
		Type = new Type(propertySymbol.Type);
		ContainingType = propertySymbol.ContainingType.ToDisplayString();
		IsIndexer = propertySymbol.IsIndexer;
		if (IsIndexer && propertySymbol.Parameters.Length > 0)
		{
			IndexerParameters = new EquatableArray<MethodParameter>(
				propertySymbol.Parameters.Select(x => new MethodParameter(x)).ToArray());
		}

		Getter = propertySymbol.GetMethod is null ? null : new Method(propertySymbol.GetMethod);
		Setter = propertySymbol.SetMethod is null ? null : new Method(propertySymbol.SetMethod);
	}

	public bool IsIndexer { get; }
	public EquatableArray<MethodParameter>? IndexerParameters { get; }
	public Type Type { get; }
	public string ContainingType { get; }
	public Method? Setter { get; }

	public Method? Getter { get; }

	public bool UseOverride { get; }

	public Accessibility Accessibility { get; }
	public string Name { get; }
}
