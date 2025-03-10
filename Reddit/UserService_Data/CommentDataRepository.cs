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
    public class CommentDataRepository
    {
        private CloudStorageAccount _storageAccount;
        private CloudTable _table;

        public CommentDataRepository()
        {
            _storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("DataConnectionString"));
            CloudTableClient tableClient = new CloudTableClient(new Uri(_storageAccount.TableEndpoint.AbsoluteUri), _storageAccount.Credentials);
            _table = tableClient.GetTableReference("commentTable"); _table.CreateIfNotExists();
        }

        public bool Exists(string Id)
        {
            return RetrieveAllComments().Where(s => s.RowKey == Id).FirstOrDefault() != null;
        }

        public IEnumerable<Comment> RetrieveAllComments()
        {
            var results = from g in _table.CreateQuery<Comment>()
                          where g.PartitionKey == "Comment"
                          select g;
            return results;
        }

        public void AddComment(Comment newComment)
        {
            TableOperation insertOperation = TableOperation.Insert(newComment);
            _table.Execute(insertOperation);
        }


        public void UpdateComment(Comment comment)
        {
            TableOperation updateOperation = TableOperation.Replace(comment);
            _table.Execute(updateOperation);
        }

        public void DeleteComment(Comment comment)
        {
            TableOperation deleteOperation = TableOperation.Delete(comment);
            _table.Execute(deleteOperation);
        }
    }
}
