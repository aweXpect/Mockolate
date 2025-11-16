using Microsoft.CodeAnalysis;

namespace Mockolate.SourceGenerators.Entities;

internal record Event
{
	public Event(IEventSymbol eventSymbol, IMethodSymbol delegateInvokeMethod, List<Event>? alreadyDefinedEvents)
	{
		Accessibility = eventSymbol.DeclaredAccessibility;
		UseOverride = eventSymbol.IsVirtual || eventSymbol.IsAbstract;
		Name = eventSymbol.Name;
		Type = new Type(eventSymbol.Type);
		ContainingType = eventSymbol.ContainingType.ToDisplayString();
		Delegate = new Method(delegateInvokeMethod, null);
		Obsolete = eventSymbol.GetAttributes().GetObsoleteAttribute();

		if (alreadyDefinedEvents is not null)
		{
			if (alreadyDefinedEvents.Any(p => p.Name == Name))
			{
				ExplicitImplementation = ContainingType;
			}

			alreadyDefinedEvents.Add(this);
		}
	}

	public ObsoleteAttribute? Obsolete { get; }

	public static IEqualityComparer<Event> EqualityComparer { get; } = new EventEqualityComparer();

	public static IEqualityComparer<Event> ContainingTypeIndependentEqualityComparer { get; } =
		new ContainingTypeIndependentEventEqualityComparer();

	public Method Delegate { get; }

	public Type Type { get; }
	public string ContainingType { get; }
	public bool UseOverride { get; }

	public Accessibility Accessibility { get; }
	public string Name { get; }
	public string? ExplicitImplementation { get; }

	internal string GetUniqueNameString()
		=> $"\"{ContainingType}.{Name}\"";

	private sealed class EventEqualityComparer : IEqualityComparer<Event>
	{
		public bool Equals(Event x, Event y) => x.Name.Equals(y.Name) && x.ContainingType.Equals(y.ContainingType);
		public int GetHashCode(Event obj) => obj.Name.GetHashCode();
	}

	private sealed class ContainingTypeIndependentEventEqualityComparer : IEqualityComparer<Event>
	{
		public bool Equals(Event x, Event y) => x.Name.Equals(y.Name);
		public int GetHashCode(Event obj) => obj.Name.GetHashCode();
	}
}
