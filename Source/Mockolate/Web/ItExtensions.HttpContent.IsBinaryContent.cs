using System;
using System.Collections.Generic;
using System.Net.Http;
using Mockolate.Parameters;
#if !NET8_0_OR_GREATER
using System.Linq;
#endif

namespace Mockolate.Web;

#pragma warning disable S2325 // Methods and properties that don't access instance data should be static
public static partial class ItExtensions
{
	/// <inheritdoc cref="ItExtensions" />
	extension(It _)
	{
		/// <summary>
		///     Expects the <see cref="HttpContent" /> parameter to have a binary content.
		/// </summary>
		public static IBinaryContentParameter IsBinaryContent()
			=> new BinaryContentParameter(null);

		/// <summary>
		///     Expects the <see cref="HttpContent" /> parameter to have a binary content
		///     with the given <paramref name="mediaType" />.
		/// </summary>
		public static IBinaryContentParameter IsBinaryContent(string mediaType)
			=> new BinaryContentParameter(mediaType);
	}

	/// <summary>
	///     Further expectations on the binary <see cref="HttpContent" />.
	/// </summary>
	public interface IBinaryContentParameter : IParameter<HttpContent?>
	{
		/// <summary>
		///     Expects the binary content to have the given <paramref name="mediaType" />.
		/// </summary>
		IBinaryContentParameter WithMediaType(string? mediaType);

		/// <summary>
		///     Expects the binary content to be equal to the given <paramref name="bytes" />.
		/// </summary>
		IBinaryContentParameter EqualTo(byte[] bytes);

		/// <summary>
		///     Expects the binary content to contain the given <paramref name="bytes" />.
		/// </summary>
		IBinaryContentParameter Containing(byte[] bytes);
	}

	private sealed class BinaryContentParameter(string? mediaType)
		: IBinaryContentParameter, IParameter
	{
		private byte[]? _body;
		private BodyMatchType _bodyMatchType = BodyMatchType.Exact;
		private List<Action<HttpContent?>>? _callbacks;
		private string? _mediaType = mediaType;

		public IParameter<HttpContent?> Do(Action<HttpContent?> callback)
		{
			_callbacks ??= [];
			_callbacks.Add(callback);
			return this;
		}

		/// <inheritdoc cref="IBinaryContentParameter.WithMediaType(string?)" />
		public IBinaryContentParameter WithMediaType(string? mediaType)
		{
			_mediaType = mediaType;
			return this;
		}

		/// <inheritdoc cref="IBinaryContentParameter.EqualTo(byte[])" />
		public IBinaryContentParameter EqualTo(byte[] bytes)
		{
			_body = bytes;
			_bodyMatchType = BodyMatchType.Exact;
			return this;
		}

		/// <inheritdoc cref="IBinaryContentParameter.Containing(byte[])" />
		public IBinaryContentParameter Containing(byte[] bytes)
		{
			_body = bytes;
			_bodyMatchType = BodyMatchType.Contains;
			return this;
		}

		public bool Matches(object? value)
			=> value is HttpContent typedValue && Matches(typedValue);

		public void InvokeCallbacks(object? value)
		{
			if (value is HttpContent httpContent)
			{
				_callbacks?.ForEach(a => a.Invoke(httpContent));
			}
		}

		private bool Matches(HttpContent value)
		{
			if (_mediaType is not null &&
			    value.Headers.ContentType?.MediaType?.Equals(_mediaType, StringComparison.OrdinalIgnoreCase) != true)
			{
				return false;
			}

			if (_body is not null)
			{
				byte[] content = value.ReadAsByteArrayAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				switch (_bodyMatchType)
				{
					case BodyMatchType.Exact when
						!content.SequenceEqual(_body):
						return false;
					case BodyMatchType.Contains when
						!Contains(content, _body):
						return false;
				}
			}

			return true;
		}

		private static bool Contains(byte[] data, byte[] otherData)
		{
#if NET8_0_OR_GREATER
			return data.AsSpan().IndexOf(otherData) >= 0;
#else
			int dataLength = data.Length;
			int otherDataLength = otherData.Length;

			if (dataLength < otherDataLength)
			{
				return false;
			}

			for (int i = 0; i <= dataLength - otherDataLength; i++)
			{
				bool isMatch = true;
				for (int j = 0; j < otherDataLength; j++)
				{
					if (data[i + j] != otherData[j])
					{
						isMatch = false;
						break;
					}
				}

				if (isMatch)
				{
					return true;
				}
			}

			return false;
#endif
		}

		private enum BodyMatchType
		{
			Exact,
			Contains,
		}
	}
}
#pragma warning restore S2325 // Methods and properties that don't access instance data should be static
