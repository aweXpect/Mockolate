using System.Linq;
using Mockolate.Interactions;
using Mockolate.Internals;
using Mockolate.Match;

namespace Mockolate.Verify;

#pragma warning disable S2326 // Unused type parameters should be removed
#pragma warning disable S1939 // Inheritance list should not be redundant
/// <summary>
///     Verifies the <paramref name="interactions" /> with the mocked subject in the <typeparamref name="TMock" />
///     <paramref name="mock" />.
/// </summary>
public class MockVerify<T, TMock>(MockInteractions interactions, TMock mock, string prefix) : IMockVerify<TMock>, IMockVerify<T, TMock>,
	IMockInvoked<IMockVerify<T, TMock>>,
	IMockVerifyInvoked<T, TMock>, IMockVerifyInvokedProtected<T, TMock>,
	IMockVerifyInvokedWithToStringWithEqualsWithGetHashCode<T, TMock>, IMockVerifyInvokedWithEqualsWithGetHashCode<T, TMock>, IMockVerifyInvokedWithToStringWithGetHashCode<T, TMock>, IMockVerifyInvokedWithToStringWithEquals<T, TMock>,
	IMockGot<IMockVerify<T, TMock>>,
	IMockVerifyGot<T, TMock>, IMockVerifyGotProtected<T, TMock>,
	IMockSet<IMockVerify<T, TMock>>,
	IMockVerifySet<T, TMock>, IMockVerifySetProtected<T, TMock>,
	IMockGotIndexer<IMockVerify<T, TMock>>,
	IMockVerifyGotIndexer<T, TMock>, IMockVerifyGotIndexerProtected<T, TMock>,
	IMockSetIndexer<IMockVerify<T, TMock>>,
	IMockVerifySetIndexer<T, TMock>, IMockVerifySetIndexerProtected<T, TMock>,
	IMockSubscribedTo<IMockVerify<T, TMock>>,
	IMockVerifySubscribedTo<T, TMock>, IMockVerifySubscribedToProtected<T, TMock>,
	IMockUnsubscribedFrom<IMockVerify<T, TMock>>,
	IMockVerifyUnsubscribedFrom<T, TMock>, IMockVerifyUnsubscribedFromProtected<T, TMock>
{
	/// <inheritdoc cref="IMockVerify.Interactions" />
	MockInteractions IMockVerify.Interactions
		=> interactions;

	/// <inheritdoc cref="IMockVerify{TMock}.Mock" />
	TMock IMockVerify<TMock>.Mock
		=> mock;

	/// <inheritdoc cref="IMockInvoked{TMock}.Method(string, IParameter[])" />
	VerificationResult<IMockVerify<T, TMock>> IMockInvoked<IMockVerify<T, TMock>>.Method(string methodName,
		params IParameter[] parameters)
	{
		return new VerificationResult<IMockVerify<T, TMock>>(this, interactions,
			interactions.Interactions
				.OfType<MethodInvocation>()
				.Where(method =>
					method.Name.Equals(methodName) &&
					method.Parameters.Length == parameters.Length &&
					!parameters.Where((parameter, i) => !parameter.Matches(method.Parameters[i])).Any())
				.Cast<IInteraction>()
				.ToArray(),
			$"invoked method {methodName.SubstringAfterLast('.')}({string.Join(", ", parameters.Select(x => x.ToString()))})");
	}

	/// <inheritdoc cref="IMockInvoked{TMock}.Method(string, IParameters)" />
	VerificationResult<IMockVerify<T, TMock>> IMockInvoked<IMockVerify<T, TMock>>.Method(string methodName,
		IParameters parameters)
	{
		return new VerificationResult<IMockVerify<T, TMock>>(this, interactions,
			interactions.Interactions
				.OfType<MethodInvocation>()
				.Where(method =>
					method.Name.Equals(methodName) &&
					parameters.Matches(method.Parameters))
				.Cast<IInteraction>()
				.ToArray(),
			$"invoked method {methodName.SubstringAfterLast('.')}({parameters})");
	}

	/// <inheritdoc cref="IMockGot{TMock}.Property(string)" />
	VerificationResult<IMockVerify<T, TMock>> IMockGot<IMockVerify<T, TMock>>.Property(string propertyName)
	{
		return new VerificationResult<IMockVerify<T, TMock>>(this, interactions,
			interactions.Interactions
				.OfType<PropertyGetterAccess>()
				.Where(property => property.Name.Equals(propertyName))
				.Cast<IInteraction>()
				.ToArray(),
			$"got property {propertyName.SubstringAfterLast('.')}");
	}
	/// <inheritdoc cref="IMockSet{TMock}.Property(string, IParameter)" />
	VerificationResult<IMockVerify<T, TMock>> IMockSet<IMockVerify<T, TMock>>.Property(string propertyName,
		IParameter value)
	{
		return new VerificationResult<IMockVerify<T, TMock>>(this, interactions,
			interactions.Interactions
				.OfType<PropertySetterAccess>()
				.Where(property => property.Name.Equals(propertyName) && value.Matches(property.Value))
				.Cast<IInteraction>()
				.ToArray(),
			$"set property {propertyName.SubstringAfterLast('.')} to value {value}");
	}

	/// <inheritdoc cref="IMockGotIndexer{TMock}.Got(IParameter?[])" />
	VerificationResult<IMockVerify<T, TMock>> IMockGotIndexer<IMockVerify<T, TMock>>.Got(
		params IParameter?[] parameters)
	{
		return new VerificationResult<IMockVerify<T, TMock>>(this, interactions,
			interactions.Interactions
				.OfType<IndexerGetterAccess>()
				.Where(indexer => indexer.Parameters.Length == parameters.Length &&
								  !parameters.Where((parameter, i) => parameter is null
									  ? indexer.Parameters[i] is not null
									  : !parameter.Matches(indexer.Parameters[i])).Any())
				.Cast<IInteraction>()
				.ToArray(),
			$"got indexer {string.Join(", ", parameters.Select(x => x?.ToString() ?? "null"))}");
	}

	/// <inheritdoc cref="IMockSetIndexer{TMock}.Set(IParameter?, IParameter?[])" />
	VerificationResult<IMockVerify<T, TMock>> IMockSetIndexer<IMockVerify<T, TMock>>.Set(IParameter? value,
		params IParameter?[] parameters)
	{
		return new VerificationResult<IMockVerify<T, TMock>>(this, interactions,
			interactions.Interactions
				.OfType<IndexerSetterAccess>()
				.Where(indexer => indexer.Parameters.Length == parameters.Length &&
								  (value is null ? indexer.Value is null : value!.Matches(indexer.Value)) &&
								  !parameters.Where((parameter, i) => parameter is null
									  ? indexer.Parameters[i] is not null
									  : !parameter.Matches(indexer.Parameters[i])).Any())
				.Cast<IInteraction>()
				.ToArray(),
			$"set indexer {string.Join(", ", parameters.Select(x => x?.ToString() ?? "null"))} to value {value?.ToString() ?? "null"}");
	}
	/// <inheritdoc cref="IMockUnsubscribedFrom{TMock}.Event(string)" />
	VerificationResult<IMockVerify<T, TMock>> IMockUnsubscribedFrom<IMockVerify<T, TMock>>.Event(string eventName)
	{
		return new VerificationResult<IMockVerify<T, TMock>>(this, interactions,
			interactions.Interactions
				.OfType<EventUnsubscription>()
				.Where(@event => @event.Name.Equals(eventName))
				.Cast<IInteraction>()
				.ToArray(),
			$"unsubscribed from event {eventName.SubstringAfterLast('.')}");
	}
	/// <inheritdoc cref="IMockSubscribedTo{TMock}.Event(string)" />
	VerificationResult<IMockVerify<T, TMock>> IMockSubscribedTo<IMockVerify<T, TMock>>.Event(string eventName)
	{
		return new VerificationResult<IMockVerify<T, TMock>>(this, interactions,
			interactions.Interactions
				.OfType<EventSubscription>()
				.Where(@event => @event.Name.Equals(eventName))
				.Cast<IInteraction>()
				.ToArray(),
			$"subscribed to event {eventName.SubstringAfterLast('.')}");
	}

	/// <inheritdoc cref="IMockVerify{T,TMock}.ThatAllInteractionsAreVerified()" />
	public bool ThatAllInteractionsAreVerified() => !interactions.HasMissingVerifications;

	/// <inheritdoc cref="IMockVerifyInvokedWithToString{T,TMock}.ToString()" />
	VerificationResult<IMockVerify<T, TMock>> IMockVerifyInvokedWithToString<T, TMock>.ToString()
	{
		return ((IMockInvoked<IMockVerify<T, TMock>>)this).Method(prefix + ".ToString");
	}
	/// <inheritdoc cref="IMockVerifyInvokedWithEquals{T,TMock}.Equals(IParameter{object})" />
	VerificationResult<IMockVerify<T, TMock>> IMockVerifyInvokedWithEquals<T, TMock>.Equals(IParameter<object>? obj)
	{
		return ((IMockInvoked<IMockVerify<T, TMock>>)this).Method(prefix + ".Equals", obj ?? Parameter.Null<object>());
	}
	/// <inheritdoc cref="IMockVerifyInvokedWithGetHashCode{T,TMock}.GetHashCode()" />
	VerificationResult<IMockVerify<T, TMock>> IMockVerifyInvokedWithGetHashCode<T, TMock>.GetHashCode()
	{
		return ((IMockInvoked<IMockVerify<T, TMock>>)this).Method(prefix + ".GetHashCode");
	}
}
#pragma warning restore S1939 // Inheritance list should not be redundant
#pragma warning restore S2326 // Unused type parameters should be removed
