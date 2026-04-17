using System;
using System.Collections.Generic;
using System.Diagnostics;
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
	///     Counts the invocations of methods with <paramref name="methodName" /> matching the <paramref name="predicate" /> on the <paramref name="subject" />.
	/// </summary>
	public VerificationResult<T>.IgnoreParameters VerifyMethod<T, TMethod>(T subject, string methodName, Func<TMethod, bool> predicate, Func<string> expectation) where TMethod : IMethodInteraction
	{
		return new VerificationResult<T>.IgnoreParameters(
			subject,
			Interactions,
			methodName,
			Predicate,
			() => $"invoked method {expectation()}");

		[DebuggerNonUserCode]
		bool Predicate(IInteraction interaction)
		{
			return interaction is TMethod method && predicate(method);
		}
	}

	/// <summary>
	///     Counts the getter accesses of property <paramref name="propertyName" /> on the <paramref name="subject" />.
	/// </summary>
	public VerificationResult<T> VerifyProperty<T>(T subject, string propertyName)
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
	public VerificationResult<TSubject> VerifyProperty<TSubject, TValue>(TSubject subject, string propertyName,
		IParameterMatch<TValue> value)
	{
		return new VerificationResult<TSubject>(subject,
			Interactions,
			Predicate,
			() => $"set property {propertyName.SubstringAfterLast('.')} to {value}");

		[DebuggerNonUserCode]
		bool Predicate(IInteraction interaction)
		{
			return interaction is PropertySetterAccess<TValue> property &&
			       property.Name.Equals(propertyName) &&
			       value.Matches(property.Value);
		}
	}

	/// <summary>
	///     Counts the invocations of methods matching the <paramref name="methodSetup" /> on the <paramref name="subject" />.
	/// </summary>
	public VerificationResult<T> Method<T>(T subject, IMethodSetup methodSetup)
	{
		if (methodSetup is not IVerifiableMethodSetup verifiableMethodSetup)
		{
			throw new MockException("The setup is not verifiable.");
		}

		return new VerificationResult<T>.IgnoreParameters(
			subject,
			Interactions,
			methodSetup.Name,
			Predicate,
			() => $"invoked method {methodSetup}");

		[DebuggerNonUserCode]
		bool Predicate(IInteraction interaction)
		{
			return interaction is IMethodInteraction methodInteraction &&
			       verifiableMethodSetup.Matches(methodInteraction);
		}
	}

	/// <summary>
	///     Counts the getter accesses of the indexer matching <paramref name="gotPredicate" /> on the
	///     <paramref name="subject" />.
	/// </summary>
	public VerificationResult<T> IndexerGot<T>(T subject,
		Func<IInteraction, bool> gotPredicate,
		Func<string> parametersDescription)
		=> new(subject,
			Interactions,
			gotPredicate,
			() => $"got indexer {parametersDescription()}");

	/// <summary>
	///     Counts the setter accesses of the indexer matching <paramref name="setPredicate" /> to the given
	///     <paramref name="value" /> on the <paramref name="subject" />.
	/// </summary>
	public VerificationResult<T> IndexerSet<T, TValue>(T subject,
		Func<IInteraction, IParameterMatch<TValue>, bool> setPredicate,
		IParameterMatch<TValue> value,
		Func<string> parametersDescription)
	{
		return new VerificationResult<T>(subject,
			Interactions,
			Predicate,
			() => $"set indexer {parametersDescription()} to {value}");

		[DebuggerNonUserCode]
		bool Predicate(IInteraction interaction)
		{
			return setPredicate(interaction, value);
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
