using System;

namespace CNBlogAPI.Model
{
    public class UserInfo
    {
        public Guid UserId { get; set; }

        public int SpaceUserId { get; set; }

        public int BlogId { get; set; }

        public string DisplayName { get; set; }

        public string Face { get; set; }

        public string Avatar { get; set; }

        /// <summary>
        /// 园龄
        /// </summary>
        public string Seniority { get; set; }

        public string BlogApp { get; set; }

		public string UName { get; set; }

		public string Pwd { get; set; }
    }
}
