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
		Fullname = typeSymbol.ToDisplayString().Replace("*", "");
		Namespace = typeSymbol.ContainingNamespace?.ToString();
		IsArray = typeSymbol.TypeKind == TypeKind.Array;
		if (typeSymbol is IArrayTypeSymbol arrayType)
		{
			ElementType = new Type(arrayType.ElementType);
		}
		IsTypeParameter = typeSymbol.TypeKind == TypeKind.TypeParameter;
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
	}

	public bool IsArray { get; }
	public bool IsTypeParameter { get; }
	public EquatableArray<Type>? TupleTypes { get; }
	public EquatableArray<Type>? GenericTypeParameters { get; }
	public string? Namespace { get; }

	internal static Type Void { get; } = new("void");

	public string Fullname { get; }
	public Type? ElementType { get; }

	public override string ToString() => Fullname;
}
