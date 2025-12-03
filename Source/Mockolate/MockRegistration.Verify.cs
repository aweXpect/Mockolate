using System.Collections.Generic;
using System.Linq;
using Mockolate.Interactions;
using Mockolate.Internals;
using Mockolate.Setup;
using Mockolate.Verify;

namespace Mockolate;

public partial class MockRegistration
{
	/// <summary>
	///     Counts the invocations of method <paramref name="methodName" /> with matching <paramref name="parameters" /> on the
	///     <paramref name="subject" />.
	/// </summary>
	public VerificationResult<T> Method<T>(T subject, string methodName, params Match.NamedParameter[] parameters)
		=> new(
			subject,
			Interactions,
			Interactions.Interactions
				.OfType<MethodInvocation>()
				.Where(method =>
					method.Name.Equals(methodName) &&
					method.Parameters.Length == parameters.Length &&
					!parameters.Where((parameter, i) => !parameter.Parameter.Matches(method.Parameters[i])).Any())
				.Cast<IInteraction>()
				.ToArray(),
			$"invoked method {methodName.SubstringAfterLast('.')}({string.Join(", ", parameters.Select(x => x.Parameter.ToString()))})");

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
	public VerificationResult<T> Indexer<T>(T subject,
		params Match.NamedParameter?[] parameters)
		=> new(subject,
			Interactions,
			Interactions.Interactions
				.OfType<IndexerGetterAccess>()
				.Where(indexer => indexer.Parameters.Length == parameters.Length &&
				                  !parameters.Where((parameter, i) => parameter is null
					                  ? indexer.Parameters[i] is not null
					                  : !parameter.Parameter.Matches(indexer.Parameters[i])).Any())
				.Cast<IInteraction>()
				.ToArray(),
			$"got indexer {string.Join(", ", parameters.Select(x => x?.Parameter.ToString() ?? "null"))}");

	/// <summary>
	///     Counts the setter accesses of the indexer with matching <paramref name="parameters" /> to the given
	///     <paramref name="value" /> on the <paramref name="subject" />.
	/// </summary>
	public VerificationResult<T> Indexer<T>(T subject, Match.IParameter? value,
		params Match.NamedParameter?[] parameters)
		=> new(subject,
			Interactions,
			Interactions.Interactions
				.OfType<IndexerSetterAccess>()
				.Where(indexer => indexer.Parameters.Length == parameters.Length &&
				                  (value?.Matches(indexer.Value) ?? indexer.Value is null) &&
				                  !parameters.Where((parameter, i) => parameter is null
					                  ? indexer.Parameters[i] is not null
					                  : !parameter.Parameter.Matches(indexer.Parameters[i])).Any())
				.Cast<IInteraction>()
				.ToArray(),
			$"set indexer {string.Join(", ", parameters.Select(x => x?.Parameter.ToString() ?? "null"))} to value {value?.ToString() ?? "null"}");

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

	/// <summary>
	///     Returns the setups that have not been used by the given <paramref name="interactions" />.
	/// </summary>
	public Setups GetUnusedSetups(MockInteractions interactions)
	{
		List<IndexerSetup> indexers = [];
		foreach (IndexerSetup indexerSetup in _indexerSetups.Enumerate())
		{
			if (indexerSetup is IIndexerSetup setup && interactions.Interactions
				    .All(interaction => interaction is not IndexerAccess indexerAccess
				                        || !setup.Matches(indexerAccess)))
			{
				indexers.Add(indexerSetup);
			}
		}

		List<PropertySetup> properties = _propertySetups.Enumerate().Where(propertySetup => interactions.Interactions
				.All(interaction => (interaction is not PropertyGetterAccess propertyGetterAccess ||
				                     !propertySetup.Name.Equals(propertyGetterAccess.Name)) &&
				                    (interaction is not PropertySetterAccess propertySetterAccess ||
				                     !propertySetup.Name.Equals(propertySetterAccess.Name))))
			.ToList();

		List<MethodSetup> methods = [];
		foreach (MethodSetup methodSetup in _methodSetups.Enumerate())
		{
			if (methodSetup is IMethodSetup setup && interactions.Interactions
				    .All(interaction => interaction is not MethodInvocation methodInvocation
				                        || !setup.Matches(methodInvocation)))
			{
				methods.Add(methodSetup);
			}
		}

		return new Setups(indexers, properties, methods);
	}
}
