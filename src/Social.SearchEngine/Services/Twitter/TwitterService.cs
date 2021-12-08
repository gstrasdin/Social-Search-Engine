using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Policy;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Social.SearchEngine.Services.Twitter
{
    public class TwitterService
    {
        private static readonly Regex _usernameValidationExpression = new("^[A-Za-z0-9_]{1,15}$", RegexOptions.Compiled);
        private readonly HttpClient _client;
        private readonly TwitterConfiguration _configuration;

        public TwitterService(HttpClient client, TwitterConfiguration configuration)
        {
            _client = client;
            _configuration = configuration;
        }

        public async Task<List<Uri>> GetTweetsAsync(string username, CancellationToken token = default)
        {
	        try
	        {
		        var user = await GetUserByUsernameAsync(username, token);
		        if (user == null) return new List<Uri>();

		        var tweets = await GetTweetsByUserId(user!.Id, maxCount: 14, token: token);
		        if (tweets == null) return new List<Uri>();

		        var urls = tweets.Select(t => new Uri($"http://twitter.com/{user.Username}/status/{t.Id}")).ToList();
		        return urls;
	        }
	        catch (Exception e)
	        {
		        Console.WriteLine(e);
	        }

	        return new List<Uri>();
        }

        public Task<UserByUsernameData?> GetUserByUsernameAsync(string username, CancellationToken token = default)
        {
            Console.WriteLine($"Getting Twitter user with username {username}.");
            if (!_usernameValidationExpression.IsMatch(username))
            {
                throw new ArgumentException($"Cannot get user from Twitter V2 API. Parameter value: \"{username}\" must match expression \"{_usernameValidationExpression}\"", nameof(username));
            }

            var url = $"{_configuration.BaseUrl}/users/by/username/{username}?user.fields=created_at,description,id,location,name,pinned_tweet_id,profile_image_url,protected,public_metrics,url,username,verified";
            return GetDataAsync<UserByUsernameData>(url, token);
        }

        public Task<List<TweetData>?> GetTweetsByUserId(string id, DateTime? startDate = default, DateTime? endDate = default, int maxCount = 50, CancellationToken token = default)      // TODO: Make this IAsyncEnumerable
        {
	        Console.WriteLine($"Getting Tweets for user with id {id}.");

            if (maxCount < 1) throw new ArgumentOutOfRangeException(nameof(maxCount), maxCount, "At least one tweet must be requested.");
            if (maxCount > 100) throw new ArgumentOutOfRangeException(nameof(maxCount), maxCount, "The maximum number of tweets that can be retrieved in a single request is 100.");

            var url = new StringBuilder($"{_configuration.BaseUrl}/users/{id}/tweets?");
            if (startDate != default) url.Append($"start_time={startDate:O}&");
            if (endDate != default) url.Append($"end_time={endDate:O}&");
            url.Append("tweet.fields=id,text&");
            url.Append($"max_results={maxCount}");

            return GetDataAsync<List<TweetData>>(url.ToString(), token);
        }

        private async Task<TData?> GetDataAsync<TData>(string url, CancellationToken token = default)
        {
            // Create token that will be cancellable either by the configured timeout or invoking code
            var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30)).Token;
            var requestToken = CancellationTokenSource.CreateLinkedTokenSource(timeout, token).Token;

            var request = new HttpRequestMessage(HttpMethod.Get, url)
            {
                Headers =
                {
                    Authorization = new AuthenticationHeaderValue("Bearer", _configuration.BearerToken)
                }
            };

            // Get response string whether it is the expected or error response
            var response = await _client.SendAsync(request, HttpCompletionOption.ResponseContentRead, requestToken).ConfigureAwait(false);
            var content = await response.Content.ReadAsStringAsync(token).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
	            try
                {
	                var twitterResponse = JsonSerializer.Deserialize<TwitterV2Response<TData>>(content)!;
	                return twitterResponse.Data;
                }
                catch (Exception e)
                {
                    throw new ApplicationException("An unexpected error occurred while deserializing a response from the Twitter V2 API.", e);
                }
            }

            ErrorResponse? error = null;
            try
            {
                error = JsonSerializer.Deserialize<ErrorResponse>(content);
            }
            catch
            {
            }

            if (error != null) throw new TwitterErrorException(error!, response.StatusCode);
            response.EnsureSuccessStatusCode();
            throw new Exception("How did you get here?");
        }
    }
}
