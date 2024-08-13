using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Kavenegar;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using
    Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using test.Models;

public class AuthController : Controller
{

    private readonly Context db;
    private readonly string salt = "S@lt?";

    private readonly IWebHostEnvironment _env;

    public AuthController(Context _db, IWebHostEnvironment env)
    {
        db = _db;
        _env = env;
    }

    [HttpGet]
    public IActionResult login()
    {
        if (User.Identity.IsAuthenticated)
        {
            return RedirectToAction("index", "email");
        }
        if (db.Role_tbl.Count() == 0)
        {
            db.Role_tbl.Add(new Role { Name = "admin" });
            db.Role_tbl.Add(new Role { Name = "client" });
            db.SaveChanges();
      
            db.Permission_tbl.Add(new Permission { Name = "Read" });
            db.Permission_tbl.Add(new Permission { Name = "ReadAll" });
            db.Permission_tbl.Add(new Permission { Name = "Delete" });
            db.Permission_tbl.Add(new Permission { Name = "Sent" });
            db.SaveChanges();
      
            db
                .RolePermissions_tbl
                .Add(new RolePermission { RoleId = 1, PermissionId = 1 });
            db
                .RolePermissions_tbl
                .Add(new RolePermission { RoleId = 1, PermissionId = 2 });
            db
                .RolePermissions_tbl
                .Add(new RolePermission { RoleId = 1, PermissionId = 3 });
            db
                .RolePermissions_tbl
                .Add(new RolePermission { RoleId = 1, PermissionId = 4 });
            db
                .RolePermissions_tbl
                .Add(new RolePermission { RoleId = 2, PermissionId = 1 });
            db
                .RolePermissions_tbl
                .Add(new RolePermission { RoleId = 2, PermissionId = 3 });
            db
                .RolePermissions_tbl
                .Add(new RolePermission { RoleId = 2, PermissionId = 4 });
            db.SaveChanges();
            db.smsTokens
                .Add(new smsToken {
                    Token = "3871353043697339486A70384F544A4A574C74612B51432F4C7A4B305076645457396F5267456F7A5A34383D"
                });
            db.Users_tbl.Add(new Users{
                    Username = "admin",
                    Password = "$11$yHU5vPKdGj617fT1LTjHH.58iUZAzyCw.55KqqrYOopvJCebMCVyq",
                    FirstName = "admin",
                    LastName = "admin",
                    Phone = "admin",
                    Addres = "admin",
                    NatinalCode = "admin",
                    PerconalCode = "admin",
                    Profile = "admin",
                    CreateDateTime = DateTime.UtcNow,
                    Token = "null"
            });
            db.SaveChanges();
        }
        return View();
    }

    [HttpPost]
    public IActionResult login(string Username, string Password)
    {
        Users check =
            db.Users_tbl.FirstOrDefault(x => x.Username == Username.ToLower());
        if (check == null)
        {
            ViewBag.Error = ("1اطلاعات وارد شده درست نیست");
        }
        else if (
            !BCrypt
                .Net
                .BCrypt
                .Verify(Password + salt + Username.ToLower(), check.Password)
        )
        {
            CreateUserLog((int) check.Id, 1, false);
            ViewBag.Error = ("اطلاعات وارد شده درست نیست");
        }
        else
        {
            CreateUserLog((int) check.Id, 1, true);
            ClaimsIdentity Identity =
                new ClaimsIdentity(new []
                    {
                        new Claim(ClaimTypes.Name,
                            check.FirstName + " " + check.LastName),
                        new Claim(ClaimTypes.NameIdentifier,
                            check.Id.ToString()),
                        new Claim("Profile", check.Profile),
                        new Claim("Username", check.Username)
                    },
                    CookieAuthenticationDefaults.AuthenticationScheme);

            var princpal = new ClaimsPrincipal(Identity);

            var properties =
                new AuthenticationProperties {
                    ExpiresUtc = DateTime.UtcNow.AddMonths(1),
                    IsPersistent = true
                };

            HttpContext.SignInAsync (princpal, properties);

            if(Username.ToLower() == "admin"){
                return RedirectToAction("ReportSeen", "home", new { area = "admin" });
            }

            return RedirectToAction("index", "email");
        }
        return View();
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> RegisterAsync(DtodUser user)
    {
        string PathSave;

        Users check =
            db
                .Users_tbl
                .FirstOrDefault(x =>
                    x.Username == user.Username ||
                    x.NatinalCode == user.NatinalCode ||
                    x.Phone == user.Phone);
        if (check != null)
        {
            if (check.Username == user.Username.ToLower())
            {
                ViewBag.Error = ("کاربر وارد شده تکراری است");
                return View();
            }
            else if (check.NatinalCode == user.NatinalCode)
            {
                ViewBag.Error = ("کد ملی وارد شده تکراری است");
                return View();
            }
            else if (check.Phone == user.Phone)
            {
                ViewBag.Error = ("شماره تلفن وارد شده  تکراری است");
                return View();
            }
            else
            {
                ViewBag.Error = "مشکلی پیش امده است ، با پشتیبانی تماس بگیرید";
                return View();
            }
        }
        else
        {
            string FileExtension = Path.GetExtension(user.Profile.FileName);
            var NewFileName =
                String.Concat(Guid.NewGuid().ToString(), FileExtension);
            var path = $"{_env.WebRootPath}\\uploads\\{NewFileName}";
            PathSave = $"\\uploads\\{NewFileName}";
            using (var stream = new FileStream(path, FileMode.Create))
            {
                await user.Profile.CopyToAsync(stream);
            }

            var NewUser =
                new Users {
                    Username = user.Username.ToLower(),
                    Password =
                        BCrypt
                            .Net
                            .BCrypt
                            .HashPassword(user.Password +
                            salt +
                            user.Username.ToLower()),
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Phone = user.Phone,
                    Addres = user.Addres,
                    NatinalCode = user.NatinalCode,
                    PerconalCode = user.PerconalCode,
                    Profile = PathSave,
                    CreateDateTime = DateTime.UtcNow,
                    Token = "null"
                };
            db.Users_tbl.Add (NewUser);
            db.SaveChanges();

            var RoleCilentId =
                db
                    .Role_tbl
                    .Where(x => x.Name == "client")
                    .Select(x => x.Id)
                    .FirstOrDefault();

            db
                .UserRoles_tbl
                .Add(new UserRole {
                    UserId = (int) NewUser.Id,
                    RoleId = RoleCilentId
                });
            db.SaveChanges();

            CreateUserLog((int) NewUser.Id, 3, true);
            CreateUserLog((int) NewUser.Id, 7, true);

            ViewBag.Result = "ثبت نام با موفقیت انجام شد ";
            return View("login");
        }
    }

    [HttpGet]
    public IActionResult Forget()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Forget(string Username, string NatinalCode)
    {
        Users check =
            db
                .Users_tbl
                .FirstOrDefault(x =>
                    x.Username == Username.ToLower() &&
                    x.NatinalCode == NatinalCode);
        if (check == null)
        {
            ViewBag.Error = "اطلاعات وارد شده نادرست میباشد";
            return View("Forget");
        }

        // sms check
        smsUser request = db.sms_tbl.FirstOrDefault(x => x.UserId == check.Id);

        if (request != null)
        {
            if (request.IsValid = false)
            {
                CreateUserLog((int) check.Id, 4, false);

                //return Ok("you Must Wait about 10 min");
                ViewBag.Error =
                    "تعداد تلاش شما برای ورود بیشتر از 5 میباشد . شما مجاز به ورود تا 10 دقیقه دیگر نیستید ، مجددا تلاش کنید";
                return View("Forget");
            }
            else if (DateTime.UtcNow.AddMinutes(-10) < request.CreateDateTime)
            {
                CreateUserLog((int) check.Id, 4, false);

                //return Ok("you Must Wait about 10 min");
                ViewBag.Error =
                    "شما باید 10 دقیقه برای دریافت کد جدید صبر کنید";
                return View("forget");
            }
            else
            {
                db.sms_tbl.Remove (request);
            }
        }
        Random random = new Random();
        smsUser newSms =
            new smsUser {
                TryCount = 0,
                SmsCode = random.Next(100000, 999999).ToString(),
                UserId = (int) check.Id,
                IsValid = true,
                CreateDateTime = DateTime.UtcNow
            };
        db.sms_tbl.Add (newSms);
        db.SaveChanges();

        CreateUserLog((int) check.Id, 4, true);

        SmsCode(newSms.SmsCode, check.Phone);
        ViewBag.smsPhone =
            check.Phone.Substring(check.Phone.Count() - 4) + "*****09";
        ViewBag.userId = check.Id;
        return View("Verify");
    }

    [HttpPost]
    public IActionResult Verify(int userid, string otp)
    {
        Users check = db.Users_tbl.Find(userid);
        if (check == null)
        {
            ViewBag.Error = "کاربر یافت نشد (محاله به این ارور بخوری)";
            return View("Forget");
        }

        //sms Check
        smsUser smsCheck = db.sms_tbl.FirstOrDefault(x => x.UserId == check.Id);
        if (smsCheck == null)
        {
            CreateUserLog((int) check.Id, 5, false);
            ViewBag.Error = "خطایی رخ داده است ، دوباره تلاش کنید";
            return View("Forget");
        }
        else if (DateTime.UtcNow.AddMinutes(-10) > smsCheck.CreateDateTime)
        {
            //Time Passed
            db.sms_tbl.Remove (smsCheck);
            db.SaveChanges();
            CreateUserLog((int) check.Id, 5, false);
            ViewBag.Error = "کد شما منقضی شده ، مجددا تلاش کنید";
            return View("Forget");
        }
        else if (smsCheck.IsValid == true)
        {
            if (otp == smsCheck.SmsCode)
            {
                //check.Password = BCrypt.Net.BCrypt.HashPassword(NewPassword + salt + Username.ToLower());
                //db.Users_tbl.Update(check);
                db.sms_tbl.Remove (smsCheck);
                db.SaveChanges();

                ClaimsIdentity Identity =
                    new ClaimsIdentity(new []
                        {
                            new Claim(ClaimTypes.Name,
                                check.FirstName + " " + check.LastName),
                            new Claim(ClaimTypes.NameIdentifier,
                                check.Id.ToString()),
                            new Claim("Profile", check.Profile),
                            new Claim("Username", check.Username)
                        },
                        CookieAuthenticationDefaults.AuthenticationScheme);

                var princpal = new ClaimsPrincipal(Identity);

                var properties =
                    new AuthenticationProperties {
                        ExpiresUtc = DateTime.UtcNow.AddMonths(1),
                        IsPersistent = true
                    };

                HttpContext.SignInAsync (princpal, properties);

                return RedirectToAction("resetPassword");
            }
            else
            {
                if (
                    smsCheck.TryCount > 3 // start from 0 -> 1,2,3,4 -> when 4 still can try 5 ! done
                )
                    smsCheck.IsValid = false;
                else
                    ++smsCheck.TryCount;
                db.sms_tbl.Update (smsCheck);
                db.SaveChanges();
                CreateUserLog((int) check.Id, 5, false);
                ViewBag.Error = "کد نامعتبر است ، دوباره تلاش کنید";
                ViewBag.smsPhone =
                    check.Phone.Substring(check.Phone.Count() - 4) + "*****09";
                ViewBag.userId = check.Id;
                return View("Verify");
            }
        }
        else
        {
            CreateUserLog((int) check.Id, 5, false);
            ViewBag.Error =
                "تعداد تلاش شما برای ورود بیشتر از 5 میباشد . شما مجاز به ورود تا 10 دقیقه دیگر نیستید ، مجددا تلاش کنید";
            return View("Forget");
        }
    }

    [HttpGet]
    [Authorize]
    public IActionResult resetPassword()
    {
        return View();
    }

    [HttpPost]
    public IActionResult resetPassword(string password)
    {
        var check =
            db
                .Users_tbl
                .Find(Convert
                    .ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier)));
        check.Password =
            BCrypt
                .Net
                .BCrypt
                .HashPassword(password + salt + check.Username.ToLower());
        db.SaveChanges();
        CreateUserLog((int) check.Id, 5, true);
        return RedirectToAction("index", "email");
    }

    private void SmsCode(string Code, string Phone)
    {
        // real sms
        KavenegarApi SmsApi = new KavenegarApi(db.smsTokens.Find(1).Token);
        SmsApi.VerifyLookup(Phone, Code, "demo");

        // price less
        //watch from sql server
    }

    private void CreateUserLog(int UserId, int LogAction, bool isSucces)
    {
        db
            .userLogs_tbl
            .Add(new UserLog {
                UserId = UserId,
                LogAction = LogAction,
                isSucces = isSucces,
                CreateDateTime = DateTime.UtcNow
            });
        db.SaveChanges();
    }

    public IActionResult NotAuthorized()
    {
        return View();
    }

    public IActionResult logout()
    {
        HttpContext
            .SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        CreateUserLog(Convert
            .ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier)),
        2,
        true);
        return RedirectToAction("login", "Auth");
    }
}
