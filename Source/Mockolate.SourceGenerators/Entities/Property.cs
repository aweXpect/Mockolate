using System.Text;
using Mockolate.SourceGenerators.Internals;
using Microsoft.CodeAnalysis;

namespace Mockolate.SourceGenerators.Entities;

internal readonly record struct Property
{
	public Property(IPropertySymbol propertySymbol)
	{
		Accessibility = propertySymbol.DeclaredAccessibility;
		UseOverride = propertySymbol.IsVirtual || propertySymbol.IsAbstract;
		Name = propertySymbol.Name;
		Type = new Type(propertySymbol.Type);
		IsIndexer = propertySymbol.IsIndexer;
		if (IsIndexer && propertySymbol.Parameters.Length > 0)
		{
			IndexerParameter = new MethodParameter(propertySymbol.Parameters[0]);
		}
		Getter = propertySymbol.GetMethod is null ? null : new Method(propertySymbol.GetMethod);
		Setter = propertySymbol.SetMethod is null ? null : new Method(propertySymbol.SetMethod);
	}

	public bool IsIndexer { get; }
	public MethodParameter? IndexerParameter { get; }
	public Type Type { get; }

	public Method? Setter { get; }

	public Method? Getter { get; }

	public bool UseOverride { get; }

	public Accessibility Accessibility { get; }
	public string Name { get; }
}
