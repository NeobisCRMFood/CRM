using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataTier.Entities.Concrete
{
    public class User
    {
        [JsonIgnore]
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleName { get; set; }
        public string Gender { get; set; }
        public DateTime DateBorn { get; set; }
        public string PhoneNumber { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public DateTime StartWorkDay { get; set; }
        public int RoleId { get; set; }
        [JsonIgnore]
        public Role Role { get; set; }
        public string Comment { get; set; }

        [JsonIgnore]
        public ICollection<Order> Orders { get; set; }
        public User()
        {
            Orders = new List<Order>();
        }
    }
}
