using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication_Playground.Models.RestModels
{
    public sealed class Child
    {

        [Required(ErrorMessage = "{0}: is required")]
        public string name { get; set; }

    }
}
