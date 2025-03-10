using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserService_Data
{
    public class Votes : TableEntity
    {
        public string ThemeId { get; set; }
        public string UserId { get; set; }
        public string VoteType { get; set; }

        public Votes()
        {

        }

        public Votes(string id)
        {
            PartitionKey = "Votes";
            RowKey = id;
        }
    }
}
