using System.Collections.Generic;
using System.Linq;
using Mockolate.Interactions;
using Mockolate.Internals;
using Mockolate.Parameters;
using Mockolate.Setup;
using Mockolate.Verify;

namespace Mockolate;

public partial class MockRegistration
{
	/// <summary>
	///     Counts the invocations of methods matching the <paramref name="methodMatch" /> on the <paramref name="subject" />.
	/// </summary>
	public VerificationResult<T> Method<T>(T subject, IMethodMatch methodMatch)
		=> new(
			subject,
			Interactions,
			interaction => interaction is MethodInvocation method &&
			               methodMatch.Matches(method),
			$"invoked method {methodMatch}");

	/// <summary>
	///     Counts the getter accesses of property <paramref name="propertyName" /> on the <paramref name="subject" />.
	/// </summary>
	public VerificationResult<T> Property<T>(T subject, string propertyName) => new(subject,
		Interactions,
		interaction => interaction is PropertyGetterAccess property &&
		               property.Name.Equals(propertyName),
		$"got property {propertyName.SubstringAfterLast('.')}");

	/// <summary>
	///     Counts the setter accesses of property <paramref name="propertyName" />
	///     with the matching <paramref name="value" /> on the <paramref name="subject" />.
	/// </summary>
	public VerificationResult<T> Property<T>(T subject, string propertyName,
		IParameter value)
		=> new(subject,
			Interactions,
			interaction => interaction is PropertySetterAccess property &&
			               property.Name.Equals(propertyName) &&
			               value.Matches(property.Value),
			$"set property {propertyName.SubstringAfterLast('.')} to value {value}");

	/// <summary>
	///     Counts the getter accesses of the indexer with matching <paramref name="parameters" /> on the
	///     <paramref name="subject" />.
	/// </summary>
	public VerificationResult<T> Indexer<T>(T subject,
		params NamedParameter[] parameters)
		=> new(subject,
			Interactions,
			interaction => interaction is IndexerGetterAccess indexer &&
			               indexer.Parameters.Length == parameters.Length &&
			               !parameters
				               .Where((parameter, i) => !parameter.Matches(indexer.Parameters[i]))
				               .Any(),
			$"got indexer [{string.Join(", ", parameters.Select(x => x.Parameter.ToString()))}]");

	/// <summary>
	///     Counts the setter accesses of the indexer with matching <paramref name="parameters" /> to the given
	///     <paramref name="value" /> on the <paramref name="subject" />.
	/// </summary>
	public VerificationResult<T> Indexer<T>(T subject, IParameter? value,
		params NamedParameter[] parameters)
		=> new(subject,
			Interactions,
			interaction => interaction is IndexerSetterAccess indexer &&
			               indexer.Parameters.Length == parameters.Length &&
			               (value?.Matches(indexer.Value) ?? indexer.Value is null) &&
			               !parameters
				               .Where((parameter, i) => !parameter.Matches(indexer.Parameters[i]))
				               .Any(),
			$"set indexer [{string.Join(", ", parameters.Select(x => x.Parameter.ToString()))}] to value {value?.ToString() ?? "null"}");

	/// <summary>
	///     Counts the subscriptions to the event <paramref name="eventName" /> on the <paramref name="subject" />.
	/// </summary>
	public VerificationResult<T> SubscribedTo<T>(T subject, string eventName)
		=> new(subject, Interactions,
			interaction => interaction is EventSubscription @event &&
			               @event.Name.Equals(eventName),
			$"subscribed to event {eventName.SubstringAfterLast('.')}");

	/// <summary>
	///     Counts the unsubscriptions from the event <paramref name="eventName" /> on the <paramref name="subject" />.
	/// </summary>
	public VerificationResult<T> UnsubscribedFrom<T>(T subject, string eventName)
		=> new(subject, Interactions,
			interaction => interaction is EventUnsubscription @event &&
			               @event.Name.Equals(eventName),
			$"unsubscribed from event {eventName.SubstringAfterLast('.')}");

	/// <summary>
	///     Returns the setups that have not been used by the given <paramref name="interactions" />.
	/// </summary>
	public IReadOnlyCollection<ISetup> GetUnusedSetups(MockInteractions interactions)
	{
		List<ISetup> unusedSetups =
		[
			.._indexerSetups.EnumerateUnusedSetupsBy(interactions),
			.._propertySetups.EnumerateUnusedSetupsBy(interactions),
			.._methodSetups.EnumerateUnusedSetupsBy(interactions),
		];
		return unusedSetups;
	}
}
