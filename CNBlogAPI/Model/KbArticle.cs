using System;
namespace CNBlogAPI.Model
{
	public class KbArticle
	{
		public int Id { get; set; }
		public string Title { get; set; }
		public string Summary { get; set; }
		public string Author { get; set; }
		public int ViewCount { get; set; }
		public int DiggCount { get; set; }
		public DateTime DateAdded { get; set; }
	}
}
