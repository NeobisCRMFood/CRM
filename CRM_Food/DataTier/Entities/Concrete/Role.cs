using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DataTier.Entities.Concrete
{
    public class Role
    {
        [JsonIgnore]
        public int Id { get; set; }

        [Required(ErrorMessage = "Заполните название")]
        public string Name { get; set; }

        [JsonIgnore]
        public ICollection<User> Users { get; set; }

        public Role()
        {
            Users = new List<User>();
        }
    }
}
