using System.Linq;

namespace Mockolate.Web;

#pragma warning disable S2325 // Methods and properties that don't access instance data should be static
public static partial class ItExtensions
{
	/// <inheritdoc cref="IHttpContentParameter" />
	extension(IHttpContentParameter parameter)
	{
		/// <summary>
		///     Expects the binary content to be equal to the given <paramref name="bytes" />.
		/// </summary>
		public IHttpContentParameter WithBytes(byte[] bytes)
			=> parameter.WithBytes(bytes.SequenceEqual);
	}
}
#pragma warning restore S2325 // Methods and properties that don't access instance data should be static
