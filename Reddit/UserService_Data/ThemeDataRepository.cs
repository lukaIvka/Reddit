using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserService_Data
{
    public class ThemeDataRepository
    {
        private CloudStorageAccount _storageAccount;
        private CloudTable _table;

        public ThemeDataRepository()
        {
            _storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("DataConnectionString"));
            CloudTableClient tableClient = new CloudTableClient(new Uri(_storageAccount.TableEndpoint.AbsoluteUri), _storageAccount.Credentials);
            _table = tableClient.GetTableReference("topicTable"); _table.CreateIfNotExists();
        }

        public bool Exists(string Id)
        {
            return RetrieveAllThemes().Where(s => s.RowKey == Id).FirstOrDefault() != null;
        }

        public IEnumerable<Topic> RetrieveAllThemes()
        {
            var results = from g in _table.CreateQuery<Topic>()
                          where g.PartitionKey == "Topic"
                          select g;
            return results;
        }

        public void AddTheme(Topic newTopic)
        {
            TableOperation insertOperation = TableOperation.Insert(newTopic);
            _table.Execute(insertOperation);
        }


        public void UpdateTopic(Topic topic)
        {
            TableOperation updateOperation = TableOperation.Replace(topic);
            _table.Execute(updateOperation);
        }

        public void DeleteTheme(Topic topic)
        {
            TableOperation deleteOperation = TableOperation.Delete(topic);
            _table.Execute(deleteOperation);
        }

    }
}
