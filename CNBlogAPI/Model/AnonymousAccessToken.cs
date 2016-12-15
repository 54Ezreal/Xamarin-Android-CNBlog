using System;
namespace CNBlogAPI.Model
{
	/// <summary>
	/// 匿名用户身份标识(未登录用户)
	/// </summary>
    public class AnonymousAccessToken
    {
        public string access_token { get; set; }

        public string token_type { get; set; }

        public int expires_in { get; set; }
    }
}
