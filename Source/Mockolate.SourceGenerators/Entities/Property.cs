using Microsoft.CodeAnalysis;
using Mockolate.SourceGenerators.Internals;

namespace Mockolate.SourceGenerators.Entities;

internal record Property
{
	public Property(IPropertySymbol propertySymbol, List<Property>? alreadyDefinedProperties)
	{
		Accessibility = propertySymbol.DeclaredAccessibility;
		UseOverride = propertySymbol.IsVirtual || propertySymbol.IsAbstract;
		Name = propertySymbol.Name;
		Type = new Type(propertySymbol.Type);
		ContainingType = propertySymbol.ContainingType.ToDisplayString();
		IsIndexer = propertySymbol.IsIndexer;
		IsAbstract = propertySymbol.IsAbstract;
		if (IsIndexer && propertySymbol.Parameters.Length > 0)
		{
			IndexerParameters = new EquatableArray<MethodParameter>(
				propertySymbol.Parameters.Select(x => new MethodParameter(x)).ToArray());
		}

		if (alreadyDefinedProperties is not null)
		{
			if (alreadyDefinedProperties.Any(p => p.Name == Name))
			{
				ExplicitImplementation = ContainingType;
			}

			alreadyDefinedProperties.Add(this);
		}

		Getter = propertySymbol.GetMethod is null ? null : new Method(propertySymbol.GetMethod, null);
		Setter = propertySymbol.SetMethod is null ? null : new Method(propertySymbol.SetMethod, null);
	}

	public static IEqualityComparer<Property> EqualityComparer { get; } = new PropertyEqualityComparer();
	public bool IsIndexer { get; }
	public bool IsAbstract { get; }
	public EquatableArray<MethodParameter>? IndexerParameters { get; }
	public Type Type { get; }
	public string ContainingType { get; }
	public Method? Setter { get; }

	public Method? Getter { get; }

	public bool UseOverride { get; }

	public Accessibility Accessibility { get; }
	public string Name { get; }
	public string? ExplicitImplementation { get; }

	internal string GetUniqueNameString()
		=> $"\"{ContainingType}.{Name}\"";

	private sealed class PropertyEqualityComparer : IEqualityComparer<Property>
	{
		public bool Equals(Property x, Property y) => !x.IsIndexer && !y.IsIndexer && x.Name.Equals(y.Name) &&
		                                              x.ContainingType.Equals(y.ContainingType);

		public int GetHashCode(Property obj) => obj.Name.GetHashCode();
	}
}
