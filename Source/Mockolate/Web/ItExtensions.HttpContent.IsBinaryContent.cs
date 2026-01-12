using System.Net.Http;
#if !NET8_0_OR_GREATER
using System.Linq;
#else
using System;
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
			=> new BinaryContentParameter();

		/// <summary>
		///     Expects the <see cref="HttpContent" /> parameter to have a binary content
		///     with the given <paramref name="mediaType" />.
		/// </summary>
		public static IBinaryContentParameter IsBinaryContent(string mediaType)
			=> new BinaryContentParameter().WithMediaType(mediaType);
	}

	/// <summary>
	///     Further expectations on the binary <see cref="HttpContent" />.
	/// </summary>
	public interface IBinaryContentParameter : IHttpContentParameter<IBinaryContentParameter>
	{
		/// <summary>
		///     Expects the binary content to be equal to the given <paramref name="bytes" />.
		/// </summary>
		IBinaryContentParameter EqualTo(byte[] bytes);

		/// <summary>
		///     Expects the binary content to contain the given <paramref name="bytes" />.
		/// </summary>
		IBinaryContentParameter Containing(byte[] bytes);
	}

	private sealed class BinaryContentParameter : HttpContentParameter<IBinaryContentParameter>, IBinaryContentParameter
	{
		private byte[]? _body;
		private BodyMatchType _bodyMatchType = BodyMatchType.Exact;

		/// <inheritdoc cref="HttpContentParameter{TParameter}.GetThis" />
		protected override IBinaryContentParameter GetThis => this;

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

		/// <inheritdoc cref="HttpContentParameter{TParameter}.Matches(HttpContent)" />
		protected override bool Matches(HttpContent value)
		{
			if (!base.Matches(value))
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
