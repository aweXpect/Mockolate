using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Mockolate.SourceGenerators.Internals;

namespace Mockolate.SourceGenerators.Entities;

[DebuggerDisplay("{ContainingType}.{Name}")]
internal record Event
{
	public Event(IEventSymbol eventSymbol, IMethodSymbol delegateInvokeMethod, List<Event>? alreadyDefinedEvents)
	{
		Accessibility = eventSymbol.DeclaredAccessibility;
		UseOverride = eventSymbol.IsVirtual || eventSymbol.IsAbstract;
		IsAbstract = eventSymbol.IsAbstract;
		Name = eventSymbol.ExplicitInterfaceImplementations.Length > 0 ? eventSymbol.ExplicitInterfaceImplementations[0].Name : eventSymbol.Name;
		Type = new Type(eventSymbol.Type);
		ContainingType = eventSymbol.ContainingType.ToDisplayString(Helpers.TypeDisplayFormat);
		Delegate = new Method(delegateInvokeMethod, null);
		Attributes = eventSymbol.GetAttributes().ToAttributeArray();
		IsStatic = eventSymbol.IsStatic;

		if (alreadyDefinedEvents is not null)
		{
			if (alreadyDefinedEvents.Any(p => p.Name == Name))
			{
				ExplicitImplementation = ContainingType;
			}

			alreadyDefinedEvents.Add(this);
		}
	}

	public EquatableArray<Attribute>? Attributes { get; }

	public static IEqualityComparer<Event> EqualityComparer { get; } = new EventEqualityComparer();

	public static IEqualityComparer<Event> ContainingTypeIndependentEqualityComparer { get; } =
		new ContainingTypeIndependentEventEqualityComparer();

	public Method Delegate { get; }

	public Type Type { get; }
	public string ContainingType { get; }
	public bool UseOverride { get; }
	public bool IsAbstract { get; }
	public bool IsStatic { get; }
	public bool IsProtected => Accessibility is Accessibility.Protected or Accessibility.ProtectedOrInternal;

	public MemberType MemberType => (IsStatic, IsProtected) switch
	{
		(true, _) => MemberType.Static,
		(_, true) => MemberType.Protected,
		(_, _) => MemberType.Public
	};
	public Accessibility Accessibility { get; }
	public string Name { get; }
	public string? ExplicitImplementation { get; }

	internal string GetUniqueNameString()
		=> $"\"{ContainingType}.{Name}\"";

	internal string GetBackingFieldName()
	{
		char[] sanitized = ContainingType.Select(c => char.IsLetterOrDigit(c) ? c : '_').ToArray();
		return $"_mockolateEvent_{new string(sanitized)}_{Name}";
	}

	private sealed class EventEqualityComparer : IEqualityComparer<Event>
	{
		public bool Equals(Event? x, Event? y)
			=> (x is null && y is null) ||
			   (x is not null && y is not null &&
			    x.Name.Equals(y.Name) &&
			    x.ContainingType.Equals(y.ContainingType));

		public int GetHashCode(Event obj) => obj.Name.GetHashCode();
	}

	private sealed class ContainingTypeIndependentEventEqualityComparer : IEqualityComparer<Event>
	{
		public bool Equals(Event? x, Event? y)
			=> (x is null && y is null) ||
			   (x is not null && y is not null &&
			    x.Name.Equals(y.Name));

		public int GetHashCode(Event obj) => obj.Name.GetHashCode();
	}
}
