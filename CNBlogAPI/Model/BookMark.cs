using System;
namespace CNBlogAPI.Model
{
	public class BookMark
	{
		public int WzLinkId { get; set; }
		public string Title { get; set; }
		public string LinkUrl { get; set; }
		public string Summary { get; set; }
		public string[] Tags { get; set; }
		public DateTime DateAdded { get; set; }
		public bool FromCNBlogs { get; set; }
	}
}
