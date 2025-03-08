using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Project_6.Models;
using BCrypt.Net;
using Microsoft.EntityFrameworkCore;

namespace Project_6.Controllers
{
    public class HRDashboardController : Controller
    {
        private readonly MyDbContext _context;


        public HRDashboardController(MyDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }

        // department 

        public IActionResult Department()
        {
            var departement = _context.Departments.ToList();
            return View(departement);
        }


        public IActionResult CreateDepartment()
        {
            ViewData["ManagerId"] = new SelectList(_context.Managers, "Id", "Name");
            return View();
        }

        [HttpPost]
        public IActionResult CreateDepartment(Department department, IFormFile image)
        {
            if (image != null)
            {
                string fileName = Path.GetFileName(image.FileName);
                string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/HR_image", fileName);

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    image.CopyTo(stream);
                }
                department.Image = fileName;
            }
            if (ModelState.IsValid)
            {
                department.Id = 0;
                _context.Departments.Add(department);
                _context.SaveChanges();
                return RedirectToAction("Department", "HRDashboard");
            }
            return View();
        }

        [HttpPost]
        public IActionResult DeleteDepartment(int id)
        {
            var departement = _context.Departments.Find(id);
            _context.Departments.Remove(departement);
            _context.SaveChanges();
            return RedirectToAction("Department", "HRDashboard");
        }

        // manager 


        public IActionResult Manager()
        {
            var manager = _context.Managers.ToList();
            return View(manager);
        }

        public IActionResult CreateManager()
        {
            return View();
        }

        [HttpPost]
        public IActionResult CreateManager(Manager manager, IFormFile profileImage)
        {
            if (profileImage != null)
            {
                string fileName = Path.GetFileName(profileImage.FileName);
                string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/HR_image", fileName);

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    profileImage.CopyTo(stream);
                }
                manager.ProfileImage = fileName;
            }
            if (ModelState.IsValid)
            {
                manager.Id = 0;

                manager.Password = BCrypt.Net.BCrypt.HashPassword(manager.Password);
                _context.Managers.Add(manager);
                _context.SaveChanges();
                return RedirectToAction("Manager", "HRDashboard");
            }
            return View();

        }

        public IActionResult Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var manager = _context.Managers
                .FirstOrDefault(m => m.Id == id);
            if (manager == null)
            {
                return NotFound();
            }

            return View(manager);
        }

        [HttpPost]
        public IActionResult DeleteManager(int id)
        {
            var manager = _context.Managers.Find(id);
            _context.Managers.Remove(manager);
            _context.SaveChanges();
            return RedirectToAction("Manager", "HRDashboard");
        }




        // employee 
        public IActionResult IndexEmployee()
        {
            return View();
        }



        public IActionResult Employee()
        {
            var employee = _context.Employees.Include(u => u.Department).ToList();
            return View(employee);
        }


        public IActionResult EmployeeDetails(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = _context.Employees.Include(u => u.Department)
                .FirstOrDefault(m => m.Id == id);
            if (employee == null)
            {
                return NotFound();
            }

            return View(employee);
        }


        [HttpGet]
        public IActionResult Approvment()
        {
            var request = _context.RequestLeaves.Where(E => E.Status == "Approve").Include(e => e.Employee).ToList();
            return View(request);
        }


        public IActionResult Evaluation()
        {
            var evaluations = _context.Evaluations
                                    .Include(e => e.Employee)  // Include the Employee entity
                                    .ToList();

            return View(evaluations);
        }
        public IActionResult feedback()
        {
            var feedback = _context.Contacts.ToList();
            return View(feedback);
        }


        // login 


        [HttpGet]
        public IActionResult LoginHR()
        {
            return View();
        }

        [HttpPost]
        public IActionResult LoginHR(Hr hr)
        {
            if (string.IsNullOrWhiteSpace(hr.Email) || string.IsNullOrWhiteSpace(hr.Password))
            {
                ModelState.AddModelError("", "Email and Password are required.");
                return View(hr);
            }


            var HRInfo = _context.Hrs.FirstOrDefault(e => e.Email == hr.Email);

            if (HRInfo != null)
            {
                string password = hr.Password;

                // The stored hash
                string storedHash = HRInfo.Password;


                if (BCrypt.Net.BCrypt.Verify(hr.Password, HRInfo.Password))
                {

                    HttpContext.Session.SetString("Id", HRInfo.Id.ToString());
                    HttpContext.Session.SetString("UserName", HRInfo.Name);
                    HttpContext.Session.SetString("UserEmail", HRInfo.Email);
                    HttpContext.Session.SetString("UserPassword", HRInfo.Password);
                    HttpContext.Session.SetString("ProfileImage", HRInfo.ProfileImage);


                    return RedirectToAction("Index");
                }
                else
                {

                    ModelState.AddModelError("", "Email Or Password not correct.");
                    return View(hr);
                }
            }


            ModelState.AddModelError("", "Email Or Password not correct.");
            return View(hr);
        }


        public IActionResult HrProfile()
        {

            return View();
        }

        public IActionResult HrEditProfile()
        {

            string userEmail = HttpContext.Session.GetString("UserEmail");
            var user = _context.Hrs.FirstOrDefault(e => e.Email == userEmail);

            return View(user);
        }



        public IActionResult HrEditProfileHandle(Hr updatedEmployee, IFormFile profilePicture)
        {

            var userEmail = HttpContext.Session.GetString("UserEmail");
            var user = _context.Hrs.FirstOrDefault(e => e.Email == userEmail);
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
                return RedirectToAction("HrProfile");

            }


            return View(user);
        }


        public IActionResult HrResetPassword()
        {

            return View();
        }

        //[HttpPost]
        //public IActionResult HrResetPasswordHandle(string oldPassword, string newPassword, string confirmPassword)
        //{
        //    if (newPassword != confirmPassword)
        //    {
        //        ViewBag.ErrorMessage = "Passwords do not match.";
        //        return View();
        //    }
        //    string userEmail = HttpContext.Session.GetString("UserEmail");
        //    var user = _context.Hrs.FirstOrDefault(u => u.Email == userEmail);
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
        //            return RedirectToAction("HrProfile");
        //        }
        //        else
        //        {

        //            ViewBag.ErrorMessage = "Incorrect old password.";
        //            return View();
        //        }
        //    }
        //    else
        //    {

        //        ViewBag.ErrorMessage = "User not found.";
        //        return View();
        //    }

        //}

        [HttpPost]
        public IActionResult HrResetPasswordHandle(string oldPassword, string newPassword, string confirmPassword)
        {
            if (newPassword != confirmPassword)
            {
                ViewBag.ErrorMessage = "Passwords do not match.";
                return View("HrResetPassword");
            }
            string userEmail = HttpContext.Session.GetString("UserEmail");
            var user = _context.Hrs.FirstOrDefault(u => u.Email == userEmail);

            if (user != null)
            {

                if (BCrypt.Net.BCrypt.Verify(oldPassword, user.Password))
                {

                    string hashedPassword = BCrypt.Net.BCrypt.HashPassword(newPassword);


                    user.Password = hashedPassword;
                    _context.SaveChanges();
                    return RedirectToAction("HrProfile");
                }
                else
                {

                    ViewBag.ErrorMessage = "Incorrect old password.";
                    return View("HrResetPassword");
                }
            }
            else
            {

                ViewBag.ErrorMessage = "User not found.";
                return View("HrResetPassword");
            }

        }
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "HomePages");
        }

        //------------------------------------------------------------------------------------
        public IActionResult Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var department = _context.Departments.Find(id);
            if (department == null)
            {
                return NotFound();
            }
            ViewBag.DepName = department.Name;
            ViewBag.DepDes = department.Description;
            ViewBag.DepImage = department.Image;
            ViewData["ManagerId"] = new SelectList(_context.Managers, "Id", "Name");
            return View(department);
        }



        [HttpPost]
        public IActionResult Edit(Department department, IFormFile image)
        {
            var dep = _context.Departments.Find(department.Id);

            if (image != null)
            {
                string fileName = Path.GetFileName(image.FileName);
                string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/HR_image", fileName);

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    image.CopyTo(stream);
                }
                dep.Image = fileName;
            }
            if (ModelState.IsValid)
            {
                dep.Name = department.Name;
                dep.Description = department.Description;
                _context.Departments.Update(dep);
                _context.SaveChanges();
                return RedirectToAction("Department", "HRDashboard");
            }
            return View();
        }






















    }
}
