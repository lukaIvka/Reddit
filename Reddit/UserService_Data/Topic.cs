using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace UserService_Data
{
    public class Topic : TableEntity
    {
        public string Publisher { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime Time_published { get; set; }
        public int Upvote { get; set; }
        public int Downvote { get; set; }
        public List<Comment> Comments { get; set; }
        public string PhotoUrl { get; set; }
        
        public Topic(string title, string publisher, string content, DateTime time_published, int upvote, int downvote, List<Comment> comments, string photoUrl)
        {
            Publisher = publisher;
            Title = title;
            Content = content;
            Time_published = time_published;
            Upvote = upvote;
            Downvote = downvote;
            Comments = comments;
            PhotoUrl = photoUrl;
        }

        public Topic()
        {
            PartitionKey = "Topic";
            RowKey = Guid.NewGuid().ToString();
        }

    }
}
