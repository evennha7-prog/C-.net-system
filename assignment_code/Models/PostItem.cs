using System;

namespace assignment_code.Models
{
    public enum PostStatus
    {
        Published,
        Draft,
        Archived
    }

    public class PostItem
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public PostStatus Status { get; set; }
        public string Date { get; set; }

        public PostItem() { }

        public PostItem(int id, string title, PostStatus status, string date)
        {
            Id = id;
            Title = title;
            Status = status;
            Date = date;
        }
    }
}
