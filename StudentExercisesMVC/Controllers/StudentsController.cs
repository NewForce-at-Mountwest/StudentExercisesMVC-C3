using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using StudentExercisesMVC.Models;
using StudentExercisesMVC.Models.ViewModels;

namespace StudentExercisesMVC.Controllers
{
    public class StudentsController : Controller
    {

        private readonly IConfiguration _config;

        public StudentsController(IConfiguration config)
        {
            _config = config;
        }

        public SqlConnection Connection
        {
            get
            {
                return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            }
        }
        // GET: Students

            // Show list of students with their cohort
        public ActionResult Index()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
            SELECT s.Id,
                s.FirstName,
                s.LastName,
                s.SlackHandle,
                c.Name
            FROM Student s JOIN Cohort c ON s.CohortId = c.Id
        ";
                    SqlDataReader reader = cmd.ExecuteReader();

                    List<Student> students = new List<Student>();
                    while (reader.Read())
                    {
                        Student student = new Student
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                            LastName = reader.GetString(reader.GetOrdinal("LastName")),
                            SlackHandle = reader.GetString(reader.GetOrdinal("SlackHandle")),
                            Cohort = new Cohort
                            {
                                Name = reader.GetString(reader.GetOrdinal("Name"))
                            }
                        };

                        students.Add(student);
                    }

                    reader.Close();

                    return View(students);
                }
            }
        }

        // GET: Students/Details/5
        public ActionResult Details(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {

                    // Select a single student using SQL by their id
                    cmd.CommandText = @"
            SELECT s.Id,
                s.FirstName,
                s.LastName,
                s.SlackHandle,
                s.CohortId
            FROM Student s
            WHERE id = @id
        ";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    // Map the raw SQL data to a student model
                    Student student = null;
                    if (reader.Read())
                    {
                        student = new Student
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                            LastName = reader.GetString(reader.GetOrdinal("LastName")),
                            SlackHandle = reader.GetString(reader.GetOrdinal("SlackHandle")),
                            CohortId = reader.GetInt32(reader.GetOrdinal("CohortId"))
                        };

                    }

                    reader.Close();

                    // If we got something back to the db, send us to the details view
                    if (student != null)
                    {
                        return View(student);
                    }
                    else
                    {
                        // If we didn't get anything back from the db, we made a custom not found page down here
                        return RedirectToAction(nameof(NotFound));
                    }

                }
            }
        }



        // GET: Students/Create
        public ActionResult Create()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {

                    // Select all the cohorts
                    cmd.CommandText = @"SELECT Cohort.Id, Cohort.Name FROM Cohort";

                    SqlDataReader reader = cmd.ExecuteReader();

                    // Create a new instance of our view model
                    StudentCohortViewModel viewModel = new StudentCohortViewModel();
                    while (reader.Read())
                    {
                        // Map the raw data to our cohort model
                        Cohort cohort = new Cohort
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("Name"))
                        };

                        // Use the info to build our SelectListItem
                        SelectListItem cohortOptionTag = new SelectListItem()
                        {
                            Text = cohort.Name,
                            Value = cohort.Id.ToString()
                        };

                        // Add the select list item to our list of dropdown options
                        viewModel.cohorts.Add(cohortOptionTag);

                    }

                    reader.Close();


                    // send it all to the view
                    return View(viewModel);
                }
            }
        }

        // POST: Students/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(StudentCohortViewModel viewModel)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"INSERT INTO Student
                ( FirstName, LastName, SlackHandle, CohortId )
                VALUES
                ( @firstName, @lastName, @slackHandle, @cohortId )";
                        cmd.Parameters.Add(new SqlParameter("@firstName", viewModel.student.FirstName));
                        cmd.Parameters.Add(new SqlParameter("@lastName", viewModel.student.LastName));
                        cmd.Parameters.Add(new SqlParameter("@slackHandle", viewModel.student.SlackHandle));
                        cmd.Parameters.Add(new SqlParameter("@cohortId", viewModel.student.CohortId));
                        cmd.ExecuteNonQuery();

                        return RedirectToAction(nameof(Index));
                    }
                }
            }
            catch
            {
                return View();
            }
        }

        // GET: Students/Edit/5
        // This method loads the edit form
        public ActionResult Edit(int id)
        {

            StudentCohortViewModel viewModel = new StudentCohortViewModel();
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {

                    // When we load the edit form, we need to select the correct student from the db so that we can pre-load their info into the form fields
                    cmd.CommandText = @"
            SELECT s.Id,
                s.FirstName,
                s.LastName,
                s.SlackHandle,
                s.CohortId
            FROM Student s
            WHERE id = @id
        ";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    // map the raw SQL data to our student model and attach it to the view model
                    if (reader.Read())
                    {
                        viewModel.student = new Student
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                            LastName = reader.GetString(reader.GetOrdinal("LastName")),
                            SlackHandle = reader.GetString(reader.GetOrdinal("SlackHandle")),
                            CohortId = reader.GetInt32(reader.GetOrdinal("CohortId"))
                        };

                    }

                    reader.Close();

                    // Get all the cohorts for a dropdown
                    cmd.CommandText = @"SELECT Cohort.Id, Cohort.Name FROM Cohort";
                    SqlDataReader cohortReader = cmd.ExecuteReader();

                    while (cohortReader.Read())
                    {
                        // Map the raw data to our cohort model
                        Cohort cohort = new Cohort
                        {
                            Id = cohortReader.GetInt32(cohortReader.GetOrdinal("Id")),
                            Name = cohortReader.GetString(cohortReader.GetOrdinal("Name"))
                        };

                        // Use the info to build our SelectListItem
                        SelectListItem cohortOptionTag = new SelectListItem()
                        {
                            Text = cohort.Name,
                            Value = cohort.Id.ToString()
                        };

                        // Add the select list item to our list of dropdown options
                        viewModel.cohorts.Add(cohortOptionTag);

                    }

                    cohortReader.Close();

                }
            }

            return View(viewModel);
        }

        // POST: Students/Edit/5
        // This method runs when we SUBMIT the edit form
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, Student student)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        // When we submit the edit form, go ahead and update the db with the info we passed in
                        cmd.CommandText = @"UPDATE Student
                                            SET firstName=@firstName, 
                                            lastName=@lastName, 
                                            slackHandle=@slackHandle, 
                                            cohortId=@cohortId
                                            WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@firstName", student.FirstName));
                        cmd.Parameters.Add(new SqlParameter("@lastName", student.LastName));
                        cmd.Parameters.Add(new SqlParameter("@slackHandle", student.SlackHandle));
                        cmd.Parameters.Add(new SqlParameter("@cohortId", student.CohortId));
                        cmd.Parameters.Add(new SqlParameter("@id", id));

                        int rowsAffected = cmd.ExecuteNonQuery();


                    }
                }
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View(student);
            }
        }

        // GET: Students/Delete/5
        // This method runs when we click the "delete" link from the index view or the details view
        // Anchor tags (like the ones in the index view) automatically send a GET request
        // This will load a "are you sure you want to delete this?" pages
        public ActionResult Delete(int id, IFormCollection collection)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {

                    // When we confirm that the user wants to delete a student, let's show them the student they're about to delete. That means we need to select it from the db
                    cmd.CommandText = @"
            SELECT s.Id,
                s.FirstName,
                s.LastName,
                s.SlackHandle,
                s.CohortId
            FROM Student s
            WHERE id = @id
        ";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    // And then map it to our models so that we can send it to our view
                    Student student = null;
                    if (reader.Read())
                    {
                        student = new Student
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                            LastName = reader.GetString(reader.GetOrdinal("LastName")),
                            SlackHandle = reader.GetString(reader.GetOrdinal("SlackHandle")),
                            CohortId = reader.GetInt32(reader.GetOrdinal("CohortId"))
                        };

                    }

                    reader.Close();

                    if (student != null)
                    {
                        return View(student);
                    }
                    else
                    {
                        return RedirectToAction(nameof(NotFound));
                    }

                }
            }
        }

        // POST: Students/Delete/5
        // This method runs once they have CONFIRMED that they want to delete a student
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            try
            {

                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        // We'll take the Id of the student we want to delete, and we'll delete all their exercises and also the student itself
                        cmd.CommandText = @"DELETE FROM StudentExercise WHERE studentId = @id
                        DELETE FROM Student WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@id", id));

                        int rowsAffected = cmd.ExecuteNonQuery();

                    }
                }

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // This is a method we made to handle 404's. This will show us the NotFound view in our students folder.
        public new ActionResult NotFound()
        {
            return View();
        }

        public ActionResult Taco()
        {
            return View();
        }


    }
}