using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using test.Models;
namespace test.Areas.Admin.Controllers;
//add Area Admin
[Area("Admin")]
// [Authorize]
public class HomeController : Controller
{
    private readonly Context dbs;
    private readonly string salt = "S@lt?";

    private readonly IWebHostEnvironment _env;
    public HomeController(Context _db, IWebHostEnvironment env)
    {
        _env = env;
        dbs = _db;
    }


    public IActionResult Index()
    {
        return View();
    }

    public IActionResult AddCat(int? id)
    {
     
        Category catMe = new Category();
        if(id.HasValue)
        {
           //edit
              var cat = dbs.Categories_tbl.Find(id);
                catMe.Id = cat.Id;
                catMe.CatName = cat.CatName;
                catMe.ParentId = cat.ParentId;
                catMe.CatCode = cat.CatCode;
                ViewBag.Cats = dbs.Categories_tbl.OrderByDescending(x=>x.Id).Where(x=>x.Id != id).ToList();

            
        }
        else{
           ViewBag.Cats = dbs.Categories_tbl.OrderByDescending(x=>x.Id).ToList();
        }
       
        return View("AddCat",catMe);
    }


    [HttpPost]
    public IActionResult AddCat(int? ParentId , string CatName, int? id , int CatCode)
    {   
        
        if(id.HasValue)
        {
            var cat = dbs.Categories_tbl.Find(id);
            cat.CatName = CatName;
            cat.ParentId = !ParentId.HasValue ? 0 : ParentId.Value;
            cat.CatCode = CatCode;
            dbs.SaveChanges();
            ViewBag.Cats = dbs.Categories_tbl.OrderByDescending(x=>x.Id).ToList();
            return View("AddCat");
        }
        dbs.Categories_tbl.Add(new Category
        {
            ParentId = !ParentId.HasValue ? 0 : ParentId.Value ,
            CatName = CatName,
            CatCode = (int)CatCode
        });
        dbs.SaveChanges();
        ViewBag.Cats = dbs.Categories_tbl.OrderByDescending(x=>x.Id).ToList();
        return View("AddCat");
    }

    [HttpGet]
    public IActionResult DeleteCat(int id)
    {
        var cat = dbs.Categories_tbl.Find(id);
        dbs.Categories_tbl.Remove(cat);
        dbs.SaveChanges();
        return RedirectToAction("AddCat");
    }

    public IActionResult ReportSeen()
    {
       ViewBag.Categories = dbs.Categories_tbl.Where(x=> x.ParentId == 0).OrderByDescending(x=>x.Id).ToList();

        return View();
    }
    public IActionResult SubReportSeen(int Id)
    {
       ViewBag.Categories = dbs.Categories_tbl.Where(x=> x.ParentId == Id).OrderByDescending(x=>x.Id).ToList();

        return View();
    }
    
    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Register(DtodUser user)
    {
        string PathSave;

        Users check =
            dbs
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
                    Token = "null",
                    CategoryId = 4 // تغییر داده شود به پیشفرض
                    ,isActive = true
                };
            dbs.Users_tbl.Add (NewUser);
            dbs.SaveChanges();

            var RoleCilentId =
                dbs
                    .Role_tbl
                    .Where(x => x.Name == "client")
                    .Select(x => x.Id)
                    .FirstOrDefault();

            dbs
                .UserRoles_tbl
                .Add(new UserRole {
                    UserId = (int) NewUser.Id,
                    RoleId = RoleCilentId
                });
            dbs.SaveChanges();
            ViewBag.Result = "ثبت نام با موفقیت انجام شد ";
            return RedirectToAction("GetUsers");
        }
    }
    public IActionResult GetUsers(){
        ViewBag.Users = dbs.Users_tbl.Where(x=> x.Username != "admin").OrderByDescending(x=>x.Id).ToList();
        return View("GetUsers");
    }

    public IActionResult UserSetting(int Id){
        Users Check = dbs.Users_tbl.Find(Id);
        ViewBag.User = Check;
        return View("UserSetting");
    }

    [HttpPost]
    public async Task<IActionResult> UserSettingAsync(string Addres , string FirstName , string LastName , string Phone , string PerconalCode , string NatinalCode , IFormFile Profile , int Id){
        var BaseUser = dbs.Users_tbl.Find(Id);
        BaseUser.Addres = Addres;
        BaseUser.FirstName = FirstName;
        BaseUser.LastName = LastName;
        BaseUser.Phone = Phone;
        BaseUser.PerconalCode = PerconalCode;
        BaseUser.NatinalCode = NatinalCode;
        var PathSave = BaseUser.Profile; 
        if (Profile != null)
        {
            string FileExtension = Path.GetExtension(Profile.FileName);
            var NewFileName = System.String.Concat(Guid.NewGuid().ToString(), FileExtension);
            var path = $"{_env.WebRootPath}\\uploads\\{NewFileName}";
            PathSave = $"\\uploads\\{NewFileName}";
            using (var stream = new FileStream(path, FileMode.Create))
            {
                await Profile.CopyToAsync(stream);
            }
        }
        BaseUser.Profile = PathSave;
        dbs.Users_tbl.Update(BaseUser);
        dbs.SaveChanges();


       //update Claim Profile
        var Identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Name,BaseUser.FirstName+" "+BaseUser.LastName),
            new Claim(ClaimTypes.NameIdentifier,BaseUser.Id.ToString()),
            new Claim("Profile",BaseUser.Profile),
            new Claim("Username", BaseUser.Username)
        }, CookieAuthenticationDefaults.AuthenticationScheme);




        var princpal = new ClaimsPrincipal(Identity);

        var properties = new AuthenticationProperties
        {
            ExpiresUtc = DateTime.UtcNow.AddMonths(1),
            IsPersistent = true
        };

        HttpContext.SignInAsync(princpal, properties);
        return RedirectToAction("GetUsers");
    }


    [HttpGet]
    public IActionResult ProfileUser()
    {
        var UserId = Convert.ToInt32(User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier).Value);


        Users Check = dbs.Users_tbl.Find(UserId);
        ViewBag.data = Check;

        var sentCheck = EmailController.DataEater(1, User, dbs, false, true, 5);
        ViewBag.dataSent = sentCheck;

        var reciveCheck = EmailController.DataEater(1, User, dbs, false, false, 5);
        ViewBag.dataRecive = reciveCheck;

        var UserLogCheck = Log.AllUserLog(dbs, User);
        ViewBag.dataUserLog = UserLogCheck;
        return View();
    }

    public IActionResult GetCategory(){
        ViewBag.Users = dbs.Users_tbl.Where(x=> x.Username != "admin").Include(x=>x.Category).OrderByDescending(x=>x.Id).ToList();
        return View();
    }
    [HttpGet]
    public IActionResult ChildCategories(int id){
        ViewBag.Categories = dbs.Categories_tbl.Where(x=> x.ParentId == 0).OrderByDescending(x=>x.Id).ToList();
        ViewBag.Id = id;
        return View("Categories");
    }
    [HttpGet]
    public IActionResult Categories(int id , int parentId){
        ViewBag.Categories = dbs.Categories_tbl.Where(x=> x.ParentId == parentId).OrderByDescending(x=>x.Id).ToList();
        ViewBag.Id = id;
        ViewBag.parentId = parentId;
        return View();
    }
    [HttpGet]
    public IActionResult AddCategories(int id , int catId ){
        var check = dbs.Users_tbl.Find(id);
        check.CategoryId=catId;
        dbs.Users_tbl.Update(check);
        dbs.SaveChanges();
        return RedirectToAction("GetCategory");
    }
}