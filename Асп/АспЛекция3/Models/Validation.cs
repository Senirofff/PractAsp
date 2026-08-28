using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using АспЛекция3.Models;

namespace АспЛекция3.Models
{
    public class PhoneNumberAttribute : ValidationAttribute
    {
        public PhoneNumberAttribute()
        {
            ErrorMessage = "Введите корректный российский номер телефона (например: +7(999)123-45-67, 89991234567)";
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                return ValidationResult.Success;

            string phone = value.ToString()!.Trim();
            // Поддерживаемые форматы: +7(999)123-45-67, 8(999)1234567, 89991234567
            string pattern = @"^(\+7|8)?[\s\-]?\(?\d{3}\)?[\s\-]?\d{3}[\s\-]?\d{2}[\s\-]?\d{2}$";

            if (Regex.IsMatch(phone, pattern))
                return ValidationResult.Success;

            return new ValidationResult(ErrorMessage);
        }
    }

    public class EmailDomainAttribute : ValidationAttribute
    {
        private readonly string[] _allowedDomains = { "mail.ru", "yandex.ru", "gmail.com", "bk.ru", "inbox.ru", "list.ru" };

        public EmailDomainAttribute()
        {
            ErrorMessage = "Разрешены только следующие почтовые сервисы: mail.ru, yandex.ru, gmail.com, bk.ru";
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                return ValidationResult.Success;

            string email = value.ToString()!.Trim().ToLower();

            foreach (var domain in _allowedDomains)
            {
                if (email.EndsWith("@" + domain))
                    return ValidationResult.Success;
            }

            return new ValidationResult(ErrorMessage);
        }
    }

    public class ExpirationDateAttribute : ValidationAttribute
    {
        public ExpirationDateAttribute()
        {
            ErrorMessage = "Срок годности должен быть в будущем и не более чем через 5 лет";
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null)
                return ValidationResult.Success;

            if (value is not DateOnly expirationDate)
                return new ValidationResult("Некорректный формат даты");

            DateOnly today = DateOnly.FromDateTime(DateTime.Now);
            DateOnly maxDate = today.AddYears(5);

            if (expirationDate <= today)
                return new ValidationResult("Срок годности не может быть в прошлом или сегодняшним днём");

            if (expirationDate > maxDate)
                return new ValidationResult("Срок годности не может превышать 5 лет от текущей даты");

            return ValidationResult.Success;
        }
    }
    public class DosageFormatAttribute : ValidationAttribute
    {
        public DosageFormatAttribute()
        {
            ErrorMessage = "Некорректный формат дозировки. Примеры: 500 мг, 10 мл, таблетки, капсулы, сироп, мазь, ME, —";
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                return ValidationResult.Success;

            string input = value.ToString()!.Trim();

            if (input == "—" || input == "-" || input.Equals("ME", StringComparison.OrdinalIgnoreCase))
                return ValidationResult.Success;

            string[] simpleForms = {
            "таблетки", "капсулы", "таблетка", "капсула", "сироп", "мазь", "гель", "крем",
            "раствор", "спрей", "порошок", "суспензия", "ампулы", "флакон", "пачка",
            "капли", "пластырь", "свечи", "суппозитории"
        };

            if (simpleForms.Any(f => input.ToLower().Contains(f)))
                return ValidationResult.Success;

            string pattern = @"^\d+(\.\d+)?\s*(мг|г|мл|л|таб\.?|капс\.?|%|амп\.?|фл\.?|ме|ME|ed|ED)?$";

            if (Regex.IsMatch(input, pattern, RegexOptions.IgnoreCase))
                return ValidationResult.Success;

            return new ValidationResult(ErrorMessage);
        }
    }

    public class PrescriptionRequiredAttribute : ValidationAttribute
    {
        public PrescriptionRequiredAttribute()
        {
            ErrorMessage = "Для рецептурных препаратов цена должна быть выше 100 рублей";
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var product = validationContext.ObjectInstance as Product;

            if (product == null)
                return ValidationResult.Success;

            if (product.RequiresPrescription && product.Price <= 100)
            {
                return new ValidationResult(ErrorMessage);
            }

            return ValidationResult.Success;
        }
    }
}