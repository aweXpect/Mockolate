using Microsoft.CodeAnalysis;

namespace Mockerade.SourceGenerators.Entities;

internal readonly record struct Event
{
	public Event(IEventSymbol eventSymbol, IMethodSymbol delegateInvokeMethod)
	{
		Accessibility = eventSymbol.DeclaredAccessibility;
		UseOverride = eventSymbol.IsVirtual || eventSymbol.IsAbstract;
		Name = eventSymbol.Name;
		Type = new Type(eventSymbol.Type);
		Delegate = new Method(delegateInvokeMethod);
	}

	public Method Delegate { get; }

	public Type Type { get; }

	public bool UseOverride { get; }

	public Accessibility Accessibility { get; }
	public string Name { get; }
}
