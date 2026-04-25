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
	///     Member-id-keyed fast-path overload of
	///     <see cref="VerifyMethod{T, TMethod}(T, string, Func{TMethod, bool}, Func{string})" />. When the mock is
	///     wired to a <see cref="FastMockInteractions" /> with a typed buffer at <paramref name="memberId" />, the
	///     resulting <see cref="VerificationResult{T}.IgnoreParameters" /> walks only that buffer instead of the
	///     full shared interaction list. Falls back to the legacy full-list scan otherwise.
	/// </summary>
	/// <typeparam name="T">The verification facade type; returned to the caller so chaining can continue.</typeparam>
	/// <typeparam name="TMethod">The concrete <see cref="IMethodInteraction" /> subtype to filter on.</typeparam>
	/// <param name="subject">The verification facade the result is bound to.</param>
	/// <param name="memberId">The generator-emitted member id for the method.</param>
	/// <param name="methodName">The simple method name (e.g. <c>"Greet"</c>).</param>
	/// <param name="predicate">Argument predicate applied to each matching interaction.</param>
	/// <param name="expectation">Factory producing the expectation description used in failure messages.</param>
	public VerificationResult<T>.IgnoreParameters VerifyMethod<T, TMethod>(T subject, int memberId, string methodName, Func<TMethod, bool> predicate, Func<string> expectation) where TMethod : IMethodInteraction
	{
		IFastMemberBuffer? buffer = TryGetBuffer(memberId);
		if (buffer is null)
		{
			return VerifyMethod<T, TMethod>(subject, methodName, predicate, expectation);
		}

		return new VerificationResult<T>.IgnoreParameters(
			subject,
			Interactions,
			buffer,
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
	///     Member-id-keyed fast-path overload of <see cref="VerifyProperty{T}(T, string)" />. Walks only the typed
	///     getter buffer when the mock is wired to a <see cref="FastMockInteractions" />.
	/// </summary>
	/// <typeparam name="T">The verification facade type.</typeparam>
	/// <param name="subject">The verification facade the result is bound to.</param>
	/// <param name="memberId">The generator-emitted member id for the property getter.</param>
	/// <param name="propertyName">The simple property name.</param>
	public VerificationResult<T> VerifyProperty<T>(T subject, int memberId, string propertyName)
	{
		IFastMemberBuffer? buffer = TryGetBuffer(memberId);
		if (buffer is null)
		{
			return VerifyProperty(subject, propertyName);
		}

		return new VerificationResult<T>(subject,
			Interactions,
			buffer,
			static _ => true,
			() => $"got property {propertyName.SubstringAfterLast('.')}");
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
	///     Member-id-keyed fast-path overload of
	///     <see cref="VerifyProperty{TSubject, TValue}(TSubject, string, IParameterMatch{TValue})" />. Walks only the
	///     typed setter buffer when the mock is wired to a <see cref="FastMockInteractions" />.
	/// </summary>
	/// <typeparam name="TSubject">The verification facade type.</typeparam>
	/// <typeparam name="TValue">The property's type.</typeparam>
	/// <param name="subject">The verification facade the result is bound to.</param>
	/// <param name="memberId">The generator-emitted member id for the property setter.</param>
	/// <param name="propertyName">The simple property name.</param>
	/// <param name="value">Parameter matcher evaluated against the assigned value.</param>
	public VerificationResult<TSubject> VerifyProperty<TSubject, TValue>(TSubject subject, int memberId,
		string propertyName, IParameterMatch<TValue> value)
	{
		IFastMemberBuffer? buffer = TryGetBuffer(memberId);
		if (buffer is null)
		{
			return VerifyProperty(subject, propertyName, value);
		}

		return new VerificationResult<TSubject>(subject,
			Interactions,
			buffer,
			Predicate,
			() => $"set property {propertyName.SubstringAfterLast('.')} to {value}");

		[DebuggerNonUserCode]
		bool Predicate(IInteraction interaction)
		{
			return interaction is PropertySetterAccess<TValue> property &&
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
	///     Member-id-keyed fast-path overload of
	///     <see cref="IndexerGot{T}(T, Func{IInteraction, bool}, Func{string})" />. Walks only the typed indexer
	///     getter buffer when the mock is wired to a <see cref="FastMockInteractions" />.
	/// </summary>
	/// <typeparam name="T">The verification facade type.</typeparam>
	/// <param name="subject">The verification facade the result is bound to.</param>
	/// <param name="memberId">The generator-emitted member id for the indexer getter.</param>
	/// <param name="gotPredicate">Predicate evaluated against each recorded interaction.</param>
	/// <param name="parametersDescription">Factory producing the indexer-argument description used in failure messages.</param>
	public VerificationResult<T> IndexerGot<T>(T subject, int memberId,
		Func<IInteraction, bool> gotPredicate,
		Func<string> parametersDescription)
	{
		IFastMemberBuffer? buffer = TryGetBuffer(memberId);
		if (buffer is null)
		{
			return IndexerGot(subject, gotPredicate, parametersDescription);
		}

		return new VerificationResult<T>(subject,
			Interactions,
			buffer,
			gotPredicate,
			() => $"got indexer {parametersDescription()}");
	}

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
	///     Member-id-keyed fast-path overload of
	///     <see cref="IndexerSet{T, TValue}(T, Func{IInteraction, IParameterMatch{TValue}, bool}, IParameterMatch{TValue}, Func{string})" />.
	///     Walks only the typed indexer setter buffer when the mock is wired to a <see cref="FastMockInteractions" />.
	/// </summary>
	/// <typeparam name="T">The verification facade type.</typeparam>
	/// <typeparam name="TValue">The indexer's value type.</typeparam>
	/// <param name="subject">The verification facade the result is bound to.</param>
	/// <param name="memberId">The generator-emitted member id for the indexer setter.</param>
	/// <param name="setPredicate">Predicate evaluated against each recorded interaction and the expected value matcher.</param>
	/// <param name="value">Parameter matcher evaluated against the assigned value.</param>
	/// <param name="parametersDescription">Factory producing the indexer-argument description used in failure messages.</param>
	public VerificationResult<T> IndexerSet<T, TValue>(T subject, int memberId,
		Func<IInteraction, IParameterMatch<TValue>, bool> setPredicate,
		IParameterMatch<TValue> value,
		Func<string> parametersDescription)
	{
		IFastMemberBuffer? buffer = TryGetBuffer(memberId);
		if (buffer is null)
		{
			return IndexerSet(subject, setPredicate, value, parametersDescription);
		}

		return new VerificationResult<T>(subject,
			Interactions,
			buffer,
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
	///     Member-id-keyed fast-path overload of <see cref="SubscribedTo{T}(T, string)" />. Walks only the typed
	///     event-subscribe buffer when the mock is wired to a <see cref="FastMockInteractions" />.
	/// </summary>
	/// <typeparam name="T">The verification facade type.</typeparam>
	/// <param name="subject">The verification facade the result is bound to.</param>
	/// <param name="memberId">The generator-emitted member id for the event subscribe.</param>
	/// <param name="eventName">The simple event name.</param>
	public VerificationResult<T> SubscribedTo<T>(T subject, int memberId, string eventName)
	{
		IFastMemberBuffer? buffer = TryGetBuffer(memberId);
		if (buffer is null)
		{
			return SubscribedTo(subject, eventName);
		}

		return new VerificationResult<T>(subject, Interactions,
			buffer,
			static _ => true,
			() => $"subscribed to event {eventName.SubstringAfterLast('.')}");
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
	///     Member-id-keyed fast-path overload of <see cref="UnsubscribedFrom{T}(T, string)" />. Walks only the typed
	///     event-unsubscribe buffer when the mock is wired to a <see cref="FastMockInteractions" />.
	/// </summary>
	/// <typeparam name="T">The verification facade type.</typeparam>
	/// <param name="subject">The verification facade the result is bound to.</param>
	/// <param name="memberId">The generator-emitted member id for the event unsubscribe.</param>
	/// <param name="eventName">The simple event name.</param>
	public VerificationResult<T> UnsubscribedFrom<T>(T subject, int memberId, string eventName)
	{
		IFastMemberBuffer? buffer = TryGetBuffer(memberId);
		if (buffer is null)
		{
			return UnsubscribedFrom(subject, eventName);
		}

		return new VerificationResult<T>(subject, Interactions,
			buffer,
			static _ => true,
			() => $"unsubscribed from event {eventName.SubstringAfterLast('.')}");
	}

	private IFastMemberBuffer? TryGetBuffer(int memberId)
	{
		if (Interactions is FastMockInteractions fast)
		{
			IFastMemberBuffer?[] buffers = fast.Buffers;
			if ((uint)memberId < (uint)buffers.Length)
			{
				return buffers[memberId];
			}
		}

		return null;
	}

	/// <summary>
	///     Returns every registered setup (indexer, property, method) that was not hit by any of the given
	///     <paramref name="interactions" />.
	/// </summary>
	/// <param name="interactions">The interactions to check against.</param>
	/// <returns>The unused setups; empty when every setup was exercised.</returns>
	public IReadOnlyCollection<ISetup> GetUnusedSetups(IMockInteractions interactions)
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
