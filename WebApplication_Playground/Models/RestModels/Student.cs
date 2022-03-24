using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication_Playground.Models.RestModels
{
    public sealed class Student
    {

        public enum ClassLevel : int { Freshman = 1, Sophmore = 2, Junior = 3, Senior = 4 }

        public string firstName { get; set; }

        public string lastName{ get; set; } // public fields NOT ok for serialization

        public string fullName { get => $"{this.firstName} {this.lastName}"; }

        [DataType(DataType.Date)] // can be time or datetime too
        // [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "yyyy/MM/dd")]
        public DateTime expectedGraduationDate { get; set; }

        public readonly Guid id = Guid.NewGuid();
        public int totalCredits { get; set; }

        public ClassLevel classLevel
        {
            // assume no negatives/malicious inputs
            get
            {
                if (this.totalCredits <= 40)
                    return ClassLevel.Freshman;
                else if (this.totalCredits <= 70)
                    return ClassLevel.Sophmore;
                else if (this.totalCredits <= 100)
                    return ClassLevel.Junior;
                else
                    return ClassLevel.Senior;
            }
        }

        public void addCredits(int credits)
        {
            this.totalCredits += credits;
        }

    }
}
