using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Project_6.Models;
using BCrypt.Net;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using Rotativa.AspNetCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;


namespace Project_6.Controllers
{
    public class ManagersController : Controller
    {
        private readonly MyDbContext _context;
        private static Dictionary<string, string> _resetCodes = new Dictionary<string, string>(); // In-memory storage for reset codes

        public ManagersController(MyDbContext context)
        {
            _context = context;
        }


        // GET: Managers
        public IActionResult Index()
        {
            var managerId = HttpContext.Session.GetInt32("ManagerId");

            var myDbContext = _context.RequestLeaves
                .Where(leave => _context.Employees
                    .Any(emp => emp.ManagerId == managerId && emp.Id == leave.EmployeeId))
                .ToList();

            return View(myDbContext);

        }


        public IActionResult Approve(int id)
        {
            var Request = _context.RequestLeaves.Find(id);
            Request.Status = "Approve";
            _context.Update(Request);
            _context.SaveChanges();


            return RedirectToAction(nameof(Index));
        }


        public IActionResult Reject(int id)
        {
            var Request = _context.RequestLeaves.Find(id);
            Request.Status = "Reject";
            _context.Update(Request);
            _context.SaveChanges();


            return RedirectToAction(nameof(Index));
        }

        // GET: Managers/Create
        public IActionResult Addemp()
        {
            ViewData["DepartmentId"] = new SelectList(_context.Departments, "Id", "Name");
            ViewData["ManagerId"] = new SelectList(_context.Managers, "Id", "Name");
            return View();
        }

        // POST: Managers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Addemp(Employee employee, IFormFile ProfileImage)
        {


            if (ProfileImage != null)
            {
                string fileName = Path.GetFileName(ProfileImage.FileName);
                string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/HR_image", fileName);

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    ProfileImage.CopyTo(stream);
                }
                employee.ProfileImage = fileName;
            }

            if (ModelState.IsValid)
            {
                employee.Password = BCrypt.Net.BCrypt.HashPassword(employee.Password);

                _context.Add(employee);
                _context.SaveChanges();
                return RedirectToAction("Dashboard");

            }
            return View();
        }





















        public IActionResult AssignTask()
        {
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "Id", "Name");
            return View();
        }




        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AssignTask(EmpTask task)
        {
            if (ModelState.IsValid)
            {
                // Get employee details (including email) by EmployeeId
                var employee = _context.Employees.FirstOrDefault(e => e.Id == task.EmployeeId);
                if (employee != null)
                {
                    // Save the task to the database
                    task.Status = "To Do";
                    _context.Add(task);
                    _context.SaveChanges();

                    // Prepare email content
                    string subject = $"New Task Assigned: {task.Name}";
                    string body = $@"
                Dear {employee.Name},

                You have been assigned a new task:

                - Task Name: {task.Name}
                - Description: {task.Description}
                - Status: {task.Status}
                - Assigned Date: {task.AssignedDate}
                - Due Date: {task.DueDate}

                Please complete the task by the due date.

                Best regards,
                Your Manager";

                    // Send email notification
                    SendTaskAssignmentEmail(employee.Email, subject, body);

                    ViewBag.Message = "A Task Was Added Sucessfully!";

                    // Redirect back to task list or confirmation page
                    return View();
                }
            }

            // Repopulate employee dropdown if model state is invalid
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "Id", "Id", task.EmployeeId);
            return View(task);
        }

        //private void SendTaskAssignmentEmail(string recipientEmail, string subject, string body)
        //{
        //    using (SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", 465))
        //    {
        //        smtpClient.UseDefaultCredentials = false;
        //        smtpClient.EnableSsl = true;
        //        smtpClient.Credentials = new NetworkCredential("rahaf.alsmairat@gmail.com", "louq hewz zfsx cmez");

        //        MailMessage mailMessage = new MailMessage
        //        {
        //            From = new MailAddress("rahaf.alsmairat@gmail.com"),
        //            Subject = subject,
        //            Body = body,
        //            IsBodyHtml = false
        //        };

        //        mailMessage.To.Add(recipientEmail);
        //        smtpClient.Send(mailMessage);
        //    }
        //}




        private void SendTaskAssignmentEmail(string recipientEmail, string subject, string body)
        {
            using (SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", 587))
            {
                smtpClient.UseDefaultCredentials = false;
                smtpClient.EnableSsl = true;
                smtpClient.Credentials = new NetworkCredential("rahaf.alsmairat@gmail.com", "louq hewz zfsx cmez");
                smtpClient.Timeout = 30000;
                MailMessage mailMessage = new MailMessage
                {
                    From = new MailAddress("rahaf.alsmairat@gmail.com"),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = false
                };

                mailMessage.To.Add(recipientEmail);
                smtpClient.Send(mailMessage);
            }
        }



        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ForgotPassword(string email)
        {
            if (!string.IsNullOrEmpty(email))
            {
                // Check if the email exists in the Manager table
                var manager = _context.Managers.FirstOrDefault(m => m.Email == email);
                var employee = _context.Employees.FirstOrDefault(m => m.Email == email);
                var hr = _context.Hrs.FirstOrDefault(m => m.Email == email);

                if (manager != null || employee != null || hr != null)
                {
                    // Generate a random 6-digit code
                    var code = GenerateRandomCode();

                    // Store the code in memory
                    _resetCodes[email] = code;

                    // Send the code via email
                    SendResetCodeEmail(email, code);

                    // Redirect to code verification page
                    return RedirectToAction("VerifyCode", new { email = email });
                }

                if (manager == null && employee == null && hr == null)
                {
                    ViewBag.ErrorMessage = "Invalid Email.";
                }

            }

            return View();
        }

        private string GenerateRandomCode()
        {
            var random = new Random();
            return random.Next(100000, 999999).ToString(); // 6-digit code
        }

        private void SendResetCodeEmail(string recipientEmail, string code)
        {
            using (SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", 587))
            {
                smtpClient.UseDefaultCredentials = false;
                smtpClient.EnableSsl = true;
                smtpClient.Credentials = new NetworkCredential("rahaf.alsmairat@gmail.com", "louq hewz zfsx cmez");

                MailMessage mailMessage = new MailMessage
                {
                    From = new MailAddress("rahaf.alsmairat@gmail.com"),
                    Subject = "Password Reset Code",
                    Body = $"Your password reset code is: {code}",
                    IsBodyHtml = false
                };

                mailMessage.To.Add(recipientEmail);
                smtpClient.Send(mailMessage);
            }


        }








            [HttpGet]
            public IActionResult VerifyCode(string email)
            {
                ViewBag.Email = email;
                return View();
            }

            [HttpPost]
            public IActionResult VerifyCode(string email, string code)
            {
                if (!string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(code))
                {
                    if (_resetCodes.TryGetValue(email, out var storedCode) && storedCode == code)
                    {
                        // Code is valid, redirect to reset password page
                        return RedirectToAction("ResetPassword", new { email = email });
                    }

                    ViewBag.ErrorMessage = "Invalid code.";
                }

                ViewBag.Email = email;
                return View();
            }





















        [HttpGet]
        public IActionResult ResetPassword(string email)
        {
            ViewBag.Email = email;
            return View();
        }

        [HttpPost]
        public IActionResult ResetPassword(string email, string newPassword, string confirmPassword)
        {
            if (!string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(newPassword) && newPassword == confirmPassword)
            {
                var manager = _context.Managers.FirstOrDefault(m => m.Email == email);
                var employee = _context.Employees.FirstOrDefault(m => m.Email == email);
                var hr = _context.Hrs.FirstOrDefault(m => m.Email == email);

                if (manager!=null)
                {
                    
                        // Hash the new password
                        manager.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);

                        // Save changes to the database
                        _context.Update(manager);
                        _context.SaveChanges();

                        // Remove the reset code from memory
                        _resetCodes.Remove(email);

                        // Redirect to confirmation page
                        return RedirectToAction("LoginManager", "HomePages");
                    
                }else if(employee!=null)
                {
                    // Hash the new password
                    employee.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);

                    // Save changes to the database
                    _context.Update(employee);
                    _context.SaveChanges();

                    // Remove the reset code from memory
                    _resetCodes.Remove(email);

                    // Redirect to confirmation page
                    return RedirectToAction("LoginEmp", "HomePages");
                }
                else if (hr!=null)
                {
                    // Hash the new password
                    hr.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);

                    // Save changes to the database
                    _context.Update(hr);
                    _context.SaveChanges();

                    // Remove the reset code from memory
                    _resetCodes.Remove(email);

                    // Redirect to confirmation page
                    return RedirectToAction("LoginHR", "HRDashboard");
                }
                
            }

            ViewBag.ErrorMessage = "Invalid input or passwords do not match.";
            ViewBag.Email = email;
            return View();
        }

        [HttpGet]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }


        //----------------------------------------------------------------------------------------------------------------------------------------
        //----------------------------------------------------------------------------------------------------------------------------------------
        //----------------------------------------------------------------------------------------------------------------------------------------
        //----------------------------------------------------------------------------------------------------------------------------------------
        //----------------------------------------------------------------------------------------------------------------------------------------


        public IActionResult ManagerLogin()
        {
            return View();
        }
        // POST: Manager Login
        [HttpPost]
        public IActionResult ManagerLogin(Manager manager)
        {
            var existingManager = _context.Managers
                .FirstOrDefault(x => x.Email == manager.Email && x.Password == manager.Password);

            if (existingManager != null)
            {
                // Store manager details in session or TempData if needed
                HttpContext.Session.SetInt32("ManagerId", existingManager.Id);

                // Redirect to the dashboard or manager's homepage
                return RedirectToAction("Dashboard", new
                {
                    id = existingManager.Id
                });
            }
            else
            {
                ModelState.AddModelError("", "Invalid email or password.");
                return View();
            }
        }

        public IActionResult Dashboard()
        {
            var managerID = HttpContext.Session.GetInt32("ManagerId");
            var manager = _context.Managers
                .Include(m => m.Employees) // Include employees related to the manager
                .FirstOrDefault(m => m.Id == managerID);

            if (manager == null)
            {
                return NotFound();
            }

            // Retrieve all employees assigned to this manager
            var employees = _context.Employees.Where(e => e.ManagerId == managerID).ToList();

            ViewBag.ManagerName = manager.Name;
            return View(employees);
        }
        public IActionResult ViewTasks(int id)
        {
            var tasks = _context.EmpTasks.Where(t => t.EmployeeId == id).ToList();

            if (tasks == null || tasks.Count == 0)
            {
                ViewBag.Message = "No tasks assigned to this employee.";
            }

            return View(tasks);
        }
        public IActionResult ViewAttendance(int id)
        {
            var attendanceRecords = _context.Attendances.Where(a => a.EmployeeId == id).ToList();

            if (attendanceRecords == null || attendanceRecords.Count == 0)
            {
                ViewBag.Message = "No attendance records found for this employee.";
            }

            return View(attendanceRecords);
        }
        public IActionResult ExportTasksAsPdf(int id)
        {
            var tasks = _context.EmpTasks.Where(t => t.EmployeeId == id).ToList();

            if (tasks == null || tasks.Count == 0)
            {
                ViewBag.Message = "No tasks assigned to this employee.";
                return View("ViewTasks", tasks);
            }

            return new ViewAsPdf("ExportTasksAsPdf", tasks)
            {
                FileName = "EmployeeTasks.pdf"
            };
        }
        public IActionResult ExportAttendanceAsPdf(int id)
        {
            var attendance = _context.Attendances.Where(a => a.EmployeeId == id).ToList();

            if (attendance == null || attendance.Count == 0)
            {
                ViewBag.Message = "No attendance records found for this employee.";
                return View("ViewAttendance", attendance);
            }

            return new ViewAsPdf("ExportAttendanceAsPdf", attendance)
            {
                FileName = "EmployeeAttendance.pdf"
            };
        }
        //public IActionResult Employee_evaluation( )
        //{
        //    return View();

        //}
        public IActionResult Employee_Evaluation(int id)
        {
            var employee = _context.Employees.FirstOrDefault(e => e.Id == id);

            if (employee == null)
            {
                return NotFound();
            }

            ViewBag.EmployeeId = id;
            ViewBag.ManagerId = HttpContext.Session.GetInt32("ManagerId");
            ViewBag.Questions = new List<string>
    {
        "How well does the employee complete tasks on time?",
        "How effective is the employee’s communication?",
        "How well does the employee work in a team?",
        "How proactive is the employee?",
        "How innovative is the employee?",
        "How well does the employee handle stress?",
        "How committed is the employee to the company’s goals?",
        "How well does the employee follow instructions?",
        "How punctual is the employee?",
        "How adaptable is the employee to new situations?"
    };

            return View();
        }
        [HttpPost]
        public IActionResult Submit_Evaluation(int EmployeeId, int ManagerId, List<int> Answers, string Comments)
        {
            if (Answers == null || Answers.Count < 10)
            {
                ModelState.AddModelError("", "Please answer all questions.");
                return RedirectToAction("Employee_Evaluation", new { id = EmployeeId });
            }

            // Calculate total score
            int totalScore = Answers.Sum();

            // Determine Status based on score
            string status;
            if (totalScore >= 40) status = "Excellent";
            else if (totalScore >= 30) status = "Good";
            else if (totalScore >= 20) status = "Average";
            else status = "Needs Improvement";

            // Save evaluation to the database
            var evaluation = new Evaluation
            {
                EmployeeId = EmployeeId,
                ManegerId = ManagerId,
                EvaluationDate = DateOnly.FromDateTime(DateTime.Now),
                Status = status
            };

            _context.Evaluations.Add(evaluation);
            _context.SaveChanges();

            return RedirectToAction("Dashboard", new { id = ManagerId });
        }


        public IActionResult ExportEmployeesAsPdf(int id)
        {
            var employees = _context.Employees
                .Where(e => e.ManagerId == id)
            .Select(e => new Employee
             {
                 Name = e.Name,
                 Email = e.Email,
                 Position = e.Position,
                 ProfileImage = e.ProfileImage
             })
        .ToList();

            //if (employees == null || employees.Count == 0)
            //{
            //    ViewBag.Message = "No employees found for this manager.";
            //    return View("ViewEmployees", employees);
            //}

            return new ViewAsPdf("ExportEmployeesAsPdf", employees)
            {
                FileName = "Manager_Employees.pdf"
            };
        }




        public IActionResult ManagerProfile()
        {
            return View();
        }

        public IActionResult ManagerEditProfile()
        {

            string userEmail = HttpContext.Session.GetString("UserEmail");
            var user = _context.Managers.FirstOrDefault(e => e.Email == userEmail);

            return View(user);
        }





        public IActionResult ManagerEditProfileHandle(Manager updatedEmployee, IFormFile profilePicture)
        {

            var userEmail = HttpContext.Session.GetString("UserEmail");
            var user = _context.Managers.FirstOrDefault(e => e.Email == userEmail);

            if (user != null)
            {
                user.Name = updatedEmployee.Name;
                user.Email = updatedEmployee.Email;
                //user.Position = updatedEmployee.Position;



                // Handle Profile Picture Upload
                if (profilePicture != null)
                {
                    string fileName = Path.GetFileName(profilePicture.FileName);
                    string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Img", fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        profilePicture.CopyTo(stream);
                    }

                    user.ProfileImage = "/Img/" + fileName;
                }

                _context.SaveChanges();
                HttpContext.Session.SetString("UserName", user.Name);
                HttpContext.Session.SetString("UserEmail", user.Email);
                HttpContext.Session.SetString("ProfileImage", user.ProfileImage);
                return RedirectToAction("ManagerProfile");

            }


            return View(user);
        }

        public IActionResult ManagerResetPassword()
        {

            return View();
        }

        //[HttpPost]
        //public IActionResult ManagerResetPasswordHandle(string oldPassword, string newPassword, string confirmPassword)
        //{
        //    if (newPassword != confirmPassword)
        //    {
        //        ViewBag.ErrorMessage = "Passwords do not match.";
        //        return View();
        //    }
        //    string userEmail = HttpContext.Session.GetString("UserEmail");
        //    var user = _context.Managers.FirstOrDefault(u => u.Email == userEmail);
        //    if (!user.Password.StartsWith("$2a$"))
        //    {

        //        user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
        //        _context.SaveChanges();
        //    }
        //    if (user != null)
        //    {

        //        if (BCrypt.Net.BCrypt.Verify(oldPassword, user.Password))
        //        {

        //            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(newPassword);


        //            user.Password = hashedPassword;
        //            _context.SaveChanges();
        //            return RedirectToAction(nameof(ManagerResetPassword));
        //        }
        //        else
        //        {

        //            ViewBag.ErrorMessage = "Incorrect old password.";
        //            return RedirectToAction(nameof(ManagerResetPassword));

        //        }
        //    }
        //    else
        //    {

        //        ViewBag.ErrorMessage = "User not found.";
        //        return RedirectToAction(nameof(ManagerResetPassword));

        //    }

        //}

        [HttpPost]
        public IActionResult ManagerResetPasswordHandle(string oldPassword, string newPassword, string confirmPassword)
        {
            if (newPassword != confirmPassword)
            {
                ViewBag.ErrorMessage = "Passwords do not match.";
                return View("ManagerResetPassword");
            }
            string userEmail = HttpContext.Session.GetString("UserEmail");
            var user = _context.Managers.FirstOrDefault(u => u.Email == userEmail);

            if (user != null)
            {

                if (BCrypt.Net.BCrypt.Verify(oldPassword, user.Password))
                {

                    string hashedPassword = BCrypt.Net.BCrypt.HashPassword(newPassword);


                    user.Password = hashedPassword;
                    _context.SaveChanges();
                    return RedirectToAction("ManagerProfile");
                }
                else
                {

                    ViewBag.ErrorMessage = "Incorrect old password.";
                    return View("ManagerResetPassword");
                }
            }
            else
            {

                ViewBag.ErrorMessage = "User not found.";
                return View("ManagerResetPassword");
            }

        }


        public IActionResult Logout()
        {

            HttpContext.Session.Clear();
            return RedirectToAction("Index", "HomePages");
        }





    }
}
