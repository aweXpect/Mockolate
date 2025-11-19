using Microsoft.CodeAnalysis;

namespace Mockolate.SourceGenerators.Entities;

internal readonly record struct MethodParameter
{
	public MethodParameter(IParameterSymbol parameterSymbol)
	{
		Type = new Type(parameterSymbol.Type);
		Name = parameterSymbol.Name;
		RefKind = parameterSymbol.RefKind;
		if (parameterSymbol.Type.ContainingNamespace?.Name == "System" &&
		    parameterSymbol.Type.ContainingNamespace.ContainingNamespace?.IsGlobalNamespace == true)
		{
			IsSpan = parameterSymbol.Type.Name == "Span";
			IsReadOnlySpan = parameterSymbol.Type.Name == "ReadOnlySpan";
			if (IsSpan || IsReadOnlySpan)
			{
				INamedTypeSymbol namedTypeSymbol = (INamedTypeSymbol)parameterSymbol.Type;
				if (namedTypeSymbol.TypeArguments.Length == 1)
				{
					ITypeSymbol elementType = namedTypeSymbol.TypeArguments[0];
					SpanType = new Type(elementType);
				}
				else
				{
					IsSpan = false;
					IsReadOnlySpan = false;
				}
			}
		}
	}

	public Type Type { get; }
	public string Name { get; }
	public RefKind RefKind { get; }
	public bool IsSpan { get; }
	public bool IsReadOnlySpan { get; }
	public Type? SpanType { get; }
}
