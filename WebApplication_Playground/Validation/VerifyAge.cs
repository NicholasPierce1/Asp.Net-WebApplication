using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using WebApplication_Playground.Models.RestModels;

namespace WebApplication_Playground.Validation
{
    public sealed class VerifyAge : ValidationAttribute
    {

        public string? customErrorMessage { get; set; }

        public VerifyAge(){}

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {

            DateTime dateOfBirth = (DateTime)value; //(DateTime)validationContext.ObjectInstance;

            Person person = (Person)validationContext.ObjectInstance;

            DateTime legalAge = DateTime.Now.AddYears(-21);

            if (DateTime.Compare(legalAge, dateOfBirth) < 0)
                return new ValidationResult(
                    customErrorMessage?.Replace("{0}", validationContext.MemberName) ??
                    $"{validationContext.MemberName}: must be at least 21 years old." +
                    $" Age: {legalAge.ToString("MM/dd/yyyy")}"
                );

            return ValidationResult.Success;

        }
    }
}
