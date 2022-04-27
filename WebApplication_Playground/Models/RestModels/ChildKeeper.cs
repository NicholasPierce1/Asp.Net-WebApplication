using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication_Playground.Models.RestModels
{
    public sealed class ChildKeeper
    {

        [Required(AllowEmptyStrings = false, ErrorMessage = "{0} cannot be null or empty")]
        [StringLength(maximumLength: 5, ErrorMessage = "{0} length must be between {1} and {2} inclusive", MinimumLength = 3)]
        public string name { get; set; }

        [Required(ErrorMessage = "{0} cannot be empty")]
        public IList<Child> children { get; set; }

    }
}
