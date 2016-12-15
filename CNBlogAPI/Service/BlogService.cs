using CNBlogAPI.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Globalization;
using System.Net;
using System.Xml.Linq;
using CNBlogAPI.ModelHelper;

namespace CNBlogAPI.Service
{
    public class BlogService
    {
        /// <summary>
        /// 首页文章请求地址
        /// </summary>
        private static string SiteHomeArticleUrl =@"https://api.cnblogs.com/api/blogposts/@sitehome?pageIndex={0}&pageSize={1}";
        
        /// <summary>
        /// 精华文章请求地址
        /// </summary>
        private static string PickedArticleUrl = @"https://api.cnblogs.com/api/blogposts/@picked?pageIndex={0}&pageSize={1}";

        /// <summary>
        /// 文章评论请求地址
        /// </summary>
        private static string ArticleCommentsUrl = @"https://api.cnblogs.com/api/blogs/{0}/posts/{1}/comments?pageIndex={2}&pageSize={3}";

        /// <summary>
        /// 添加文章评论请求地址
        /// </summary>
        private static string AddArticleCommentsUrl = @"https://api.cnblogs.com/api/blogs/{0}/posts/{1}/comments";

        /// <summary>
        /// 获取文章内容请求地址
        /// </summary>
		private static string GetArticleContentUrl = @"https://api.cnblogs.com/api/blogposts/{0}/body";
        ///http://wcf.open.cnblogs.com/blog/post/body/{0}

        /// <summary>
        /// 搜索博主请求地址
        /// </summary>
        private static string SearchBloggerUrl = @"http://wcf.open.cnblogs.com/blog/bloggers/search?t={0}";

		/// <summary>
		/// 根据博主名获取文章请求地址
		/// </summary>
		private static string BloggerPostsUrl = @"https://api.cnblogs.com/api/blogs/{0}/posts?pageIndex={1}";
        
		/// <summary>
		/// 分页获取知识库文章请求地址
		/// </summary>
		private static string KBArticlesUrl = @"https://api.cnblogs.com/api/KbArticles?pageIndex={0}&pageSize={1}";

		/// <summary>
		/// 获取知识库文章详情请求地址
		/// </summary>
		private static string KBArticleBodyUrl = @"https://api.cnblogs.com/api/kbarticles/{0}/body";

        /// <summary>
        /// 分页获取首页文章
        /// </summary>
        /// <param name="token"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public async static Task<List<Article>> GetHomeArticles(AccessToken token, int pageIndex ,int pageSize)
        {
				HttpClient _httpClient = new HttpClient();
				_httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + token.access_token);
				var response = await _httpClient.GetAsync(new Uri(string.Format(SiteHomeArticleUrl, pageIndex, pageSize)));
				string str = await response.Content.ReadAsStringAsync();
				return JsonConvert.DeserializeObject<List<Article>>(str);
        }

        /// <summary>
        /// 分页获取精华文章
        /// </summary>
        /// <param name="token"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public async static Task<List<Article>> GetPickedArticles(AccessToken token, int pageIndex, int pageSize)
        {
            HttpClient _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + token.access_token);
            var response = await _httpClient.GetAsync(string.Format(PickedArticleUrl, pageIndex, pageSize));
            string str = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<Article>>(str);
        }

        /// <summary>
        /// 分页获取博客评论
        /// </summary>
        /// <param name="token"></param>
        /// <param name="blogAPP"></param>
        /// <param name="postId"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public async static Task<List<ArticleComment>> GetArticleComments(AccessToken token,string blogApp,int postId,int pageIndex,int pageSize)
        {
            HttpClient _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + token.access_token);
            var response = await _httpClient.GetAsync(string.Format(ArticleCommentsUrl, blogApp, postId,pageIndex, pageSize));
            string str = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<ArticleComment>>(str);
        }

        /// <summary>
        /// 添加评论
        /// </summary>
        /// <param name="token"></param>
        /// <param name="bolgApp"></param>
        /// <param name="postId"></param>
        /// <param name="content"></param>
        /// <returns></returns>
		public async static Task<bool> AddArticleComments(AccessToken token,string bolgApp,int postId,string content)
        {
            HttpClient _httpClient = new HttpClient();
			_httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer "+token.access_token);
			HttpContent httpContent = new StringContent(content);
			var response = await _httpClient.PostAsync(string.Format(AddArticleCommentsUrl, bolgApp, postId),httpContent);
			return response.StatusCode== HttpStatusCode.OK;
        }

        /// <summary>
        /// 获取文章详情
        /// </summary>
        /// <param name="token"></param>
        /// <param name="blogId"></param>
        /// <returns></returns>
        public async static Task<string> GetArticleContent(AccessToken token,int blogId)
        {
            HttpClient _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + token.access_token);
            var response = await _httpClient.GetAsync(string.Format(GetArticleContentUrl, blogId));
			return await response.Content.ReadAsStringAsync();
        }

        /// <summary>
        /// 根据作者名搜索博主。
        /// </summary>
        /// <param name="bloggerTitle">博客标题。</param>
        /// <returns>博主列表。</returns>
        public static async Task<List<Blogger>> SearchBloggerAsync(string bloggerTitle)
        {
            var url = string.Format(CultureInfo.InvariantCulture, SearchBloggerUrl, WebUtility.UrlEncode(bloggerTitle));
            var uri = new Uri(url, UriKind.Absolute);
            var request = WebRequest.Create(uri);
            using (var response = await request.GetResponseAsync())
            {
                var document = XDocument.Load(response.GetResponseStream());
                return BloggerHelper.Deserialize(document);
            }
        }

		/// <summary>
		/// 分页获取博主文章
		/// </summary>
		/// <returns></returns>
		/// <param name="token">Token.</param>
		/// <param name="bolgApp">博主名.</param>
		/// <param name="pageIndex"></param>
		public async static Task<List<Article>> GetBolggerPosts(AccessToken token, string bolgApp, int pageIndex)
		{
			HttpClient _httpClient = new HttpClient();
			_httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + token.access_token);
			var response = await _httpClient.GetAsync(string.Format(BloggerPostsUrl, bolgApp,pageIndex));
			string str = await response.Content.ReadAsStringAsync();
			return JsonConvert.DeserializeObject<List<Article>>(str);
		}

		/// <summary>
		/// 分页获取知识库文章
		/// </summary>
		/// <returns>The KBA rticles.</returns>
		/// <param name="token">Token.</param>
		/// <param name="pageIndex">Page index.</param>
		/// <param name="pageSize">Page size.</param>
		public async static Task<List<KbArticle>> GetKBArticles(AccessToken token, int pageIndex, int pageSize)
		{
			HttpClient _httpClient = new HttpClient();
			_httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + token.access_token);
			var response = await _httpClient.GetAsync(string.Format(KBArticlesUrl, pageIndex, pageSize));
			string str = await response.Content.ReadAsStringAsync();
			return JsonConvert.DeserializeObject<List<KbArticle>>(str);
		}

		/// <summary>
		/// 获取知识库文章详情
		/// </summary>
		/// <returns>The kb article content.</returns>
		/// <param name="token">Token.</param>
		/// <param name="blogId">Blog identifier.</param>
		public async static Task<string> GetKbArticleContent(AccessToken token, int blogId)
		{
			HttpClient _httpClient = new HttpClient();
			_httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + token.access_token);
			var response = await _httpClient.GetAsync(string.Format(KBArticleBodyUrl, blogId));
			return await response.Content.ReadAsStringAsync();
		}

		/// <summary>
		/// 获取收藏内容
		/// </summary>
		/// <returns>The mark book content.</returns>
		/// <param name="token">Token.</param>
		/// <param name="url">URL.</param>
		public async static Task<string> GetMarkBookContent(AccessToken token, string url)
		{
			HttpClient _httpClient = new HttpClient();
			_httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + token.access_token);
			var response = await _httpClient.GetAsync(url);
			return await response.Content.ReadAsStringAsync();
		}

        public static async Task<string> BodyAsync(int articleId)
		{
			if (articleId < 1)
			{
				throw new ArgumentOutOfRangeException(nameof(articleId));
			}
			var url = string.Format(CultureInfo.InvariantCulture, GetArticleContentUrl, articleId);
			var uri = new Uri(url, UriKind.Absolute);
			var request = WebRequest.Create(uri);
			using (var response = await request.GetResponseAsync())
			{
				var document = XDocument.Load(response.GetResponseStream());
				return document.Root?.Value ?? string.Empty;
			}
		}
    }
}
