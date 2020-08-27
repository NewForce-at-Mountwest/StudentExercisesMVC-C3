using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace StudentExercisesMVC.Models
{
    public class Student
    {
        public int Id { get; set; }

        [Required]
        public string FirstName {get; set;}

        public string LastName { get; set; }

        public string SlackHandle { get; set; }

        public int CohortId { get; set; }

      
    }
}
