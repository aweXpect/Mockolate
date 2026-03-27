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
				ExplicitDefaultValue = explicitDefaultValue;
			}
		}
	}

	public string? ExplicitDefaultValue { get; }
	public bool HasExplicitDefaultValue { get; }
	public bool IsParams { get; }

	public bool IsNullableAnnotated { get; }
	public Type Type { get; }
	public string Name { get; }
	public RefKind RefKind { get; }
}
