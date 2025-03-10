using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;

namespace UserService_Data
{
    public class SubscriptionRepository
    {
        private CloudStorageAccount _storageAccount;
        private CloudTable _table;

        public SubscriptionRepository()
        {
            _storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("DataConnectionString"));
            CloudTableClient tableClient = new CloudTableClient(new Uri(_storageAccount.TableEndpoint.AbsoluteUri), _storageAccount.Credentials);
            _table = tableClient.GetTableReference("subscriptionTable"); _table.CreateIfNotExists();
        }

        public bool Exists(string Id)
        {
            return RetrieveAllSubscriptions().Where(s => s.RowKey == Id).FirstOrDefault() != null;
        }

        public bool SubscriptionExists(string themeId, string userId)
        {
            var subs = RetrieveAllSubscriptions().ToList();
            return subs.Any(sub => sub.ThemeId == themeId && sub.UserId == userId);
        }

        public IEnumerable<Subscription> RetrieveAllSubscriptions()
        {
            var results = from g in _table.CreateQuery<Subscription>()
                          where g.PartitionKey == "Subscription"
                          select g;
            return results;
        }

        public void AddSubscription(Subscription sub)
        {
            TableOperation insertOperation = TableOperation.Insert(sub);
            _table.Execute(insertOperation);
        }


        public void UpdateSubscription(Subscription sub)
        {
            TableOperation updateOperation = TableOperation.Replace(sub);
            _table.Execute(updateOperation);
        }

        public void DeleteSubscription(Subscription sub)
        {
            TableOperation deleteOperation = TableOperation.Delete(sub);
            _table.Execute(deleteOperation);
        }
    }
}
