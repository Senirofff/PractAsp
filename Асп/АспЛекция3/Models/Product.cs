using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace АспЛекция3.Models
{
    public partial class Product
    {
        public int IdProduct { get; set; }

        [Required(ErrorMessage = "Название товара обязательно")]
        [Display(Name = "Название")]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "Цена обязательна")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Цена должна быть больше 0")]
        [PrescriptionRequired]
        [Display(Name = "Цена")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Категория обязательна")]
        [Display(Name = "Категория")]
        public int IdCategory { get; set; }

        [DosageFormat(ErrorMessage = "Некорректный формат дозировки")]
        [Display(Name = "Форма дозировки")]
        public string? DosageForm { get; set; }

        [DosageFormat]
        [Display(Name = "Дозировка")]
        public string? Dosage { get; set; }

        [Display(Name = "Требуется рецепт")]
        public bool RequiresPrescription { get; set; }

        [ExpirationDate]
        [Display(Name = "Срок годности")]
        public DateOnly? ExpirationDate { get; set; }

        [Display(Name = "Описание")]
        public string? Discription { get; set; }

        [Display(Name = "Изображение")]
        public string? ImageFileName { get; set; }
    }
}