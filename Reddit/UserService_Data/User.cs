using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserService_Data
{
    public class User : TableEntity
    {
        public string Ime { get; set; }
        public string Prezime { get; set; }
        public string Adresa { get; set; }
        public string Grad { get; set; }
        public string Drzava { get; set; }
        public string Broj_telefona { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string PhotoUrl { get; set; }
        public List<Topic> Themes { get; set; }

        public User()
        {

        }

        public User(string UserNo)
        {
            PartitionKey = "User";
            RowKey = UserNo;
        }
    }
}
