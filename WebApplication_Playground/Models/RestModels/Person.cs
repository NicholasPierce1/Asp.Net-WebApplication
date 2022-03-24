using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using WebApplication_Playground.Validation;

namespace WebApplication_Playground.Models.RestModels
{
    public sealed class Person
    {

        public enum MaritalStatus {single = 0, married = 1}

        /*
         *DateType, Format, Display are for UI (ie: Html.DisplayNameFor(m => m.firstName))
         */

        [Required(AllowEmptyStrings = false, ErrorMessage = "firstName: cannot be null or empty")]
        [StringLength(maximumLength: 5, ErrorMessage = "firstName: length must be between 3 and 5 inclusive", MinimumLength = 3)]
        [DataType(DataType.Text)]
        public string firstName { get; set; }

        [Required(ErrorMessage = "maritalStatus: must have a marital status")]
        public MaritalStatus maritalStatus { get; set; }

        public String maritalStatusShort { get => this.maritalStatus.ToString(); }

        [Range(0,100, ErrorMessage = "{0} must be between {1} and {2}")]
        public int age { get; set; }


        [RegularExpression("^[0-9]{3}-[0-9]{2}-[0-9]{4}$", ErrorMessage = "{0} is not in proper format")]
        public String ssn { get; set; }

        [Range(typeof(DateTime), "2022-02-15", "2022-04-15", ErrorMessage = "{0} must be between {1} and {2} -- IRS")]
        [Required(ErrorMessage = "{0} is required")]
        [DataType(DataType.Date)]
        public DateTime fileTaxes { get; set; }

        [Required(ErrorMessage = "{0}: is required")]
        [VerifyAge] //(customErrorMessage = "{0}: required to be at least 21")]
        public DateTime dateOfBirth { get; set; }

        // [Required, ValidateObject]
        [Required]
        public Child child { get; set; }

    }
}
