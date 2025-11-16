using System.Linq;
using Mockolate.Interactions;
using Mockolate.Internals;
using Mockolate.Verify;

namespace Mockolate;

public partial class MockRegistration
{
	/// <summary>
	///     Counts the invocations of method <paramref name="methodName" /> with matching <paramref name="parameters" /> on the
	///     <paramref name="subject" />.
	/// </summary>
	public VerificationResult<T> Method<T>(T subject, string methodName, params Match.IParameter[] parameters) => new(
		subject,
		Interactions,
		Interactions.Interactions
			.OfType<MethodInvocation>()
			.Where(method =>
				method.Name.Equals(methodName) &&
				method.Parameters.Length == parameters.Length &&
				!parameters.Where((parameter, i) => !parameter.Matches(method.Parameters[i])).Any())
			.Cast<IInteraction>()
			.ToArray(),
		$"invoked method {methodName.SubstringAfterLast('.')}({string.Join(", ", parameters.Select(x => x.ToString()))})");

	/// <summary>
	///     Counts the invocations of method <paramref name="methodName" /> with matching <paramref name="parameters" /> on the
	///     <paramref name="subject" />.
	/// </summary>
	public VerificationResult<T> Method<T>(T subject, string methodName, Match.IParameters parameters) => new(subject,
		Interactions,
		Interactions.Interactions
			.OfType<MethodInvocation>()
			.Where(method =>
				method.Name.Equals(methodName) &&
				parameters.Matches(method.Parameters))
			.Cast<IInteraction>()
			.ToArray(),
		$"invoked method {methodName.SubstringAfterLast('.')}({parameters})");

	/// <summary>
	///     Counts the getter accesses of property <paramref name="propertyName" /> on the <paramref name="subject" />.
	/// </summary>
	public VerificationResult<T> Property<T>(T subject, string propertyName) => new(subject,
		Interactions,
		Interactions.Interactions
			.OfType<PropertyGetterAccess>()
			.Where(property => property.Name.Equals(propertyName))
			.Cast<IInteraction>()
			.ToArray(),
		$"got property {propertyName.SubstringAfterLast('.')}");

	/// <summary>
	///     Counts the setter accesses of property <paramref name="propertyName" />
	///     with the matching <paramref name="value" /> on the <paramref name="subject" />.
	/// </summary>
	public VerificationResult<T> Property<T>(T subject, string propertyName,
		Match.IParameter value)
		=> new(subject,
			Interactions,
			Interactions.Interactions
				.OfType<PropertySetterAccess>()
				.Where(property => property.Name.Equals(propertyName) && value.Matches(property.Value))
				.Cast<IInteraction>()
				.ToArray(),
			$"set property {propertyName.SubstringAfterLast('.')} to value {value}");

	/// <summary>
	///     Counts the getter accesses of the indexer with matching <paramref name="parameters" /> on the
	///     <paramref name="subject" />.
	/// </summary>
	public VerificationResult<T> Got<T>(T subject,
		params Match.IParameter?[] parameters)
		=> new(subject,
			Interactions,
			Interactions.Interactions
				.OfType<IndexerGetterAccess>()
				.Where(indexer => indexer.Parameters.Length == parameters.Length &&
				                  !parameters.Where((parameter, i) => parameter is null
					                  ? indexer.Parameters[i] is not null
					                  : !parameter.Matches(indexer.Parameters[i])).Any())
				.Cast<IInteraction>()
				.ToArray(),
			$"got indexer {string.Join(", ", parameters.Select(x => x?.ToString() ?? "null"))}");

	/// <summary>
	///     Counts the setter accesses of the indexer with matching <paramref name="parameters" /> to the given
	///     <paramref name="value" /> on the <paramref name="subject" />.
	/// </summary>
	public VerificationResult<T> Set<T>(T subject, Match.IParameter? value,
		params Match.IParameter?[] parameters)
		=> new(subject,
			Interactions,
			Interactions.Interactions
				.OfType<IndexerSetterAccess>()
				.Where(indexer => indexer.Parameters.Length == parameters.Length &&
				                  (value is null ? indexer.Value is null : value.Matches(indexer.Value)) &&
				                  !parameters.Where((parameter, i) => parameter is null
					                  ? indexer.Parameters[i] is not null
					                  : !parameter.Matches(indexer.Parameters[i])).Any())
				.Cast<IInteraction>()
				.ToArray(),
			$"set indexer {string.Join(", ", parameters.Select(x => x?.ToString() ?? "null"))} to value {value?.ToString() ?? "null"}");

	/// <summary>
	///     Counts the subscriptions to the event <paramref name="eventName" /> on the <paramref name="subject" />.
	/// </summary>
	public VerificationResult<T> SubscribedTo<T>(T subject, string eventName)
		=> new(subject, Interactions,
			Interactions.Interactions
				.OfType<EventSubscription>()
				.Where(@event => @event.Name.Equals(eventName))
				.Cast<IInteraction>()
				.ToArray(),
			$"subscribed to event {eventName.SubstringAfterLast('.')}");

	/// <summary>
	///     Counts the unsubscriptions from the event <paramref name="eventName" /> on the <paramref name="subject" />.
	/// </summary>
	public VerificationResult<T> UnsubscribedFrom<T>(T subject, string eventName)
		=> new(subject, Interactions,
			Interactions.Interactions
				.OfType<EventUnsubscription>()
				.Where(@event => @event.Name.Equals(eventName))
				.Cast<IInteraction>()
				.ToArray(),
			$"unsubscribed from event {eventName.SubstringAfterLast('.')}");
}
