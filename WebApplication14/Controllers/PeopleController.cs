using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using WebApplication14.Models;

namespace WebApplication14.Controllers
{
    public class PeopleController : Controller
    {
        private user_login_and_singupEntities db = new user_login_and_singupEntities();

        // GET: People
        public ActionResult Index()
        {
            return View(db.People.ToList());
        }

        // GET: People/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Person person = db.People.Find(id);
            if (person == null)
            {
                return HttpNotFound();
            }
            return View(person);
        }

        // GET: People/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: People/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Personid,LastName,FirstName,email,password,Age,imag")] Person person)
        {
            if (ModelState.IsValid)
            {
                db.People.Add(person);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(person);
        }

        // GET: People/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Person person = db.People.Find(id);
            if (person == null)
            {
                return HttpNotFound();
            }
            return View(person);
        }

        // POST: People/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Personid,LastName,FirstName,email,password,Age,imag")] Person person)
        {
            if (ModelState.IsValid)
            {
                db.Entry(person).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(person);
        }

        // GET: People/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Person person = db.People.Find(id);
            if (person == null)
            {
                return HttpNotFound();
            }
            return View(person);
        }

        // POST: People/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Person person = db.People.Find(id);
            db.People.Remove(person);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        public ActionResult Login()
        {
            return View();
        }

        // POST: People/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        // POST: People/Login
       
        public ActionResult Login(string email, string password)
        {
            var user = db.People.FirstOrDefault(u => u.email == email && u.password == password);
            if (user != null)
            {
                Session["Email"] = email; // Simple session management
                return RedirectToAction("Index", "Home");
            }
            ModelState.AddModelError("", "Invalid email or password");
            return View();
        }


        // GET: People/Register
        public ActionResult Register()
        {
            return View();
        }

        // POST: People/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        
        public ActionResult Register(Person person, HttpPostedFileBase ImageFile)
        {
            if (ModelState.IsValid)
            {
                // معالجة رفع الصورة
                if (ImageFile != null && ImageFile.ContentLength > 0)
                {
                    var fileName = Path.GetFileName(ImageFile.FileName);
                    var path = Path.Combine(Server.MapPath("~/Images/"), fileName);
                    ImageFile.SaveAs(path);
                    person.imag = "/Images/" + fileName; // حفظ URL الصورة في قاعدة البيانات
                }

                db.People.Add(person);
                db.SaveChanges();
                return RedirectToAction("Login");
            }

            return View(person);
        }


        // GET: People/Profile
        // GET: People/Profile
        public ActionResult Profile()
        {
            if (Session["Email"] == null)
            {
                return RedirectToAction("Login");
            }

            var email = Session["Email"].ToString();
            var user = db.People.FirstOrDefault(u => u.email == email);

            if (user == null)
            {
                return HttpNotFound();
            }

            var model = new Person
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                email = user.email,
                imag = user.imag,
                Age = user.Age,
            };

            return View(model);
        }

        // GET: People/EditProfile
        public ActionResult EditProfile()
        {
            if (Session["Email"] == null)
            {
                return RedirectToAction("Login");
            }

            var email = Session["Email"].ToString();
            var user = db.People.FirstOrDefault(u => u.email == email);

            if (user == null)
            {
                return HttpNotFound();
            }

            var model = new Person
            {
                Personid = user.Personid,  // Ensure Personid is included
                FirstName = user.FirstName,
                LastName = user.LastName,
                email = user.email,
                imag = user.imag,
                Age = user.Age,
            };

            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditProfile(Person person, HttpPostedFileBase ImageFile)
        {
            if (Session["Email"] == null)
            {
                return RedirectToAction("Login");
            }

            var email = Session["Email"].ToString();
            var user = db.People.FirstOrDefault(u => u.email == email);

            if (user == null)
            {
                return HttpNotFound();
            }

            // Update user details
            user.FirstName = person.FirstName;
            user.LastName = person.LastName;
            user.email = person.email;
            user.Age = person.Age;

            // Handle image upload
            if (ImageFile != null && ImageFile.ContentLength > 0)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var extension = Path.GetExtension(ImageFile.FileName).ToLower();

                if (!allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError("", "Only image files are allowed.");
                    return View(person);
                }

                if (ImageFile.ContentLength > 1048576) // 1MB
                {
                    ModelState.AddModelError("", "File size cannot exceed 1MB.");
                    return View(person);
                }

                var fileName = Path.GetFileName(ImageFile.FileName);
                var path = Path.Combine(Server.MapPath("~/Images/"), fileName);
                ImageFile.SaveAs(path);
                user.imag = "/Images/" + fileName;
            }

            try
            {
                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Profile");
            }
            catch (DbEntityValidationException ex)
            {
                foreach (var validationErrors in ex.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        ModelState.AddModelError("", $"{validationErrors.Entry.Entity.GetType().Name}: {validationError.ErrorMessage}");
                    }
                }
                return View(person);
            }
        }


        

       

        // GET: People/ResetPassword
        public ActionResult ResetPassword()
        {
            return View();
        }

        // POST: People/ResetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        // POST: People/ResetPassword
        public ActionResult ResetPassword(string oldPassword, string newPassword, string confirmPassword)
        {
            if (Session["Email"] == null)
            {
                return RedirectToAction("Login");
            }

            var email = Session["Email"].ToString();
            var user = db.People.FirstOrDefault(u => u.email == email);

            if (user == null || user.password != oldPassword)
            {
                ModelState.AddModelError("", "Invalid password");
                return View();
            }

            if (newPassword != confirmPassword)
            {
                ModelState.AddModelError("", "New password and confirmation password do not match");
                return View();
            }

            user.password = newPassword;
            db.Entry(user).State = EntityState.Modified;
            db.SaveChanges();

            return RedirectToAction("Profile");
        }
        // GET: People/Logout
        public ActionResult Logout()
        {
            Session.Clear(); // Clear the session
            return RedirectToAction("Index", "Home");
        }


    }
}
