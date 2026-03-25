using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Mockolate.Exceptions;
using Mockolate.Interactions;
using Mockolate.Internals;
using Mockolate.Parameters;
using Mockolate.Setup;
using Mockolate.Verify;

namespace Mockolate;

public partial class MockRegistry
{
	/// <summary>
	///     Counts the invocations of methods matching the <paramref name="methodSetup" /> on the <paramref name="subject" />.
	/// </summary>
	public VerificationResult<T> Method<T>(T subject, IMethodSetup methodSetup)
	{
		if (methodSetup is not IVerifiableMethodSetup verifiableMethodSetup)
		{
			throw new MockException("The setup is not verifiable.");
		}

		return Method(subject, verifiableMethodSetup.GetMatch());
	}

	/// <summary>
	///     Counts the invocations of methods matching the <paramref name="methodMatch" /> on the <paramref name="subject" />.
	/// </summary>
	public VerificationResult<T> Method<T>(T subject, IMethodMatch methodMatch)
	{
		return new VerificationResult<T>(
			subject,
			Interactions,
			Predicate,
			() => $"invoked method {methodMatch}");

		[DebuggerNonUserCode]
		bool Predicate(IInteraction interaction)
		{
			return interaction is MethodInvocation method &&
			       methodMatch.Matches(method);
		}
	}

	/// <summary>
	///     Counts the getter accesses of property <paramref name="propertyName" /> on the <paramref name="subject" />.
	/// </summary>
	public VerificationResult<T> Property<T>(T subject, string propertyName)
	{
		return new VerificationResult<T>(subject,
			Interactions,
			Predicate,
			() => $"got property {propertyName.SubstringAfterLast('.')}");

		[DebuggerNonUserCode]
		bool Predicate(IInteraction interaction)
		{
			return interaction is PropertyGetterAccess property &&
			       property.Name.Equals(propertyName);
		}
	}

	/// <summary>
	///     Counts the setter accesses of property <paramref name="propertyName" />
	///     with the matching <paramref name="value" /> on the <paramref name="subject" />.
	/// </summary>
	public VerificationResult<T> Property<T>(T subject, string propertyName,
		IParameter value)
	{
		return new VerificationResult<T>(subject,
			Interactions,
			Predicate,
			() => $"set property {propertyName.SubstringAfterLast('.')} to value {value}");

		[DebuggerNonUserCode]
		bool Predicate(IInteraction interaction)
		{
			return interaction is PropertySetterAccess property &&
			       property.Name.Equals(propertyName) &&
			       value.Matches(property.Value);
		}
	}

	/// <summary>
	///     Counts the getter accesses of the indexer with matching <paramref name="parameters" /> on the
	///     <paramref name="subject" />.
	/// </summary>
	public VerificationResult<T> Indexer<T>(T subject,
		params NamedParameter[] parameters)
	{
		return new VerificationResult<T>(subject,
			Interactions,
			Predicate,
			() => $"got indexer [{string.Join(", ", parameters.Select(x => x.Parameter))}]");

		[DebuggerNonUserCode]
		bool Predicate(IInteraction interaction)
		{
			if (interaction is not IndexerGetterAccess indexer ||
			    indexer.Parameters.Length != parameters.Length)
			{
				return false;
			}

			for (int i = 0; i < parameters.Length; i++)
			{
				if (!parameters[i].Matches(indexer.Parameters[i]))
				{
					return false;
				}
			}

			return true;
		}
	}

	/// <summary>
	///     Counts the setter accesses of the indexer with matching <paramref name="parameters" /> to the given
	///     <paramref name="value" /> on the <paramref name="subject" />.
	/// </summary>
	public VerificationResult<T> Indexer<T>(T subject, IParameter value,
		params NamedParameter[] parameters)
	{
		return new VerificationResult<T>(subject,
			Interactions,
			Predicate,
			() => $"set indexer [{string.Join(", ", parameters.Select(x => x.Parameter))}] to value {value}");

		[DebuggerNonUserCode]
		bool Predicate(IInteraction interaction)
		{
			if (interaction is not IndexerSetterAccess indexer ||
			    indexer.Parameters.Length != parameters.Length ||
			    !value.Matches(indexer.Value))
			{
				return false;
			}

			for (int i = 0; i < parameters.Length; i++)
			{
				if (!parameters[i].Matches(indexer.Parameters[i]))
				{
					return false;
				}
			}

			return true;
		}
	}

	/// <summary>
	///     Counts the subscriptions to the event <paramref name="eventName" /> on the <paramref name="subject" />.
	/// </summary>
	public VerificationResult<T> SubscribedTo<T>(T subject, string eventName)
	{
		return new VerificationResult<T>(subject, Interactions,
			Predicate,
			() => $"subscribed to event {eventName.SubstringAfterLast('.')}");

		[DebuggerNonUserCode]
		bool Predicate(IInteraction interaction)
		{
			return interaction is EventSubscription @event &&
			       @event.Name.Equals(eventName);
		}
	}

	/// <summary>
	///     Counts the unsubscriptions from the event <paramref name="eventName" /> on the <paramref name="subject" />.
	/// </summary>
	public VerificationResult<T> UnsubscribedFrom<T>(T subject, string eventName)
	{
		return new VerificationResult<T>(subject, Interactions,
			Predicate,
			() => $"unsubscribed from event {eventName.SubstringAfterLast('.')}");

		[DebuggerNonUserCode]
		bool Predicate(IInteraction interaction)
		{
			return interaction is EventUnsubscription @event &&
			       @event.Name.Equals(eventName);
		}
	}

	/// <summary>
	///     Returns the setups that have not been used by the given <paramref name="interactions" />.
	/// </summary>
	public IReadOnlyCollection<ISetup> GetUnusedSetups(MockInteractions interactions)
	{
		List<ISetup> unusedSetups =
		[
			..Setup.Indexers.EnumerateUnusedSetupsBy(interactions),
			..Setup.Properties.EnumerateUnusedSetupsBy(interactions),
			..Setup.Methods.EnumerateUnusedSetupsBy(interactions),
		];
		return unusedSetups;
	}
}
