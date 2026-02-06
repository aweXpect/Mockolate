using System;
using System.Collections.Generic;
using System.Net.Http;

namespace Mockolate.Web;

#pragma warning disable S2325 // Methods and properties that don't access instance data should be static
public static partial class ItExtensions
{
	/// <inheritdoc cref="ItExtensions" />
	extension(It _)
	{
		/// <summary>
		///     Expects the <see cref="HttpContent" /> parameter to have a form data content.
		/// </summary>
		public static IFormDataContentParameter IsFormDataContent()
			=> new FormDataContentParameter();
	}

	/// <summary>
	///     Further expectations on the form-data <see cref="HttpContent" />.
	/// </summary>
	public interface IFormDataContentParameter : IHttpContentParameter<IFormDataContentParameter>
	{
		/// <summary>
		///     Expects the form data content to contain the given <paramref name="key" />-<paramref name="value" /> pair.
		/// </summary>
		IFormDataContentParameter Containing(string key, HttpFormDataValue value);

		/// <summary>
		///     Expects the form data content to contain the given <paramref name="values" />.
		/// </summary>
		IFormDataContentParameter Containing(params IEnumerable<(string Key, HttpFormDataValue Value)> values);

		/// <summary>
		///     Expects the form data content to contain the given <paramref name="values" />.
		/// </summary>
		IFormDataContentParameter Containing(string values);
	}

	private sealed class FormDataContentParameter : HttpContentParameter<IFormDataContentParameter>,
		IFormDataContentParameter
	{
		private FormDataMatcher? _formDataMatcher;

		/// <inheritdoc cref="HttpContentParameter{TParameter}.GetThis" />
		protected override IFormDataContentParameter GetThis => this;

		/// <inheritdoc cref="IFormDataContentParameter.Containing(string, HttpFormDataValue)" />
		public IFormDataContentParameter Containing(string key, HttpFormDataValue value)
		{
			_formDataMatcher = new FormDataMatcher(key, value);
			return this;
		}

		public IFormDataContentParameter Containing(params IEnumerable<(string Key, HttpFormDataValue Value)> values)
		{
			_formDataMatcher = new FormDataMatcher(values);
			return this;
		}

		/// <inheritdoc cref="IFormDataContentParameter.Containing(string)" />
		public IFormDataContentParameter Containing(string values)
		{
			_formDataMatcher = new FormDataMatcher(values);
			return this;
		}


		/// <inheritdoc cref="HttpContentParameter{TParameter}.Matches(HttpContent)" />
		protected override bool Matches(HttpContent value)
		{
			if (!base.Matches(value))
			{
				return false;
			}

			if (value is not MultipartFormDataContent &&
			    value.Headers.ContentType?.MediaType?.Equals("application/x-www-form-urlencoded",
				    StringComparison.OrdinalIgnoreCase) != true)
			{
				return false;
			}

			if (_formDataMatcher is not null &&
			    !_formDataMatcher.Matches(value))
			{
				return false;
			}

			return true;
		}
	}
}
#pragma warning restore S2325 // Methods and properties that don't access instance data should be static
