using Microsoft.WindowsAzure.Storage.Table;
using System;


namespace UserService_Data
{
    public class Comment : TableEntity
    {
        public string Publisher { get; set; }
        public string ThemeOwner { get; set; }
        public string Content { get; set; }
        public int Upvote { get; set; }
        public int Downvote { get; set; }

       

        public override string ToString()
        {
            return Content;
        }

        public Comment()
        {
            PartitionKey = "Comment";
            RowKey = Guid.NewGuid().ToString(); 
        }

    }
}
