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
    public class VotesDataRepository
    {
        private CloudStorageAccount _storageAccount;
        private CloudTable _table;
        public VotesDataRepository()
        {
            _storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("DataConnectionString"));
            CloudTableClient tableClient = new CloudTableClient(new Uri(_storageAccount.TableEndpoint.AbsoluteUri), _storageAccount.Credentials);
            _table = tableClient.GetTableReference("VotesTable"); _table.CreateIfNotExists();
        }

        public bool Exists(string UserNo)
        {
            return RetrieveAllVotes().Where(s => s.RowKey == UserNo).FirstOrDefault() != null;
        }

        public IEnumerable<Votes> RetrieveAllVotes()
        {
            var results = from g in _table.CreateQuery<Votes>()
                          where g.PartitionKey == "Votes"
                          select g;
            return results;
        }

        public void AddVote(Votes newVote)
        {
            TableOperation insertOperation = TableOperation.Insert(newVote);
            _table.Execute(insertOperation);
        }

        public void UpdateVote(Votes vote)
        {
            TableOperation updateOperation = TableOperation.Replace(vote);
            _table.Execute(updateOperation);
        }

        public void DeleteVote(string id)
        {
            Votes vote = RetrieveAllVotes().Where(s => s.RowKey == id).FirstOrDefault();

            if (vote != null)
            {
                TableOperation deleteOperation = TableOperation.Delete(vote);
                _table.Execute(deleteOperation);
            }
        }
    }
}
