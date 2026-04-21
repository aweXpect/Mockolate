using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Mockolate.SourceGenerators.Entities;

internal readonly record struct MethodParameter
{
	public MethodParameter(IParameterSymbol parameterSymbol)
	{
		Type = new Type(parameterSymbol.Type);
		Name = SyntaxFacts.GetKeywordKind(parameterSymbol.Name) != SyntaxKind.None
			? "@" + parameterSymbol.Name
			: parameterSymbol.Name;
		RefKind = parameterSymbol.RefKind;
		IsNullableAnnotated = parameterSymbol.NullableAnnotation == NullableAnnotation.Annotated;
		IsParams = parameterSymbol.IsParams;
		HasExplicitDefaultValue = parameterSymbol.HasExplicitDefaultValue;
		if (HasExplicitDefaultValue)
		{
			string? explicitDefaultValue = SymbolDisplay.FormatPrimitive(parameterSymbol.ExplicitDefaultValue, true, false);
			if (explicitDefaultValue == "null"
			    && parameterSymbol.Type.IsValueType
			    && parameterSymbol.Type is not INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T, })
			{
				ExplicitDefaultValue = "default";
			}
			else if (parameterSymbol.Type.TypeKind == TypeKind.Enum)
			{
				string enumTypeName = parameterSymbol.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
				ExplicitDefaultValue = "(" + enumTypeName + ")" + explicitDefaultValue;
			}
			else
			{
				ExplicitDefaultValue = AppendLiteralSuffix(explicitDefaultValue, parameterSymbol.Type);
			}
		}
	}

	// SymbolDisplay.FormatPrimitive strips the 'm'/'f' type suffix from the literal, which would
	// produce invalid C# (e.g. "decimal x = 19.95" is a narrowing conversion from double). Re-add
	// the suffix based on the effective parameter type so the generated code compiles.
	private static string? AppendLiteralSuffix(string? value, ITypeSymbol type)
	{
		if (value is null or "null")
		{
			return value;
		}

		ITypeSymbol effectiveType = type is INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T, } named
			? named.TypeArguments[0]
			: type;

		return effectiveType.SpecialType switch
		{
			SpecialType.System_Decimal => value + "m",
			SpecialType.System_Single => value + "f",
			_ => value,
		};
	}

	public string? ExplicitDefaultValue { get; }
	public bool HasExplicitDefaultValue { get; }
	public bool IsParams { get; }

	public bool IsNullableAnnotated { get; }
	public Type Type { get; }
	public string Name { get; }
	public RefKind RefKind { get; }
}
