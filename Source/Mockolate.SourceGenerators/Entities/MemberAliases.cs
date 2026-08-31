namespace Mockolate.SourceGenerators.Entities;

/// <summary>
///     Maps a member of an additional interface onto the member of the mocked class that already
///     implements it, so both surfaces address a single mock member instead of two independent ones.
/// </summary>
/// <remarks>
///     Only populated for combination mocks (<c>Implementing&lt;TInterface&gt;()</c>) whose mocked
///     class implements <c>TInterface</c> with an overridable member. Filled once by
///     <see cref="Class.RebaseOnto" /> and read-only from then on: <c>Sources.ComputeMemberIds</c>
///     assigns ids against a finished map.
/// </remarks>
internal sealed class MemberAliases
{
	public Dictionary<Method, Method> Methods { get; } = new();
	public Dictionary<Property, Property> Properties { get; } = new();
	public Dictionary<Event, Event> Events { get; } = new();

	/// <summary>
	///     Records that <paramref name="alias" /> resolves to <paramref name="target" />.
	/// </summary>
	/// <remarks>
	///     Rebasing only rewrites the containing type, so an interface member that already matches the
	///     class member in every other respect ends up equal to it. Recording that as an alias would make
	///     <see cref="IsAlias(Method)" /> true for the class member itself, which then never gets an id -
	///     and resolving it later would throw. Both surfaces already share one id in that case, by plain
	///     record equality, so there is nothing to record.
	/// </remarks>
	public void Add(Method alias, Method target)
	{
		if (!alias.Equals(target))
		{
			Methods[alias] = target;
		}
	}

	/// <inheritdoc cref="Add(Method,Method)" />
	public void Add(Property alias, Property target)
	{
		if (!alias.Equals(target))
		{
			Properties[alias] = target;
		}
	}

	/// <inheritdoc cref="Add(Method,Method)" />
	public void Add(Event alias, Event target)
	{
		if (!alias.Equals(target))
		{
			Events[alias] = target;
		}
	}

	public Method Resolve(Method method) => Methods.TryGetValue(method, out Method? target) ? target : method;

	public Property Resolve(Property property)
		=> Properties.TryGetValue(property, out Property? target) ? target : property;

	public Event Resolve(Event @event) => Events.TryGetValue(@event, out Event? target) ? target : @event;

	public bool IsAlias(Method method) => Methods.ContainsKey(method);

	public bool IsAlias(Property property) => Properties.ContainsKey(property);

	public bool IsAlias(Event @event) => Events.ContainsKey(@event);
}
