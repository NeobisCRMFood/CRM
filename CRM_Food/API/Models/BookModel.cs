using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace API.Models
{
    public class BookModel
    {
        public int TableId { get; set; }
        public DateTime BookDate { get; set; }
        public int MenQuantity { get; set; }
        public string ClientName { get; set; }
        [Required(ErrorMessage = "Заполните номер телефона в правильном формате")]
        [RegularExpression(@"^\(?\+([9]{2}?[6])\)?[-. ]?([0-9]{3})[-. ]?([0-9]{3})[-. ]?([0-9]{3})$", ErrorMessage = "Некорректный номер телефона")]
        public string PhoneNumber { get; set; }
    }
}
