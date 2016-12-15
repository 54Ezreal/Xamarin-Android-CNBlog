using System;
namespace CNBlogAPI.Model
{
	public class AccessToken
	{
		public AccessToken()
		{
		}
		public string access_token { get; set; }

		public string token_type { get; set; }

		public int expires_in { get; set; }

		public bool IsIdentityUser { get; set; }

		public DateTime RefreshTime { get; set; }

		public bool CheckTokenIsOverdue()
		{
			return DateTime.Now > RefreshTime.AddSeconds(expires_in);
		}
	}
}

