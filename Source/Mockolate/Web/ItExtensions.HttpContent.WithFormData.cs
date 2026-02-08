using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
#if NETSTANDARD2_0
using Mockolate.Internals.Polyfills;
#endif

namespace Mockolate.Web;

#pragma warning disable S2325 // Methods and properties that don't access instance data should be static
public static partial class ItExtensions
{
	private static IEnumerable<(string Key, string Value)> ParseFormDataParameters(string input)
		=> input.TrimStart('?')
			.Split('&')
			.Select(pair => pair.Split('=', 2))
			.Where(pair => !string.IsNullOrWhiteSpace(pair[0]))
			.Select(pair =>
				(
					WebUtility.UrlDecode(pair[0]),
					pair.Length == 2 ? WebUtility.UrlDecode(pair[1]) : ""
				)
			);

	/// <inheritdoc cref="IHttpContentParameter" />
	extension(IHttpContentParameter parameter)
	{
		/// <summary>
		///     Expects the form data content to contain the given <paramref name="key" />-<paramref name="value" /> pair.
		/// </summary>
		public IFormDataContentParameter WithFormData(string key, HttpFormDataValue value)
			=> parameter.WithFormData([(key, value)]);

		/// <summary>
		///     Expects the form data content to contain the given <paramref name="values" />.
		/// </summary>
		public IFormDataContentParameter WithFormData(params IEnumerable<(string Key, HttpFormDataValue Value)> values)
		{
			FormDataMatcher data = new(values);
			FormDataContentParameter contentParameter = new(parameter, data);
			return contentParameter;
		}

		/// <summary>
		///     Expects the form data content to contain the given <paramref name="values" />.
		/// </summary>
		public IFormDataContentParameter WithFormData(string values)
			=> parameter.WithFormData(ParseFormDataParameters(values)
				.Select(pair => (pair.Key, new HttpFormDataValue(pair.Value))));
	}

	/// <summary>
	///     Further expectations on the form-data <see cref="HttpContent" />.
	/// </summary>
	public interface IFormDataContentParameter : IHttpContentParameter
	{
		/// <summary>
		///     Expects the form data content to not contain any additional key-value pairs other than the ones already specified.
		/// </summary>
		IFormDataContentParameter Exactly();
	}

	private sealed class FormDataMatcher
	{
		private readonly List<(string Name, HttpFormDataValue Value)> _requiredFormDataParameters = [];
		private bool _isExactly;

		public FormDataMatcher(IEnumerable<(string Name, HttpFormDataValue Value)> formDataParameters)
		{
			_requiredFormDataParameters.AddRange(formDataParameters);
		}

		public bool Matches(string content)
		{
			List<(string Key, string Value)> formDataParameters = GetFormData(content).ToList();
			return _isExactly
				? _requiredFormDataParameters.All(requiredParameter
					  => formDataParameters.Any(parameter
						  => parameter.Key == requiredParameter.Name &&
						     requiredParameter.Value.Matches(parameter.Value))) &&
				  formDataParameters.All(parameter => _requiredFormDataParameters
					  .Any(requiredParameter => parameter.Key == requiredParameter.Name &&
					                            requiredParameter.Value.Matches(parameter.Value)))
				: _requiredFormDataParameters.All(requiredParameter
					=> formDataParameters.Any(parameter
						=> parameter.Key == requiredParameter.Name &&
						   requiredParameter.Value.Matches(parameter.Value)));
		}

		public void Exactly()
			=> _isExactly = true;

		private IEnumerable<(string, string)> GetFormData(string rawContent)
		{
			string rawFormData = ExtractFormDataFromMultipartContent(rawContent);
			return ParseFormDataParameters(rawFormData);
		}

		private string ExtractFormDataFromMultipartContent(string rawContent)
		{
			string[] lines = rawContent.Split('\n');
			StringBuilder sb = new();
			foreach (string line in lines)
			{
				if (line.StartsWith("--") ||
				    line.StartsWith("Content-Type: ") ||
				    line.StartsWith("Content-Disposition: "))
				{
					continue;
				}

				if (!string.IsNullOrWhiteSpace(line))
				{
					sb.AppendLine(line.TrimEnd());
				}
			}

			return sb.ToString().Trim();
		}
	}

	private sealed class FormDataContentParameter : HttpContentParameterWrapper, IFormDataContentParameter
	{
		private readonly FormDataMatcher _data;

		public FormDataContentParameter(IHttpContentParameter parameter, FormDataMatcher data) : base(parameter)
		{
			_data = data;
			parameter.WithString(data.Matches);
		}

		public IFormDataContentParameter Exactly()
		{
			_data.Exactly();
			return this;
		}
	}
}
#pragma warning restore S2325 // Methods and properties that don't access instance data should be static
