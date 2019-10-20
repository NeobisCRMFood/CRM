using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DataTier.Entities.Concrete
{
    public class User
    {
        [JsonIgnore]
        public int Id { get; set; }

        [Required(ErrorMessage = "Заполните имя")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Заполните фамилию")]
        public string LastName { get; set; }

        public string MiddleName { get; set; }
        public string Gender { get; set; }

        [Required(ErrorMessage = "Укажите дату рождения")]
        public DateTime DateBorn { get; set; }

        [Required(ErrorMessage = "Заполните номер телефона")]
        [RegularExpression(@"^\(?\+([9]{2}?[6])\)?[-. ]?([0-9]{3})[-. ]?([0-9]{3})[-. ]?([0-9]{3})$", ErrorMessage = "Некорректный номер телефона")]
        public string PhoneNumber { get; set; }
        
        [Required(ErrorMessage = "Заполните логин")]
        public string Login { get; set; }
        [Required(ErrorMessage = "Заполните пароль")]
        public string Password { get; set; }

        public DateTime StartWorkDay { get; set; }

        [Required(ErrorMessage = "Выберите роль")]
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
