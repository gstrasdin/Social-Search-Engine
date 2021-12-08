using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Extensions.Logging;

namespace Social.SearchEngine.Services.Iframely
{
	public class IframelyService
	{
		private readonly HttpClient _client;
		private readonly IframelyConfiguration _configuration;

		public IframelyService(HttpClient client, IframelyConfiguration configuration)
		{
			_client = client;
			_configuration = configuration;
		}

		public async Task<IframelyOEmbedResponse?> GetOEmbedAsync(Uri postUrl)
		{
			Console.WriteLine($"Getting oEmbed response for {postUrl}");

			try
			{
				var url = String.Format(_configuration.OEmbedUrl, new object[] { HttpUtility.UrlEncode(postUrl.ToString()), _configuration.ApiKey });
				var response = await _client.GetAsync(url).ConfigureAwait(false);
				var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
				var oEmbed = JsonSerializer.Deserialize<IframelyOEmbedResponse>(json);

				return oEmbed;
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				return null;
			}
		}
	}
}
