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
	///     Counts invocations of the method named <paramref name="methodName" /> on <paramref name="subject" /> whose
	///     recorded interaction satisfies <paramref name="predicate" />.
	/// </summary>
	/// <typeparam name="T">The verification facade type; returned to the caller so chaining can continue.</typeparam>
	/// <typeparam name="TMethod">The concrete <see cref="IMethodInteraction" /> subtype to filter on.</typeparam>
	/// <param name="subject">The verification facade the result is bound to.</param>
	/// <param name="methodName">The simple method name (e.g. <c>"Greet"</c>).</param>
	/// <param name="predicate">Argument predicate applied to each matching interaction.</param>
	/// <param name="expectation">Factory producing the expectation description used in failure messages.</param>
	/// <returns>A verification result exposing <c>AnyParameters()</c> in addition to the usual terminators.</returns>
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
	///     Counts getter accesses of the property named <paramref name="propertyName" /> on <paramref name="subject" />.
	/// </summary>
	/// <typeparam name="T">The verification facade type.</typeparam>
	/// <param name="subject">The verification facade the result is bound to.</param>
	/// <param name="propertyName">The simple property name.</param>
	/// <returns>A verification result pending a count terminator (e.g. <c>.Once()</c>).</returns>
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
	///     Counts setter accesses of the property named <paramref name="propertyName" /> on <paramref name="subject" />
	///     where the assigned value matches <paramref name="value" />.
	/// </summary>
	/// <typeparam name="TSubject">The verification facade type.</typeparam>
	/// <typeparam name="TValue">The property's type.</typeparam>
	/// <param name="subject">The verification facade the result is bound to.</param>
	/// <param name="propertyName">The simple property name.</param>
	/// <param name="value">Parameter matcher evaluated against the assigned value.</param>
	/// <returns>A verification result pending a count terminator.</returns>
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
	///     Counts invocations on <paramref name="subject" /> that match the <paramref name="methodSetup" />.
	/// </summary>
	/// <typeparam name="T">The verification facade type.</typeparam>
	/// <param name="subject">The verification facade the result is bound to.</param>
	/// <param name="methodSetup">An existing method setup. Must also implement <see cref="IVerifiableMethodSetup" />.</param>
	/// <returns>A verification result pending a count terminator.</returns>
	/// <exception cref="MockException"><paramref name="methodSetup" /> does not implement <see cref="IVerifiableMethodSetup" />.</exception>
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
	///     Counts indexer getter accesses on <paramref name="subject" /> whose recorded interaction satisfies
	///     <paramref name="gotPredicate" />.
	/// </summary>
	/// <typeparam name="T">The verification facade type.</typeparam>
	/// <param name="subject">The verification facade the result is bound to.</param>
	/// <param name="gotPredicate">Predicate evaluated against each recorded interaction.</param>
	/// <param name="parametersDescription">Factory producing the indexer-argument description used in failure messages.</param>
	/// <returns>A verification result pending a count terminator.</returns>
	public VerificationResult<T> IndexerGot<T>(T subject,
		Func<IInteraction, bool> gotPredicate,
		Func<string> parametersDescription)
		=> new(subject,
			Interactions,
			gotPredicate,
			() => $"got indexer {parametersDescription()}");

	/// <summary>
	///     Counts indexer setter accesses on <paramref name="subject" /> where the recorded interaction matches
	///     <paramref name="setPredicate" /> and the assigned value matches <paramref name="value" />.
	/// </summary>
	/// <typeparam name="T">The verification facade type.</typeparam>
	/// <typeparam name="TValue">The indexer's value type.</typeparam>
	/// <param name="subject">The verification facade the result is bound to.</param>
	/// <param name="setPredicate">Predicate evaluated against each recorded interaction and the expected value matcher.</param>
	/// <param name="value">Parameter matcher evaluated against the assigned value.</param>
	/// <param name="parametersDescription">Factory producing the indexer-argument description used in failure messages.</param>
	/// <returns>A verification result pending a count terminator.</returns>
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
	///     Counts subscriptions (<c>+=</c>) to the event named <paramref name="eventName" /> on <paramref name="subject" />.
	/// </summary>
	/// <typeparam name="T">The verification facade type.</typeparam>
	/// <param name="subject">The verification facade the result is bound to.</param>
	/// <param name="eventName">The simple event name.</param>
	/// <returns>A verification result pending a count terminator.</returns>
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
	///     Counts unsubscriptions (<c>-=</c>) from the event named <paramref name="eventName" /> on <paramref name="subject" />.
	/// </summary>
	/// <typeparam name="T">The verification facade type.</typeparam>
	/// <param name="subject">The verification facade the result is bound to.</param>
	/// <param name="eventName">The simple event name.</param>
	/// <returns>A verification result pending a count terminator.</returns>
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
	///     Returns every registered setup (indexer, property, method) that was not hit by any of the given
	///     <paramref name="interactions" />.
	/// </summary>
	/// <param name="interactions">The interactions to check against.</param>
	/// <returns>The unused setups; empty when every setup was exercised.</returns>
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
