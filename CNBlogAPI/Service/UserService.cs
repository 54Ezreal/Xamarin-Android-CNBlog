using CNBlogAPI.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace CNBlogAPI.Service
{
    public class UserService
    {
        /// <summary>
        /// 登录加密用户名和密码的公钥
        /// </summary>
        public static string publicKey = "";

        private static string GetUserInfoUrl = @"https://api.cnblogs.com/api/Users";

        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="passWord"></param>
        /// <returns></returns>
		public async static Task<AccessToken> Login(string userName,string passWord)
        {
            var parameters = new Dictionary<string, string>();
            parameters.Add("grant_type", "password");
            parameters.Add("username", userName);
            parameters.Add("password", passWord);
            HttpClient _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("ContentType", "application/x-www-form-urlencoded");
            var clientId = "";
            var clientSecret = "";
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                    "Basic",
                    Convert.ToBase64String(Encoding.UTF8.GetBytes(clientId + ":" + clientSecret)));
            var response = await _httpClient.PostAsync("https://api.cnblogs.com/token", new FormUrlEncodedContent(parameters));
            var responseValue = await response.Content.ReadAsStringAsync();
            var token = JsonConvert.DeserializeObject< AccessToken>(responseValue);
			token.RefreshTime = DateTime.Now;
			return token;
		}

        /// <summary>
        /// 获取当前登录用户信息
        /// </summary>
        /// <returns></returns>
        public async static Task<UserInfo> GetCurrentLoginUserInfo(AccessToken token)
        {
            HttpClient _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("Authorization", token.token_type + " " + token.access_token);
            var response = await _httpClient.GetAsync(GetUserInfoUrl);
            string str = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<UserInfo>(str);
        }

    }
}
