using System;

namespace assignment_code.Models
{
    public class CommentItem
    {
        public int Id { get; set; }
        public string Author { get; set; }
        public string Preview { get; set; }
        public string Date { get; set; }

        public CommentItem() { }

        public CommentItem(int id, string author, string preview, string date)
        {
            Id = id;
            Author = author;
            Preview = preview;
            Date = date;
        }
    }
}
