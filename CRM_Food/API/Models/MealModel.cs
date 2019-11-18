using DataTier.Entities.Abstract;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace API.Models
{
    public class MealModel
    {
        [Required(ErrorMessage = "Укажите Id категории")]
        public int CategoryId { get; set; }
        [Required(ErrorMessage = "Заполните название")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Заполните описание")]
        public string Description { get; set; }
        [Required(ErrorMessage = "Укажите цену")]
        public decimal Price { get; set; }
        public string Weight { get; set; }
        public string ImageURL { get; set; }
    }
}
