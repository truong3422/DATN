using Firebase.Auth;
using Firebase.Storage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Newtonsoft.Json;
using PoolComVnWebClient.Common;
using PoolComVnWebClient.DTO;
using System.Net;
using System.Net.Http.Headers;

namespace PoolComVnWebClient.Controllers
{
    public class UserController : Controller
    {
        private readonly HttpClient client = null;
        private string ApiUrl = Constant.ApiUrl;
        private string ApiKey = FirebaseAPI.ApiKey;
        private string Bucket = FirebaseAPI.Bucket;
        private string AuthEmail = FirebaseAPI.AuthEmail;
        private string AuthPassword = FirebaseAPI.AuthPassword;
        public UserController()
        {
            client = new HttpClient();
            var contentType = new MediaTypeWithQualityHeaderValue("application/json");
            client.DefaultRequestHeaders.Accept.Add(contentType);
        }
        public IActionResult Index(string? email)
        {
            string emailCookie = HttpContext.Request.Cookies["Email"];
            if (email == null)
            {
                email = emailCookie;
                ViewBag.IsOnEditBtn = true;
            }
            else
            {
                if(email == emailCookie)
                {
                    ViewBag.IsOnEditBtn = true;
                }
            }
            var response = client.GetAsync($"{ApiUrl}/Account/GetAccountByEmail/{email}").Result;
            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(string.Empty, "Không thể lấy thông tin tài khoản.");
                return View();
            }
            var AccountData = response.Content.ReadAsStringAsync().Result;
            var account = JsonConvert.DeserializeObject<AccountDTO>(AccountData);
            var responeUser = client.GetAsync($"{ApiUrl}/Account/GetUserByAccount?accountId={account.AccountID}").Result;
            if (responeUser.StatusCode == HttpStatusCode.NotFound)
            {
                return RedirectToAction("CreateUser");
            }
            else if (responeUser.IsSuccessStatusCode)
            {
                var UserData = responeUser.Content.ReadAsStringAsync().Result;
                var user = JsonConvert.DeserializeObject<UserDTO>(UserData);
                ViewBag.User = user;
                ViewBag.Account = account;
                var responseGetTour = client.GetAsync(ApiUrl + "/User/GetTourJoinedForUser?userId=" + user.UserId).Result;
                if (responseGetTour.IsSuccessStatusCode)
                {
                    var TourData = responseGetTour.Content.ReadAsStringAsync().Result;
                    var tournaments = JsonConvert.DeserializeObject<List<TournamentDetailDTO>>(TourData);
                    tournaments.Reverse();
                    if(tournaments.Count > 0)
                    {
                        ViewBag.Tournaments = tournaments;
                    }
                }

                var responseWinMatchNum = client.GetAsync(ApiUrl + "/User/GetWinMatchNumber/" + user.UserId).Result;
                if (responseWinMatchNum.IsSuccessStatusCode)
                {
                    var winMatchNum = responseWinMatchNum.Content.ReadAsStringAsync().Result;
                    ViewBag.WinMatchNum = winMatchNum;
                }

                var responseLoseMatchNum = client.GetAsync(ApiUrl + "/User/GetLoseMatchNumber/" + user.UserId).Result;
                if (responseLoseMatchNum.IsSuccessStatusCode)
                {
                    var loseMatchNum = responseLoseMatchNum.Content.ReadAsStringAsync().Result;
                    ViewBag.LoseMatchNum = loseMatchNum;
                }

                var responseGetTopForPlayer = client.GetAsync(ApiUrl + "/User/GetTopForPlayer/" + user.UserId).Result;
                if (responseGetTopForPlayer.IsSuccessStatusCode)
                {
                    var awardsData = responseGetTopForPlayer.Content.ReadAsStringAsync().Result;
                    var awards = JsonConvert.DeserializeObject<List<AwardDTO>>(awardsData);
                    if(awards.Count > 0)
                    {
                        ViewBag.Awards = awards;
                    }
                }
            }
            return View();
        }

        public IActionResult CreateUser()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> CreateUser(UserDTO userDTO, string Nickname, IFormFile BannerFile, string ward)
        {
            string email = HttpContext.Request.Cookies["Email"];
            var response = client.GetAsync($"{ApiUrl}/Account/GetAccountByEmail/{email}").Result;
            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(string.Empty, "Không thể lấy thông tin tài khoản.");
                return View();
            }
            var AccountData = response.Content.ReadAsStringAsync().Result;
            var account = JsonConvert.DeserializeObject<AccountDTO>(AccountData);
            userDTO.PhoneNumber = account.PhoneNumber;
            userDTO.AccountId = account.AccountID;
            userDTO.CreatedDate = DateTime.Now;
            userDTO.UpdatedDate = DateTime.Now;
            userDTO.WardCode = ward;
            userDTO.AccountEmail = email;

            string firebaseFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Firebase");
            if (!Directory.Exists(firebaseFolder))
            {
                Directory.CreateDirectory(firebaseFolder);
            }

            if (BannerFile != null && BannerFile.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(BannerFile.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Firebase", fileName);

                using (FileStream memoryStream = new FileStream(filePath, FileMode.Create))
                {
                    BannerFile.CopyTo(memoryStream);


                }
                var fileStream2 = new FileStream(filePath, FileMode.Open);
                var downloadLink = await UploadFromFirebase(fileStream2, BannerFile.FileName, "User", account.Email, 0);
                fileStream2.Close();
                userDTO.Avatar = downloadLink;
            }
            string directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Firebase");

            try
            {

                string[] filePaths = Directory.GetFiles(directoryPath);
                foreach (string filePath2 in filePaths)
                {
                    System.IO.File.Delete(filePath2);
                }

                Console.WriteLine("All images in the directory have been deleted successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while deleting images: {ex.Message}");
            }
           var response2 = await client.PostAsJsonAsync($"{ApiUrl}/Account/CreateUser", userDTO);
            var responseBody = await response2.Content.ReadAsStringAsync();
            int userId = JsonConvert.DeserializeObject<int>(responseBody);
            var playerDTO = new PlayerDTO
            {
                PlayerName = Nickname,
                CountryId = 1,
                Level = 0,
                UserId = userId,
                TourId = null,
                PhoneNumber = account.PhoneNumber,
                Email = account.Email,
                IsPayed = false,
                CountryName = "Viet Nam"

            };
            var response3 = await client.PostAsJsonAsync($"https://localhost:5000/api/Player/AddPlayer", playerDTO);

            if (response2.IsSuccessStatusCode && response3.IsSuccessStatusCode)
            {
                return RedirectToAction("Index");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Lỗi khi điền thông tin chi tiết.");
                return View(userDTO);
            }

        }
        public async Task<string> UploadFromFirebase(FileStream stream, string filename, string folderName, string newsTitle, int order)
        {
            var auth = new FirebaseAuthProvider(new FirebaseConfig(ApiKey));
            var a = await auth.SignInWithEmailAndPasswordAsync(AuthEmail, AuthPassword);
            var cancellation = new CancellationTokenSource();
            if (order == 0)
            {
                if (!string.IsNullOrEmpty(newsTitle))
                {
                    var task = new FirebaseStorage(
                   Bucket,
                   new FirebaseStorageOptions
                   {
                       AuthTokenAsyncFactory = () => Task.FromResult(a.FirebaseToken),
                       ThrowOnCancel = true
                   }
               ).Child(folderName)
               .Child(newsTitle)
                .Child($"Banner")
                .PutAsync(stream, cancellation.Token);
                    string link = await task;
                    return link;
                }
                else
                {
                    var task = new FirebaseStorage(
                   Bucket,
                   new FirebaseStorageOptions
                   {
                       AuthTokenAsyncFactory = () => Task.FromResult(a.FirebaseToken),
                       ThrowOnCancel = true
                   }
               ).Child(folderName)
                .Child($"Banner")
                .PutAsync(stream, cancellation.Token);
                    string link = await task;
                    return link;
                }




            }
            else
            {
                var orderedFileName = $"Image{order}{Path.GetExtension(stream.Name)}";
                var task = new FirebaseStorage(
                    Bucket,
                    new FirebaseStorageOptions
                    {
                        AuthTokenAsyncFactory = () => Task.FromResult(a.FirebaseToken),
                        ThrowOnCancel = true
                    }
                ).Child(folderName)
                .Child(newsTitle)
                 .Child(orderedFileName)
                 .PutAsync(stream, cancellation.Token);
                try
                {
                    string link = await task;
                    return link;


                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception was thrown : {0}", ex);
                    return null;
                }
            }
        }

        public IActionResult EditUserProfile()
        {
            if (TempData["Success"] != null)
            {
                ViewBag.Success = TempData["Success"].ToString();
            }
            if (TempData["Failure"] != null)
            {
                ViewBag.Failure = TempData["Failure"].ToString();
            }
            string email = HttpContext.Request.Cookies["Email"];
            var response = client.GetAsync($"{ApiUrl}/Account/GetAccountByEmail/{email}").Result;
            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(string.Empty, "Không thể lấy thông tin tài khoản.");
                return View();
            }
            var AccountData = response.Content.ReadAsStringAsync().Result;
            var account = JsonConvert.DeserializeObject<AccountDTO>(AccountData);
            var responeUser = client.GetAsync($"{ApiUrl}/Account/GetUserByAccount?accountId={account.AccountID}").Result;
            if (responeUser.IsSuccessStatusCode)
            {
                var UserData = responeUser.Content.ReadAsStringAsync().Result;
                var user = JsonConvert.DeserializeObject<UserDTO>(UserData);
                ViewBag.User = user;
                ViewBag.Account = account;

                var responseWard = client.GetAsync($"{ApiUrl}/Address/getwardsBywardCode/{user.WardCode}").Result;
                var wardData = responseWard.Content.ReadAsStringAsync().Result;
                var ward = JsonConvert.DeserializeObject<WardDTO>(wardData);

                var responseDistrict = client.GetAsync($"{ApiUrl}/Address/GetdistrictsByDistrictCode/{ward.DistrictCode}").Result;
                var districtData = responseDistrict.Content.ReadAsStringAsync().Result;
                var district = JsonConvert.DeserializeObject<DistrictDTO>(districtData);

                var responseProvince = client.GetAsync($"{ApiUrl}/Address/getProvincesByProvinceCode/{district.ProvinceCode}").Result;
                var provinceData = responseProvince.Content.ReadAsStringAsync().Result;
                var province = JsonConvert.DeserializeObject<ProvinceDTO>(provinceData);

                ViewBag.Ward = ward;
                ViewBag.District = district;
                ViewBag.Province = province;
            }
            return View();
        }
        public async Task DeleteFromFirebase(string title, string filename)
        {
            try
            {
                var auth = new FirebaseAuthProvider(new FirebaseConfig(ApiKey));
                var a = await auth.SignInWithEmailAndPasswordAsync(AuthEmail, AuthPassword);

                var cancellation = new CancellationTokenSource();
                var storage = new FirebaseStorage(
                    Bucket,
                    new FirebaseStorageOptions
                    {
                        AuthTokenAsyncFactory = () => Task.FromResult(a.FirebaseToken),
                        ThrowOnCancel = true
                    }
                );
                var folderPath = $"User/{title}/{filename}";


                await storage.Child(folderPath).DeleteAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occurred during deletion: {0}", ex);
            }
        }
        [HttpPost]
        public async Task<IActionResult> EditUserProfile(UserDTO userDTO,string ward,IFormFile BannerFile )
        {
            if (ward!=null) { userDTO.WardCode = ward; }

            string firebaseFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Firebase");
            if (!Directory.Exists(firebaseFolder))
            {
                Directory.CreateDirectory(firebaseFolder);
            }

            if (BannerFile != null && BannerFile.Length > 0)
            {
                await DeleteFromFirebase(userDTO.AccountEmail, userDTO.Avatar);
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(BannerFile.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Firebase", fileName);

                using (FileStream memoryStream = new FileStream(filePath, FileMode.Create))
                {
                    BannerFile.CopyTo(memoryStream);


                }
                var fileStream2 = new FileStream(filePath, FileMode.Open);
                var downloadLink = await UploadFromFirebase(fileStream2, BannerFile.FileName, "User", userDTO.AccountEmail, 0);
                fileStream2.Close();
                userDTO.Avatar = downloadLink;
            }
            string directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Firebase");

            try
            {

                string[] filePaths = Directory.GetFiles(directoryPath);
                foreach (string filePath2 in filePaths)
                {
                    System.IO.File.Delete(filePath2);
                }

                Console.WriteLine("All images in the directory have been deleted successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while deleting images: {ex.Message}");
            }
            var response2 = await client.PostAsJsonAsync($"{ApiUrl}/User/UpdateUser", userDTO);
            if (response2.IsSuccessStatusCode)
            {
                TempData["Success"] = "Cập nhật thông tin thành công";
                return RedirectToAction("EditUserProfile");
            }
            else
            {
                TempData["Failure"] = "Cập nhật thông tin thất bại";
                return RedirectToAction("EditUserProfile");
            }
        }

        public IActionResult UserList()
        {
            string email = HttpContext.Request.Cookies["Email"];
            var responseAccount = client.GetAsync($"https://localhost:5000/api/Account/GetAccountByEmail/{email}").Result;
            var AccountData = responseAccount.Content.ReadAsStringAsync().Result;
            var account = JsonConvert.DeserializeObject<AccountDTO>(AccountData);
            ViewBag.Account = account;
            var responeUser = client.GetAsync($"{ApiUrl}/User/GetUserlist").Result;
            if (responeUser.IsSuccessStatusCode)
            {
                var UserData = responeUser.Content.ReadAsStringAsync().Result;
                var userList = JsonConvert.DeserializeObject<List<UserIncludStatisticDTO>>(UserData);
                ViewBag.UserList = userList;
            }
            return View();
        }

    }
}
