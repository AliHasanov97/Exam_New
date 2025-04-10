using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;
using Exam.DAL;
using Exam.Models;
using System;
using Newtonsoft.Json;

namespace Exam.Controllers
{
    public class Auth : Controller
    {

        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public Auth(AppDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }
        public IActionResult Index()
        {
            if (TempData["UserName"] == null)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }
        [HttpPost]
        public IActionResult GoogleLogin()
        {
            var redirectUrl = Url.Action("GoogleResponse", "Auth");
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        public async Task<IActionResult> GoogleResponse()
        {
            // İstifadəçi məlumatlarının doğrulamasını həyata keçiririk
            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            if (!result.Succeeded)
            {
                return RedirectToAction("Index", "Home");
            }

            var accessToken = result.Properties.GetTokenValue("access_token");
            if (string.IsNullOrEmpty(accessToken))
            {
                return RedirectToAction("Index", "Home");
            }

            // Google API-dən istifadəçi məlumatlarını alırıq
            var userInfo = await GetGoogleUserInfo(accessToken);
            if (userInfo == null)
            {
                return RedirectToAction("Index", "Home");
            }

            // İstifadəçi məlumatlarını hazırlayırıq
            var profileImage = GetProfileImageUrl(userInfo);


            // TempData ilə istifadəçi məlumatlarını göndəririk
            TempData["UserName"] = userInfo.name.ToString();
            TempData["UserEmail"] = userInfo.email.ToString();
            TempData["UserImage"] = profileImage;

            // İstifadəçi email-ə görə bazada olub olmadığını yoxlayırıq
            string userEmail = userInfo.email.ToString();
            if (_context.Users.Any(x => x.Email == userEmail))
            {
                var user = _context.Users.FirstOrDefault(x => x.Email == userEmail);
                // Session-a istifadəçi məlumatını əlavə edirik
                HttpContext.Session.SetString("User", user.Id.ToString());
                return RedirectToAction("Index", "Home");
            }

            return RedirectToAction("Index", "Auth");
        }

        // Google API-dən istifadəçi məlumatlarını əldə edən metod
        private async Task<dynamic> GetGoogleUserInfo(string accessToken)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
                    var response = await client.GetStringAsync("https://www.googleapis.com/oauth2/v2/userinfo");

                    return JsonConvert.DeserializeObject<dynamic>(response);
                }
            }
            catch (Exception)
            {
                // Hər hansı bir xətanı idarə edirik, məsələn, şəbəkə problemi
                return null;
            }
        }

        // Profil şəkli URL-sini əldə edən metod
        private string GetProfileImageUrl(dynamic userInfo)
        {
            string profileImage = userInfo.picture != null ? userInfo.picture.ToString() : "/images/default-profile.png";

            if (profileImage.Contains("?"))
            {
                profileImage = profileImage.Split('?')[0];
            }

            if (profileImage.Contains("=s"))
            {
                int index = profileImage.IndexOf("=s");
                profileImage = profileImage.Substring(0, index) + "=s400-c";
            }
            else
            {
                profileImage += "=s400-c";
            }

            return profileImage;
        }


        [HttpPost]
        public async Task<IActionResult> Index(User model, IFormFile Picture_r, string Gphoto)
        {

            string imagePath = Gphoto; // Default profil şəkli

            if (Picture_r != null && Picture_r.Length > 0)
            {
                // Şəkilin yüklənəcəyi qovluğu təyin edirik
                string uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // Unikal fayl adı
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + Picture_r.FileName;
                imagePath = Path.Combine("uploads", uniqueFileName);

                using (var fileStream = new FileStream(Path.Combine(_environment.WebRootPath, imagePath), FileMode.Create))
                {
                    await Picture_r.CopyToAsync(fileStream);
                }
            }

            var user = new User
            {
                FullName = model.FullName,
                Email = model.Email,
                Balance = 0,
                Active = true,
                Picture = imagePath,
                Tranzactions = new List<Tranzaction>
    {
        // Initial bonus deposit
        new Tranzaction
        {
            Id = Guid.NewGuid(),
            Amount = 10,
            DateTime = DateTime.UtcNow,
            Title = "Qeydiyyat bonusu",
            Description = "Yeni qeydiyyatdan keçdiyiniz üçün bonus balans",
            FinancialOperations = "Deposit",
            Icon = "bi-gift",
            Balance = 0,
            DiscountPercent = 0,
            DiscountAmount = 0
        },
        
        // Sample expense with discount (Premium subscription)
        new Tranzaction
        {
            Id = Guid.NewGuid(),
            Amount = 2,
            OriginalAmount = 4,
            DateTime = DateTime.UtcNow.AddSeconds(10),
            Title = "Premium abunə",
            Description = "1 aylıq premium abunə (50% endirim)",
            FinancialOperations = "Expence",
            Icon = "bi-crown",
            Balance = 0, // Will be updated after calculation
            DiscountPercent = 50,
            DiscountAmount = 2
        },
       
    },
                Notifications = new List<Notification>
    {
        new Notification
        {
            Id = Guid.NewGuid(),
            Icon = "bi-info-circle",
            Title = "Xoş gəlmisiniz",
            Message = "Xidmətlərimizdən yararlanmağınıza şadıq!",
            IsRead = false,
            CreatedAt = DateTime.Now
        },
        new Notification
        {
            Id = Guid.NewGuid(),
            Icon = "bi-percent",
            Title = "Xüsusi endirim",
            Message = "Yeni istifadəçi endirimi - ilk ödənişinizdə 10% endirim!",
            IsRead = false,
            CreatedAt = DateTime.Now
        }
    }
            };

            // Calculate balances for each transaction
            decimal runningBalance = user.Balance;
            foreach (var transaction in user.Tranzactions.OrderBy(t => t.DateTime))
            {
                if (transaction.FinancialOperations == "Deposit")
                {
                    runningBalance += transaction.Amount;
                }
                else // Expence
                {
                    runningBalance -= transaction.Amount;
                }
                transaction.Balance = runningBalance;
            }

            // Update user's current balance
            user.Balance = runningBalance;

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var CurruntPerson = _context.Users.Where(x => x.Email == model.Email).FirstOrDefault();


            HttpContext.Session.SetString("User", CurruntPerson.Id.ToString());
            Console.WriteLine("UserID : " + CurruntPerson.Id.ToString());

            // Qeydiyyatdan sonra istifadəçini Cabinet səhifəsinə yönləndiririk
            return RedirectToAction("Index", "Cabinet");
        }



        [HttpPost]
        public IActionResult Logout()
        {
            // Yalnız "User" məlumatını silirik
            HttpContext.Session.Remove("User");

            // Sessiyanı təmizləyirik
            HttpContext.Session.Clear();

            return RedirectToAction("Index", "Home");
        }

    }
}
