using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Social.SearchEngine.Models
{
	public class SearchViewModel
	{
		public string SearchText { get; set; } = String.Empty;
		public List<string> Tweets { get; set; } = new();
	}
}
