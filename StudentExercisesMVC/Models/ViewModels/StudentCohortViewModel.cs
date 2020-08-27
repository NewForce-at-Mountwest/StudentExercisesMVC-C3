using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StudentExercisesMVC.Models.ViewModels
{
    public class StudentCohortViewModel
    {
        // a property of type student
        public Student student { get; set; }

        // dropdown list of ALLLLL the cohorts
        public List<SelectListItem> cohorts { get; set; } = new List<SelectListItem>();
    }
}
