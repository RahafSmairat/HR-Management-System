using Project_6.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;

namespace HR_Managment_System.Controllers
{
    public class EmployeeController : Controller
    {




        private readonly MyDbContext _context;
        private static bool IsPunchedIn = false;//--------------

        public EmployeeController(MyDbContext context)
        {
            _context = context;
        }
        //----------------------------------------------------
        public IActionResult PunchIn()
        {
            IsPunchedIn = true;
            return RedirectToAction("AddNewAtt");
        }

        public IActionResult PunchOut()
        {
            IsPunchedIn = false;
            return RedirectToAction("UpdateNewAtt");
        }


        //----------------------------------------------------
        public IActionResult Index()
        {


            ViewBag.IsPunchedIn = IsPunchedIn;
            var AllAtend = _context.Attendances.ToList();

            if(AllAtend ==  null)
            {
                return NotFound();
            }

            return View(AllAtend);
        }

        public IActionResult AddNewAtt()
        {
            var NewAtt = new Attendance
            {
                EmployeeId = HttpContext.Session.GetInt32("Id"),
                Date = DateOnly.FromDateTime(DateTime.Now),
                PunchInTime = TimeOnly.FromDateTime(DateTime.Now)

            };

            _context.Attendances.Add(NewAtt);
            _context.SaveChanges();        

            return RedirectToAction("Index");
        }

        public IActionResult UpdateNewAtt()
        {
            var UpdateAtt = _context.Attendances
            .OrderByDescending(a => a.Id)
            .FirstOrDefault();

            if (UpdateAtt == null)
            {
                return RedirectToAction("Index");
            }

            if (UpdateAtt.PunchInTime.HasValue)
            {
      
                UpdateAtt.PunchOutTime = TimeOnly.FromDateTime(DateTime.Now);
            }

            _context.Attendances.Update(UpdateAtt);
            _context.SaveChanges();
          

            return RedirectToAction("Index");


        }


        public IActionResult MyLeaves()
        {

            var AllLeaves = _context.RequestLeaves.ToList();

            if (AllLeaves == null)
            {
                return NotFound();
            }

            return View(AllLeaves);
        }


        [HttpPost]
        public IActionResult leaveFormModal(string LeaveReason, DateTime StartDate_1, 
            DateTime EndDate_1, TimeSpan StartTime_1, TimeSpan EndTime_1 , string RequestDescription_1)
        {
            if (ModelState.IsValid)
            {

                var leaveRequest = new RequestLeave
                {
                    EmployeeId = HttpContext.Session.GetInt32("Id"),
                    RequestDate = DateOnly.FromDateTime(DateTime.Now),
                    StartDate = DateOnly.FromDateTime(StartDate_1), 
                    EndDate = DateOnly.FromDateTime(EndDate_1), 
                    Status = "Pending",
                    RequestName = LeaveReason,
                    RequestDescription = RequestDescription_1,
                    StartTime = TimeOnly.FromDateTime(StartDate_1.Add(StartTime_1)),
                    EndTime = TimeOnly.FromDateTime(EndDate_1.Add(EndTime_1))         
                };

       
                _context.Add(leaveRequest);
                _context.SaveChanges();

                return RedirectToAction("MyLeaves");
            }

            return RedirectToAction("MyLeaves");
        }


        [HttpPost]
        public IActionResult VacationFormModal(string LeaveReason, DateTime StartDate_1,
           DateTime EndDate_1)
        {
            if (ModelState.IsValid)
            {

                var leaveRequest = new RequestLeave
                {
                    EmployeeId = HttpContext.Session.GetInt32("Id"),
                    RequestDate = DateOnly.FromDateTime(DateTime.Now),
                    StartDate = DateOnly.FromDateTime(StartDate_1),
                    EndDate = DateOnly.FromDateTime(EndDate_1),
                    Status = "Pending",
                    RequestName = LeaveReason,
                    RequestDescription = "N/A",
                };


                _context.Add(leaveRequest);
                _context.SaveChanges();

                return RedirectToAction("MyLeaves");
            }

            return RedirectToAction("MyLeaves");
        }


        public async Task<IActionResult> EmployeeTasks()
        {
            

            int? employeeId = HttpContext.Session.GetInt32("Id");

            var tasks = await _context.EmpTasks.Where(t => t.EmployeeId == employeeId).ToListAsync();

            return View(tasks);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateTasks(Dictionary<int, EmpTask> updatedTasks)
        {
            if (updatedTasks != null)
            {
                foreach (var updatedTask in updatedTasks.Values)
                {
                    var task = await _context.EmpTasks.FindAsync(updatedTask.Id);
                    if (task != null)
                    {
                        task.Status = updatedTask.Status;
                    }
                }
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("EmployeeTasks");
        }


        public IActionResult Profile()
        {
            //int? employeeId = HttpContext.Session.GetInt32("EmployeeID");

            int? employeeId = HttpContext.Session.GetInt32("Id");

            var tasks = _context.Employees.FirstOrDefault(t => t.Id == employeeId);

            return View(tasks);
        }
        public IActionResult EditProfile()
        {

            string userEmail = HttpContext.Session.GetString("UserEmail");
            var user = _context.Employees.FirstOrDefault(e => e.Email == userEmail);

            return View(user);
        }

        public IActionResult EditProfileHandle(Employee updatedEmployee, IFormFile profilePicture)
        {

            var userEmail = HttpContext.Session.GetString("UserEmail");
            var user = _context.Employees.FirstOrDefault(e => e.Email == userEmail);
            if (user != null)
            {
                user.Name = updatedEmployee.Name;
                user.Email = updatedEmployee.Email;
                user.Position = updatedEmployee.Position;



                // Handle Profile Picture Upload
                if (profilePicture != null)
                {
                    string fileName = Path.GetFileName(profilePicture.FileName);
                    string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img", fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        profilePicture.CopyTo(stream);
                    }

                    user.ProfileImage = fileName;
                }

                _context.SaveChanges();
                HttpContext.Session.SetString("UserName", user.Name);
                HttpContext.Session.SetString("UserEmail", user.Email);
                HttpContext.Session.SetString("ProfileImage", user.ProfileImage);
                HttpContext.Session.SetString("Position", user.Position);
                return RedirectToAction("Profile");

            }


            return View(user);
        }











        public IActionResult ResetPassword()
        {

            return View();
        }



        //[HttpPost]
        //public IActionResult ResetPasswordHandle(string oldPassword, string newPassword, string confirmPassword)
        //{

        //    if (newPassword != confirmPassword)
        //    {
        //        ViewBag.ErrorMessage = "Passwords do not match.";
        //        return RedirectToAction(nameof(ResetPassword));
        //    }

        //    string userEmail = HttpContext.Session.GetString("UserEmail");
        //    var user = _context.Employees.FirstOrDefault(u => u.Email == userEmail);
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
        //            return RedirectToAction("Profile");
        //        }
        //        else
        //        {

        //            ViewBag.ErrorMessage = "Incorrect old password.";
        //            return RedirectToAction(nameof(ResetPassword));

        //        }
        //    }
        //    else
        //    {

        //        ViewBag.ErrorMessage = "User not found.";
        //        return RedirectToAction(nameof(ResetPassword));

        //    }

        //}


        [HttpPost]
        public IActionResult ResetPasswordHandle(string oldPassword, string newPassword, string confirmPassword)
        {
            if (newPassword != confirmPassword)
            {
                ViewBag.ErrorMessage = "Passwords do not match.";
                return View("ResetPassword");
            }

            string userEmail = HttpContext.Session.GetString("UserEmail");
            var user = _context.Employees.FirstOrDefault(u => u.Email == userEmail);

            if (user == null)
            {
                ViewBag.ErrorMessage = "User not found.";
                return View("ResetPassword");
            }

            // Verify old password (assuming it's already hashed)
            if (!BCrypt.Net.BCrypt.Verify(oldPassword, user.Password))
            {
                ViewBag.ErrorMessage = "Incorrect old password.";
                return View("ResetPassword");
            }

            // Hash new password and update
            user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
            _context.SaveChanges();

            return RedirectToAction("Profile");

        }


        public IActionResult Logout()
        {

            HttpContext.Session.Clear();
            return RedirectToAction("Index", "HomePages");

        }



    }
}

