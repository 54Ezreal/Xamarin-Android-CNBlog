using CNBlogAPI.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace CNBlogAPI
{
    public class OAuthClient
    {
        private HttpClient _httpClient;

        public OAuthClient()
        {
            _httpClient = new HttpClient();
        }

		public async Task<AccessToken> GetAccessToken()
        {
            var clientId = "";
            var clientSecret = "";
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                    "Basic",
                    Convert.ToBase64String(Encoding.UTF8.GetBytes(clientId + ":" + clientSecret)));

            var parameters = new Dictionary<string, string>();
            parameters.Add("grant_type", "client_credentials");

            var str =await _httpClient.PostAsync("https://api.cnblogs.com/token", new FormUrlEncodedContent(parameters))
                .Result.Content.ReadAsStringAsync();

            var token = JsonConvert.DeserializeObject<AccessToken>(str);
			token.RefreshTime = DateTime.Now;
			return token;
		}
        
    }
}
