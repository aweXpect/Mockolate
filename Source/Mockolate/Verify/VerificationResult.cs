using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mockolate.Exceptions;
using Mockolate.Interactions;
using Mockolate.Internals;

namespace Mockolate.Verify;

/// <summary>
///     The result of a verification containing the matching interactions.
/// </summary>
public class VerificationResult<TVerify> : IVerificationResult<TVerify>, IVerificationResult
{
	private readonly string _expectation;
	private readonly MockInteractions _interactions;
	private readonly Func<IInteraction, bool> _predicate;
	private readonly TVerify _verify;

	/// <inheritdoc cref="VerificationResult{TMock}" />
	public VerificationResult(TVerify verify,
		MockInteractions interactions,
		Func<IInteraction, bool> predicate,
		string expectation)
	{
		_verify = verify;
		_interactions = interactions;
		_predicate = predicate;
		_expectation = expectation;
	}

	#region IVerificationResult<TVerify>

	/// <inheritdoc cref="IVerificationResult{TVerify}.Object" />
	TVerify IVerificationResult<TVerify>.Object
		=> _verify;

	#endregion

	internal VerificationResult<T> Map<T>(T mock)
		=> new(mock, _interactions, _predicate, _expectation);

	/// <summary>
	///     Makes the verification result awaitable, using the specified <paramref name="timeout" /> to wait for the expected
	///     interactions to occur.
	/// </summary>
	public virtual VerificationResult<TVerify> Within(TimeSpan timeout)
		=> new Awaitable(this, timeout);

	/// <summary>
	///     Makes the verification result awaitable, using the specified <paramref name="cancellationToken" /> to wait for the
	///     expected interactions to occur.
	/// </summary>
	public virtual VerificationResult<TVerify> WithCancellation(CancellationToken cancellationToken)
		=> new Awaitable(this, cancellationToken);

	/// <summary>
	///     An awaitable <see cref="VerificationResult{TVerify}" /> that uses the timeout or cancellation token to wait for the
	///     expected interactions to occur.
	/// </summary>
	internal class Awaitable : VerificationResult<TVerify>, IAsyncVerificationResult
	{
		private CancellationToken? _cancellationToken;
		private TimeSpan? _timeout;

		/// <summary>
		///     An awaitable <see cref="VerificationResult{TVerify}" /> that uses the <paramref name="timeout" /> to wait for the
		///     expected interactions to occur.
		/// </summary>
		public Awaitable(VerificationResult<TVerify> inner, TimeSpan timeout) : base(inner._verify, inner._interactions,
			inner._predicate, inner._expectation)
		{
			_timeout = timeout;
		}

		/// <summary>
		///     An awaitable <see cref="VerificationResult{TVerify}" /> that uses the <paramref name="cancellationToken" /> to wait
		///     for the
		///     expected interactions to occur.
		/// </summary>
		public Awaitable(VerificationResult<TVerify> inner, CancellationToken cancellationToken) : base(inner._verify,
			inner._interactions, inner._predicate, inner._expectation)
		{
			_cancellationToken = cancellationToken;
		}

		/// <inheritdoc cref="IVerificationResult.Verify(Func{IInteraction[], Boolean})" />
		bool IVerificationResult.Verify(Func<IInteraction[], bool> predicate)
		{
			IInteraction[] matchingInteractions = _interactions.Interactions.Where(_predicate).ToArray();
			_interactions.Verified(matchingInteractions);
			bool result = predicate(matchingInteractions);
			if (result)
			{
				return true;
			}

			return VerifyAsync(predicate).GetAwaiter().GetResult();
		}

		/// <inheritdoc cref="IAsyncVerificationResult.VerifyAsync(Func{IInteraction[], Boolean})" />
		public async Task<bool> VerifyAsync(Func<IInteraction[], bool> predicate)
		{
			IInteraction[] matchingInteractions = _interactions.Interactions.Where(_predicate).ToArray();
			_interactions.Verified(matchingInteractions);
			bool result = predicate(matchingInteractions);
			if (result)
			{
				return true;
			}

			try
			{
				CancellationTokenSource? cts = null;
				CancellationToken token;
				if (_timeout is null)
				{
					token = _cancellationToken!.Value;
				}
				else
				{
					if (_cancellationToken is not null)
					{
						cts = CancellationTokenSource.CreateLinkedTokenSource(_cancellationToken.Value);
					}
					else
					{
						cts = new CancellationTokenSource();
					}

					cts.CancelAfter(_timeout.Value);
					token = cts.Token;
				}

				TaskCompletionSource<bool> tcs = new();
				try
				{
					_interactions.InteractionAdded += OnInteractionAdded;
					do
					{
						await tcs.WaitAsync(token);

						matchingInteractions = _interactions.Interactions.Where(_predicate).ToArray();
						_interactions.Verified(matchingInteractions);
						if (predicate(matchingInteractions))
						{
							return true;
						}

						tcs = new TaskCompletionSource<bool>();
					} while (!token.IsCancellationRequested);

					return false;
				}
				finally
				{
					_interactions.InteractionAdded -= OnInteractionAdded;
					cts?.Dispose();
				}

				void OnInteractionAdded(object? sender, EventArgs eventArgs)
				{
					tcs.SetResult(true);
				}
			}
			catch (OperationCanceledException ex)
			{
				throw new MockVerificationTimeoutException(_timeout, ex);
			}
		}

		/// <inheritdoc cref="VerificationResult{TVerify}.Within(TimeSpan)" />
		public override VerificationResult<TVerify> Within(TimeSpan timeout)
		{
			_timeout = timeout;
			return this;
		}

		/// <inheritdoc cref="VerificationResult{TVerify}.WithCancellation(CancellationToken)" />
		public override VerificationResult<TVerify> WithCancellation(CancellationToken cancellationToken)
		{
			_cancellationToken = cancellationToken;
			return this;
		}
	}

	#region IVerificationResult

	/// <inheritdoc cref="IVerificationResult.Expectation" />
	string IVerificationResult.Expectation
		=> _expectation;

	/// <inheritdoc cref="IVerificationResult.MockInteractions" />
	MockInteractions IVerificationResult.MockInteractions
		=> _interactions;

	/// <inheritdoc cref="IVerificationResult.Verify(Func{IInteraction[], Boolean})" />
	bool IVerificationResult.Verify(Func<IInteraction[], bool> predicate)
	{
		IInteraction[] matchingInteractions = _interactions.Interactions.Where(_predicate).ToArray();
		_interactions.Verified(matchingInteractions);
		return predicate(matchingInteractions);
	}

	#endregion
}
