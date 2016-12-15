using System;

namespace CNBlogAPI.Model
{
    /// <summary>
    /// 博主
    /// </summary>
    public class Blogger
    {
        /// <summary>
        /// 博客主页。
        /// </summary>
        public Uri Id { get; set; } 

        /// <summary>
        /// 博客标题。
        /// </summary>
        public string Title
        { get; set; }

        /// <summary>
        /// 最后更新日期。
        /// </summary>
        public DateTime Updated
        { get; set; }

        /// <summary>
        /// 博客主页。
        /// </summary>
        public Uri Link
        { get; set; }

        /// <summary>
        /// 用户名。
        /// </summary>
        public string BlogApp
        { get; set; }

        /// <summary>
        /// 头像，可能为空。
        /// </summary>
		public string Avatar
        { get; set; }

        /// <summary>
        /// 发表文章数。
        /// </summary>
        public int PostCount
        { get; set; }

    }
}