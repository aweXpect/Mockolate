using Microsoft.CodeAnalysis;

namespace Mockolate.SourceGenerators.Entities;

internal readonly record struct MethodParameter
{
	public MethodParameter(IParameterSymbol parameterSymbol)
	{
		Type = new Type(parameterSymbol.Type);
		Name = parameterSymbol.Name;
		RefKind = parameterSymbol.RefKind;
		if (parameterSymbol.Type.IsSpanOrReadOnlySpan(out bool isSpan, out bool isReadOnlySpan, out Type? spanType))
		{
			IsSpan = isSpan;
			IsReadOnlySpan = isReadOnlySpan;
			SpanType = spanType;
		}
	}

	public Type Type { get; }
	public string Name { get; }
	public RefKind RefKind { get; }
	public bool IsSpan { get; }
	public bool IsReadOnlySpan { get; }
	public Type? SpanType { get; }
}
