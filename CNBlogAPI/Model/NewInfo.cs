using System;
namespace CNBlogAPI.Model
{
    public class NewInfo
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Summary { get; set; }
        public int TopicId { get; set; }
        public string TopicIcon { get; set; }
        public int ViewCount { get; set; }
        public int CommentCount { get; set; }
        public int DiggCount { get; set; }
        public DateTime DateAdded { get; set; }
    }
}
