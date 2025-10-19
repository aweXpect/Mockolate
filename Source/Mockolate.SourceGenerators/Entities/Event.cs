using Microsoft.CodeAnalysis;

namespace Mockolate.SourceGenerators.Entities;

internal readonly record struct Event
{
	public Event(IEventSymbol eventSymbol, IMethodSymbol delegateInvokeMethod)
	{
		Accessibility = eventSymbol.DeclaredAccessibility;
		UseOverride = eventSymbol.IsVirtual || eventSymbol.IsAbstract;
		Name = eventSymbol.Name;
		Type = new Type(eventSymbol.Type);
		ContainingType = eventSymbol.ContainingType.ToDisplayString();
		Delegate = new Method(delegateInvokeMethod, null);
	}

	public Method Delegate { get; }

	public Type Type { get; }
	public string ContainingType { get; }
	public bool UseOverride { get; }

	public Accessibility Accessibility { get; }
	public string Name { get; }
}
