using Microsoft.CodeAnalysis;

namespace Mockerade.SourceGenerators.Entities;

internal readonly record struct MethodParameter
{
	public MethodParameter(IParameterSymbol parameterSymbol)
	{
		Type = new Type(parameterSymbol.Type);
		Name = parameterSymbol.Name;
		RefKind = parameterSymbol.RefKind;
	}

	public Type Type { get; }
	public string Name { get; }
	public RefKind RefKind { get; }
}
