using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Mockolate.Exceptions;
using Mockolate.Parameters;
#if NETSTANDARD2_0
using Mockolate.Internals.Polyfills;
#endif

namespace Mockolate.Web;

/// <summary>
///     Extensions for mocking <see cref="HttpClient" />.
/// </summary>
public static partial class HttpClientExtensions
{
	private static MockRegistration CastToMockRegistrationOrThrow<T>(IInteractiveMock<T> subject)
	{
		if (subject is IHasMockRegistration mock)
		{
			return mock.Registrations;
		}

		throw new MockException("The subject is no mock.");
	}

	private static Mock<T> CastToMockOrThrow<T>(IInteractiveMock<T> subject)
	{
		if (subject is Mock<T> mock)
		{
			return mock;
		}

		throw new MockException("The subject is no mock.");
	}

	private sealed class HttpRequestMessageParameters(
		HttpMethod method,
		params IHttpRequestMessageParameter[] parameters)
		: IParameter<HttpRequestMessage>, IParameter
	{
		private List<Action<HttpRequestMessage>>? _callbacks;

		public bool Matches(object? value)
		{
			if (value is HttpRequestMessage requestMessage &&
			    requestMessage.Method == method)
			{
				return parameters.All(parameter => parameter.Matches(requestMessage));
			}

			return false;
		}

		public void InvokeCallbacks(object? value)
		{
			if (TryCast(value, out HttpRequestMessage typedValue))
			{
				_callbacks?.ForEach(a => a.Invoke(typedValue));
				foreach (IHttpRequestMessageParameter parameter in parameters)
				{
					parameter.InvokeCallbacks(typedValue);
				}
			}
		}

		public IParameter<HttpRequestMessage> Do(Action<HttpRequestMessage> callback)
		{
			_callbacks ??= [];
			_callbacks.Add(callback);
			return this;
		}

		private static bool TryCast<T>(object? value, out T typedValue)
		{
			if (value is T castValue)
			{
				typedValue = castValue;
				return true;
			}

			if (value is null && default(T) is null)
			{
				typedValue = default!;
				return true;
			}

			typedValue = default!;
			return false;
		}
	}


	private interface IHttpRequestMessageParameter
	{
		bool Matches(HttpRequestMessage value);

		void InvokeCallbacks(HttpRequestMessage value);
	}

	private sealed class HttpRequestMessageParameter<T>(
		Func<HttpRequestMessage, T> valueSelector,
		IParameter<T>? parameter)
		: IHttpRequestMessageParameter
	{
		public bool Matches(HttpRequestMessage value)
			=> parameter is null ||
			   ((IParameter)parameter).Matches(valueSelector(value));

		public void InvokeCallbacks(HttpRequestMessage value)
		{
			if (parameter is IParameter invokableParameter)
			{
				invokableParameter.InvokeCallbacks(valueSelector(value));
			}
		}
	}

	private sealed class HttpStringUriParameter(IParameter<string?>? parameter)
		: IHttpRequestMessageParameter
	{
		public bool Matches(HttpRequestMessage value)
		{
			if (parameter is not IParameter invokableParameter)
			{
				return true;
			}

			if (value.RequestUri is null)
			{
				return false;
			}

			string requestUri1 = value.RequestUri.ToString();
			string requestUri2 = requestUri1.EndsWith('/')
				? requestUri1.Substring(0, requestUri1.Length - 1)
				: requestUri1 + '/';

			return invokableParameter.Matches(requestUri1) ||
			       invokableParameter.Matches(requestUri2);
		}

		public void InvokeCallbacks(HttpRequestMessage value)
		{
			if (parameter is IParameter invokableParameter)
			{
				invokableParameter.InvokeCallbacks(value.RequestUri?.ToString());
			}
		}
	}
}
