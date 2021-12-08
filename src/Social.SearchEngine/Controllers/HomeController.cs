using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Social.SearchEngine.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Social.SearchEngine.Services.Iframely;
using Social.SearchEngine.Services.Twitter;

namespace Social.SearchEngine.Controllers
{
	public class HomeController : Controller
	{
		private readonly TwitterService _twitterService;
		private readonly IframelyService _iframelyService;

		public HomeController(TwitterService twitterService, IframelyService iframelyService)
		{
			_twitterService = twitterService;
			_iframelyService = iframelyService;
		}

		[HttpGet]
		[HttpPost]
		public async Task<IActionResult> Index(string text)
		{
			if(String.IsNullOrWhiteSpace(text)) return View(new SearchViewModel());

			var tweets = await _twitterService.GetTweetsAsync(text);
			var oembeds = await Task.WhenAll(
				tweets.Select(url => _iframelyService.GetOEmbedAsync(url).ContinueWith(t => t.Result?.Html ?? String.Empty)).ToArray()
			);

			var model = new SearchViewModel
			{
				SearchText = text,
				Tweets = oembeds.ToList()
			};

			return View(model);
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}
