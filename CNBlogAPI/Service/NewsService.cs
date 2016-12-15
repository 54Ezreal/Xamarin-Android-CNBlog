using CNBlogAPI.Model;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace CNBlogAPI.Service
{
	public class NewsService
	{
        /// <summary>
        /// 新闻列表请求地址
        /// </summary>
        private static string GetNewsUrl = @"https://api.cnblogs.com/api/NewsItems?pageIndex={0}&pageSize={1}";

        /// <summary>
        /// 新闻内容请求地址
        /// </summary>
		private static string GetNewsInfoUrl = @"https://api.cnblogs.com/api/newsitems/{0}/body";

		/// <summary>
		/// 发送新闻评论请求地址
		/// </summary>
		private static string AddNewsCommentsUrl = @"https://api.cnblogs.com/api/news/{0}/comments";

		/// <summary>
		/// 获取新闻评论请求地址
		/// </summary>
		private static string GetNewsCommentsUrl = @"https://api.cnblogs.com/api/news/{0}/comments?pageIndex={1}&pageSize={2}";


        /// <summary>
        /// 分页获取新闻列表
        /// </summary>
        /// <param name="token"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public async static Task<List<NewInfo>> GetNews(AccessToken token, int pageIndex, int pageSize)
        {
            HttpClient _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + token.access_token);
            var response = await _httpClient.GetAsync(string.Format(GetNewsUrl, pageIndex, pageSize));
            string str = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<NewInfo>>(str);
        }

        /// <summary>
        /// 获取新闻内容
        /// </summary>
        /// <param name="token"></param>
        /// <param name="newId"></param>
        /// <returns></returns>
        public async static Task<string> GetNewInfo(AccessToken token,int newId)
        {
            HttpClient _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + token.access_token);
            var response = await _httpClient.GetAsync(string.Format(GetNewsInfoUrl, newId));
            return await response.Content.ReadAsStringAsync();
        }

		/// <summary>
		/// 添加新闻评论
		/// </summary>
		/// <param name="token"></param>
		/// <param name="newsId"></param>
		/// <param name="content"></param>
		/// <returns></returns>
		public async static Task<bool> AddNewComments(AccessToken token, int newsId,string content)
		{
			HttpClient _httpClient = new HttpClient();
			_httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + token.access_token);
			var parameters = new Dictionary<string, string>();
			parameters.Add("Content", content);
			_httpClient.DefaultRequestHeaders.Add("ContentType", "application/x-www-form-urlencoded");
			var response = await _httpClient.PostAsync(string.Format(AddNewsCommentsUrl, newsId),new FormUrlEncodedContent(parameters));
			return response.StatusCode== System.Net.HttpStatusCode.OK;
		}

		/// <summary>
		/// 分页获取新闻评论
		/// </summary>
		/// <param name="token"></param>
		/// <param name="pageIndex"></param>
		/// <param name="pageSize"></param>
		/// <returns></returns>
		public async static Task<List<NewsComment>> GetNewsComments(AccessToken token,  int newsId,int pageIndex, int pageSize=25)
		{
			HttpClient _httpClient = new HttpClient();
			_httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + token.access_token);
			var response = await _httpClient.GetAsync(string.Format(GetNewsCommentsUrl,newsId , pageIndex, pageSize));
			string str = await response.Content.ReadAsStringAsync();
			return JsonConvert.DeserializeObject<List<NewsComment>>(str);
		}

    }
}
