using System.Linq;
using Mockolate.Interactions;
using Mockolate.Internals;
using Mockolate.Setup;
using Mockolate.Verify;

namespace Mockolate.V2;

public partial class Mock<T> : IMockVerify<T>,
	IMockInvoked<IMockVerify<T>>,
	IMockVerifyInvoked<T>, IMockVerifyInvokedProtected<T>,
	IMockVerifyInvokedWithToStringWithEqualsWithGetHashCode<T>, IMockVerifyInvokedWithEqualsWithGetHashCode<T>, IMockVerifyInvokedWithToStringWithGetHashCode<T>, IMockVerifyInvokedWithToStringWithEquals<T>,
	IMockGot<IMockVerify<T>>,
	IMockVerifyGot<T>, IMockVerifyGotProtected<T>,
	IMockSet<IMockVerify<T>>,
	IMockVerifySet<T>, IMockVerifySetProtected<T>,
	IMockGotIndexer<IMockVerify<T>>,
	IMockVerifyGotIndexer<T>, IMockVerifyGotIndexerProtected<T>,
	IMockSetIndexer<IMockVerify<T>>,
	IMockVerifySetIndexer<T>, IMockVerifySetIndexerProtected<T>,
	IMockSubscribedTo<IMockVerify<T>>,
	IMockVerifySubscribedTo<T>, IMockVerifySubscribedToProtected<T>,
	IMockUnsubscribedFrom<IMockVerify<T>>,
	IMockVerifyUnsubscribedFrom<T>, IMockVerifyUnsubscribedFromProtected<T>
{
	/// <inheritdoc cref="IMockVerify{T,TMock}.ThatAllInteractionsAreVerified()" />
	public bool ThatAllInteractionsAreVerified() => !Interactions.HasMissingVerifications;

	/// <inheritdoc cref="IMockInvoked{TMock}.Method(string, Match.IParameter[])" />
	public VerificationResult<IMockVerify<T>> Method(string methodName, params Match.IParameter[] parameters) => new(this,
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

	/// <inheritdoc cref="IMockInvoked{TMock}.Method(string, Match.IParameters)" />
	public VerificationResult<IMockVerify<T>> Method(string methodName, Match.IParameters parameters) => new(this, Interactions,
		Interactions.Interactions
			.OfType<MethodInvocation>()
			.Where(method =>
				method.Name.Equals(methodName) &&
				parameters.Matches(method.Parameters))
			.Cast<IInteraction>()
			.ToArray(),
		$"invoked method {methodName.SubstringAfterLast('.')}({parameters})");

	/// <inheritdoc cref="IMockGot{TMock}.Property(string)" />
	public VerificationResult<IMockVerify<T>> Property(string propertyName) => new(this, Interactions,
		Interactions.Interactions
			.OfType<PropertyGetterAccess>()
			.Where(property => property.Name.Equals(propertyName))
			.Cast<IInteraction>()
			.ToArray(),
		$"got property {propertyName.SubstringAfterLast('.')}");

	/// <inheritdoc cref="IMockSet{TMock}.Property(string, Match.IParameter)" />
	public VerificationResult<IMockVerify<T>> Property(string propertyName,
		Match.IParameter value)
		=> new(this, Interactions,
			Interactions.Interactions
				.OfType<PropertySetterAccess>()
				.Where(property => property.Name.Equals(propertyName) && value.Matches(property.Value))
				.Cast<IInteraction>()
				.ToArray(),
			$"set property {propertyName.SubstringAfterLast('.')} to value {value}");

	/// <inheritdoc cref="IMockGotIndexer{TMock}.Got(Match.IParameter?[])" />
	public VerificationResult<IMockVerify<T>> Got(
		params Match.IParameter?[] parameters)
		=> new(this, Interactions,
			Interactions.Interactions
				.OfType<IndexerGetterAccess>()
				.Where(indexer => indexer.Parameters.Length == parameters.Length &&
				                  !parameters.Where((parameter, i) => parameter is null
					                  ? indexer.Parameters[i] is not null
					                  : !parameter.Matches(indexer.Parameters[i])).Any())
				.Cast<IInteraction>()
				.ToArray(),
			$"got indexer {string.Join(", ", parameters.Select(x => x?.ToString() ?? "null"))}");

	/// <inheritdoc cref="IMockSetIndexer{TMock}.Set(Match.IParameter?, Match.IParameter?[])" />
	public VerificationResult<IMockVerify<T>> Set(Match.IParameter? value,
		params Match.IParameter?[] parameters)
		=> new(this, Interactions,
			Interactions.Interactions
				.OfType<IndexerSetterAccess>()
				.Where(indexer => indexer.Parameters.Length == parameters.Length &&
				                  (value is null ? indexer.Value is null : value!.Matches(indexer.Value)) &&
				                  !parameters.Where((parameter, i) => parameter is null
					                  ? indexer.Parameters[i] is not null
					                  : !parameter.Matches(indexer.Parameters[i])).Any())
				.Cast<IInteraction>()
				.ToArray(),
			$"set indexer {string.Join(", ", parameters.Select(x => x?.ToString() ?? "null"))} to value {value?.ToString() ?? "null"}");

	VerificationResult<IMockVerify<T>> IMockVerifyInvokedWithEquals<T>.Equals(Match.IParameter<object>? obj) => throw new System.NotImplementedException();

	VerificationResult<IMockVerify<T>> IMockSubscribedTo<IMockVerify<T>>.Event(string eventName) => throw new System.NotImplementedException();

	VerificationResult<IMockVerify<T>> IMockUnsubscribedFrom<IMockVerify<T>>.Event(string eventName) => throw new System.NotImplementedException();

	VerificationResult<IMockVerify<T>> IMockVerifyInvokedWithGetHashCode<T>.GetHashCode() => throw new System.NotImplementedException();

	ReturnMethodSetup<int> IMockMethodSetupWithGetHashCode<T>.GetHashCode() => throw new System.NotImplementedException();

	VerificationResult<IMockVerify<T>> IMockVerifyInvokedWithToString<T>.ToString() => throw new System.NotImplementedException();
}
