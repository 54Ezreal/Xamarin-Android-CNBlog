namespace CNBlogAPI.Model
{
	/// <summary>
	/// 已登录用户身份标识
	/// </summary>
    public class IdentityAccessToken : AnonymousAccessToken
    {
        public string refresh_token { get; set; }
    }
}
