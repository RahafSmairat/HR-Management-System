using Microsoft.AspNetCore.Mvc;
using Project_6.Models;


namespace Project_6.Controllers
{

    public class HomePagesController : Controller
    {
        private readonly MyDbContext _context;

        public HomePagesController(MyDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {

            return View();
        }

        public IActionResult About()
        {
            var department = _context.Departments.ToList();
            return View(department);
        }

        public IActionResult Contact()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Contact(Contact contact)
        {
            if (ModelState.IsValid)
            {
                contact.SubmissionDate = DateOnly.FromDateTime(DateTime.UtcNow);
                _context.Contacts.Add(contact);
                await _context.SaveChangesAsync();

                return View();
            }

            return View("Contact", contact);
        }

        public IActionResult LoginManager()
        {
            return View();
        }

        [HttpPost]
        public IActionResult LoginManager(Manager manager)
        {
            if (string.IsNullOrWhiteSpace(manager.Email) || string.IsNullOrWhiteSpace(manager.Password))
            {
                ModelState.AddModelError("", "Email and Password are required.");
                return View(manager);
            }


            var managerInfo = _context.Managers.FirstOrDefault(e => e.Email == manager.Email);

            if (managerInfo != null)
            {
                string password = manager.Password;
                string maagerInfoPassword = managerInfo.Password;

                if (BCrypt.Net.BCrypt.Verify(password, maagerInfoPassword))
                {

                    HttpContext.Session.SetInt32("Id", managerInfo.Id);
                    HttpContext.Session.SetString("UserName", managerInfo.Name);
                    HttpContext.Session.SetString("UserEmail", managerInfo.Email);
                    HttpContext.Session.SetString("UserPassword", managerInfo.Password);
                    HttpContext.Session.SetString("ProfileImage", managerInfo.ProfileImage);


                    // Store manager details in session or TempData if needed
                    HttpContext.Session.SetInt32("ManagerId", managerInfo.Id);
                    TempData["ManagerId"] = managerInfo.Id;

                    // Redirect to the dashboard or manager's homepage
                    return RedirectToAction("Dashboard", "Managers", new
                    {
                        id = managerInfo.Id
                    });
                }
                else
                {

                    ModelState.AddModelError("", "Email Or Password not correct.");
                    return View(manager);
                }
            }
            return View(manager);


        }













        public IActionResult LoginEmp()
        {
            return View();

        }
        [HttpPost]
        public IActionResult LoginEmp(Employee employee)
        {
            if (string.IsNullOrWhiteSpace(employee.Email) || string.IsNullOrWhiteSpace(employee.Password))
            {
                ModelState.AddModelError("", "Email and Password are required.");
                return View(employee);
            }


            var employeeInfo = _context.Employees.FirstOrDefault(e => e.Email == employee.Email);
            if (employeeInfo != null)
            {
                string password = employee.Password;
                //string maagerInfoPassword = employeeInfo.Password;
                if (BCrypt.Net.BCrypt.Verify(password, employeeInfo.Password))
                {

                    HttpContext.Session.SetInt32("Id", employeeInfo.Id);
                    HttpContext.Session.SetString("UserName", employeeInfo.Name);
                    HttpContext.Session.SetString("UserEmail", employeeInfo.Email);
                    HttpContext.Session.SetString("UserPassword", employeeInfo.Password);
                    HttpContext.Session.SetString("ProfileImage", employeeInfo.ProfileImage);
                    HttpContext.Session.SetInt32("DepartmentId", employeeInfo.DepartmentId.GetValueOrDefault());
                    HttpContext.Session.SetInt32("ManagerId", employeeInfo.ManagerId.GetValueOrDefault());
                    HttpContext.Session.SetString("Position", employeeInfo.Position);

                    return RedirectToAction("Index", "Employee");
                }
                else
                {

                    ModelState.AddModelError("", "Email Or Password not correct.");
                    return View(employee);
                }
            }


            ModelState.AddModelError("", "Email Or Password not correct.");
            return View(employee);
        }






    }


}

