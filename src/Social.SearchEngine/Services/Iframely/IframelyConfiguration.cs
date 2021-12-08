using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Social.SearchEngine.Services.Iframely
{
	public class IframelyConfiguration
	{
		public string ApiKey { get; set; }
		public string ApiKeyHash { get; set; }
		public string OEmbedUrl { get; set; }
		public string IframelyUrl { get; set; }
	}
}
