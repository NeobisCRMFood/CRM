using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DataTier.Entities.Concrete
{
    public class OrderStatus
    {
        [JsonIgnore]
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Название")]
        public string Name { get; set; }
    }
}
