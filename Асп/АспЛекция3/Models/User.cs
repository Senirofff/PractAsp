﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace АспЛекция3.Models
{
    public partial class User
    {
        public int IdUser { get; set; }

        [Required(ErrorMessage = "Логин обязателен ")]
        [Display(Name = "Логин ")]
        public string Login { get; set; } = null!;

        [Required(ErrorMessage = "Пароль обязателен ")]
        [Display(Name = "Пароль ")]
        public string PasswordHash { get; set; } = null!;

        [Required(ErrorMessage = "Роль обязательна ")]
        [Display(Name = "Роль ")]
        public int IdRole { get; set; }

        [Required(ErrorMessage = "Фамилия обязательна ")]
        [Display(Name = "Фамилия ")]
        public string LastName { get; set; } = null!;

        [Required(ErrorMessage = "Имя обязательно ")]
        [Display(Name = "Имя ")]
        public string FirstName { get; set; } = null!;

        [Display(Name = "Отчество ")]
        public string? MiddleName { get; set; }

        [PhoneNumber(ErrorMessage = "Введите корректный российский номер телефона ")]
        [Display(Name = "Телефон ")]
        public string? Phone { get; set; }

        [EmailAddress(ErrorMessage = "Некорректный формат email ")]
        [EmailDomain(ErrorMessage = "Разрешены только почтовые сервисы: mail.ru, yandex.ru, gmail.com, bk.ru ")]
        [Display(Name = "Email ")]
        public string? Email { get; set; }

        [Column("discount_percent")]
        [Display(Name = "Скидка (%)")]
        [Range(0, 100, ErrorMessage = "Скидка должна быть от 0 до 100")]
        public int DiscountPercent { get; set; } = 0;

        [Column("is_active")]  
        [Display(Name = "Активен")]
        public bool IsActive { get; set; } = true;

    }
}