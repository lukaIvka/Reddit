using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace UserService_Data
{
    public class Subscription : TableEntity
    {
        public string ThemeId { get; set; }
        public string UserId { get; set; }

        public Subscription(string themeId, string userId)
        {
            ThemeId = themeId;
            UserId = userId;
        }

        public Subscription()
        {
            PartitionKey = "Subscription";
            RowKey = Guid.NewGuid().ToString();
        }
    }
}
