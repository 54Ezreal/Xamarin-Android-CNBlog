using CNBlogAPI.Model;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace CNBlogAPI.Service
{
    public class BookMarkService
    {
        /// <summary>
        /// 检查收藏是否已存在请求地址
        /// </summary>
        private static string CheckExistsBookMarkUrl = @"https://api.cnblogs.com/api/Bookmarks?url={0}";

        /// <summary>
        /// 添加收藏请求地址
        /// </summary>
        private static string AddBookMarkUrl = @"https://api.cnblogs.com/api/Bookmarks";

        /// <summary>
        /// 删除收藏请求地址
        /// </summary>
        private static string DeleteBookMarkUrl = @"https://api.cnblogs.com/api/Bookmarks?url={0}";

        /// <summary>
		/// 获取收藏文章列表请求地址
		/// </summary>
		private static string BookmarksUrl = @"https://api.cnblogs.com/api/Bookmarks?pageIndex={0}&pageSize={1}";

        /// <summary>
        /// 分页获取收藏列表
        /// </summary>
        /// <returns>The book marks.</returns>
        /// <param name="token">Token.</param>
        /// <param name="pageIndex">Page index.</param>
        /// <param name="pageSize">Page size.</param>
        public async static Task<List<BookMark>> GetBookMarks(AccessToken token, int pageIndex, int pageSize)
        {
            HttpClient _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + token.access_token);
            var response = await _httpClient.GetAsync(string.Format(BookmarksUrl, pageIndex, pageSize));
            string str = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<BookMark>>(str);
        }

        /// <summary>
        /// 添加收藏
        /// </summary>
        /// <param name="token"></param>
        /// <param name="title"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        public async static Task<bool> AddBookMark(AccessToken token,string title ,string url)
        {
            HttpClient _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + token.access_token);
            var parameters = new Dictionary<string, string>();
            parameters.Add("title", title);
            parameters.Add("linkUrl", url);
            _httpClient.DefaultRequestHeaders.Add("ContentType", "application/x-www-form-urlencoded");
            var response = await _httpClient.PostAsync(AddBookMarkUrl, new FormUrlEncodedContent(parameters));
            return response.StatusCode== System.Net.HttpStatusCode.Created;
        }

        /// <summary>
        /// 检查是否已存在该收藏Url
        /// </summary>
        /// <param name="token"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        public async static Task<bool> CheckExistsBookMark(AccessToken token,string url)
        {
            HttpClient _httpClient = new HttpClient(); 
            _httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + token.access_token);
            var response = await _httpClient.SendAsync(new HttpRequestMessage(new HttpMethod("HEAD"), (string.Format(CheckExistsBookMarkUrl, url))));
            return response.StatusCode==System.Net.HttpStatusCode.OK;
        }

        /// <summary>
        /// 删除收藏,url需要编码
        /// </summary>
        /// <param name="token"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        public async static Task<bool> DeleteBookMark(AccessToken token, string url)
        {
            HttpClient _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + token.access_token);
            var response = await _httpClient.DeleteAsync(string.Format(DeleteBookMarkUrl,url));
            return response.StatusCode== System.Net.HttpStatusCode.OK;
        }

    }
}
