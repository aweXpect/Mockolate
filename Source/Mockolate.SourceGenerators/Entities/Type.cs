using Microsoft.CodeAnalysis;
using Mockolate.SourceGenerators.Internals;

namespace Mockolate.SourceGenerators.Entities;

internal record Type
{
	private Type(string fullname)
	{
		Fullname = fullname;
	}

	internal Type(ITypeSymbol typeSymbol)
	{
		// Removes '*' from multi-dimensional array types
		Fullname = typeSymbol.ToDisplayString(Helpers.TypeDisplayFormat).Replace("*", "");
		Namespace = typeSymbol.ContainingNamespace?.ToString();
		if (typeSymbol is INamedTypeSymbol namedTypeSymbol)
		{
			if (typeSymbol.IsTupleType)
			{
				TupleTypes = new EquatableArray<Type>(namedTypeSymbol.TupleElements
					.Select(x => new Type(x.Type))
					.ToArray());
			}

			GenericTypeParameters = new EquatableArray<Type>(namedTypeSymbol.TypeArguments
				.Select(x => new Type(x))
				.ToArray());
		}

		SpecialGenericType = typeSymbol.GetSpecialType();
		SpecialType = typeSymbol.SpecialType;
		CanBeNullable = typeSymbol.NullableAnnotation == NullableAnnotation.Annotated ||
		                typeSymbol is INamedTypeSymbol{ OriginalDefinition.SpecialType: SpecialType.System_Nullable_T, };
		IsFormattable = typeSymbol.AllInterfaces.Any(i => i.ContainingNamespace.ContainingNamespace.IsGlobalNamespace &&
		                                                  i.ContainingNamespace.Name == "System" &&
		                                                  i.Name == "IFormattable");
	}

	public bool IsFormattable { get; }
	public bool CanBeNullable { get; }
	public SpecialType SpecialType { get; }
	public SpecialGenericType SpecialGenericType { get; }
	public EquatableArray<Type>? TupleTypes { get; }
	public EquatableArray<Type>? GenericTypeParameters { get; }
	public string? Namespace { get; }

	internal static Type Void { get; } = new("void");

	public string Fullname { get; }
	public override string ToString() => Fullname;
}
