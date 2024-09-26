using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using PoolComVnWebClient.Common;
using Newtonsoft.Json;
using PoolComVnWebClient.DTO;
using System.Net;
using Firebase.Auth;
using Firebase.Storage;
using System.Text.RegularExpressions;
using OfficeOpenXml;


namespace PoolComVnWebClient.Controllers
{
    public class ClubController : Controller
    {
        private readonly HttpClient client = null;
        private string ApiUrl = Constant.ApiUrl;
        private string ApiKey = FirebaseAPI.ApiKey;
        private string Bucket = FirebaseAPI.Bucket;
        private string AuthEmail = FirebaseAPI.AuthEmail;
        private string AuthPassword = FirebaseAPI.AuthPassword;
        public ClubController()
        {
            client = new HttpClient();
            var contentType = new MediaTypeWithQualityHeaderValue("application/json");
            client.DefaultRequestHeaders.Accept.Add(contentType);
        }
        public IActionResult Index(int? id, int? page, string searchQuery)
        {
            int pageNumber = page ?? 1;

            ViewBag.SearchQuery = searchQuery;
            if (id == null)
            {
                int pageSize = 5;
                if (TempData.ContainsKey("SuccessMessage"))
                {
                    ViewBag.Success = TempData["SuccessMessage"];
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
                var response2 = client.GetAsync($"{ApiUrl}/Club/GetClubByAccountId/?accountID={account.AccountID}").Result;
                if (!response2.IsSuccessStatusCode)
                {
                    if (response2.StatusCode == HttpStatusCode.NotFound)
                    {
                        return RedirectToAction("CreateClub");
                    }
                    else
                    {

                        ModelState.AddModelError(string.Empty, "Không thể lấy thông tin câu lạc bộ.");
                        return RedirectToAction("CreateClub");
                    }
                }
                var ClubData = response2.Content.ReadAsStringAsync().Result;
                var club = JsonConvert.DeserializeObject<ClubDTO>(ClubData);
                var response3 = client.GetAsync($"{ApiUrl}/ClubPost/GetByClubId/{club.ClubId}").Result;
                if (response3.StatusCode == HttpStatusCode.NotFound)
                {
                    ViewBag.ClubPost = null;
                }
                else if (response3.IsSuccessStatusCode)
                {
                    var clubPostData = response3.Content.ReadAsStringAsync().Result;
                    var clubPosts = JsonConvert.DeserializeObject<List<ClubPostDTO>>(clubPostData);

                    if (!string.IsNullOrEmpty(searchQuery))
                    {
                        clubPosts = clubPosts.Where(cp => cp.Title.Contains(searchQuery)).ToList();
                    }

                    var paginatedClubPosts = PaginatedList<ClubPostDTO>.CreateAsync(clubPosts, pageNumber, pageSize);
                    ViewBag.ClubPost = clubPosts;
                    ViewBag.Club = club;
                    ViewBag.AccountEmail = email;
                    return View(paginatedClubPosts);
                }
                ViewBag.Club = club;
                ViewBag.AccountEmail = email;
                return View();
            }
            else
            {
                int pageSize = 6;
                var response = client.GetAsync($"{ApiUrl}/Club/{id}").Result;
                if (!response.IsSuccessStatusCode)
                {
                    ModelState.AddModelError(string.Empty, "Không thể lấy thông tin câu lạc bộ.");
                    return RedirectToAction("CreateClub");
                }
                var ClubData = response.Content.ReadAsStringAsync().Result;
                var club = JsonConvert.DeserializeObject<ClubDTO>(ClubData);
                if (club==null)
                    
                ViewBag.Club = club;
                var response2 = client.GetAsync($"{ApiUrl}/ClubPost/GetByClubId/{club.ClubId}").Result;
                if (response2.StatusCode == HttpStatusCode.NotFound)
                {
                    ViewBag.ClubPost = null;
                }
                else if (response2.IsSuccessStatusCode)
                {
                    var clubPostData = response2.Content.ReadAsStringAsync().Result;
                    var clubPosts = JsonConvert.DeserializeObject<List<ClubPostDTO>>(clubPostData);

                    if (!string.IsNullOrEmpty(searchQuery))
                    {
                        clubPosts = clubPosts.Where(cp => cp.Title.Contains(searchQuery)).ToList();
                    }

                    var paginatedClubPosts = PaginatedList<ClubPostDTO>.CreateAsync(clubPosts, pageNumber, pageSize);
                    ViewBag.ClubPost = clubPosts;
                    ViewBag.Club = club;
                    return View(paginatedClubPosts);
                }
                var response3 = client.GetAsync($"{ApiUrl}/Account/GetAccountById/{club.AccountId}").Result;
                if (!response3.IsSuccessStatusCode)
                {
                    ModelState.AddModelError(string.Empty, "Không thể lấy thông tin tài khoản.");
                    return View();
                }
                var AccountData = response3.Content.ReadAsStringAsync().Result;
                var account = JsonConvert.DeserializeObject<AccountDTO>(AccountData);
                ViewBag.AccountEmail = account.Email;
                return View();
            }

        }

        public IActionResult AddPost(int clubid)
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
            var response2 = client.GetAsync($"{ApiUrl}/Club/GetClubByAccountId/?accountID={account.AccountID}").Result;
            if (!response2.IsSuccessStatusCode)
            {
                if (response2.StatusCode == HttpStatusCode.NotFound)
                {
                    return View();
                }
                else
                {

                    ModelState.AddModelError(string.Empty, "Không thể lấy thông tin câu lạc bộ.");
                    return View();
                }
            }
            var ClubData = response2.Content.ReadAsStringAsync().Result;
            var club = JsonConvert.DeserializeObject<ClubDTO>(ClubData);
            ViewBag.Club = club;
            ViewBag.AccountEmail = email;
            ViewBag.ClubID = clubid;
            return View();
        }
        [HttpPost]
        public ActionResult UploadImage(List<IFormFile> files)
        {
            var filepath2 = "";
            foreach (IFormFile file in Request.Form.Files)
            {
                string firebaseFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Firebase");
                if (!Directory.Exists(firebaseFolder))
                {
                    Directory.CreateDirectory(firebaseFolder);
                }
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Firebase", file.FileName);
                using (FileStream memoryStream = new FileStream(filePath, FileMode.Create))
                {
                    file.CopyTo(memoryStream);

                }
                filepath2 = "/Firebase/" + file.FileName;
            }
            return Json(new { url = filepath2 });
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
                var folderPath = $"Club/{title}/{filename}";


                await storage.Child(folderPath).DeleteAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occurred during deletion: {0}", ex);
            }
        }
        [HttpPost]
        public async Task<IActionResult> AddPost(ClubPostDTO ClubPostDTO, IFormFile BannerFile, int clubid)
        {
            ClubPostDTO.CreatedDate = DateTime.Now;
            ClubPostDTO.UpdatedDate = DateTime.Now;
            ClubPostDTO.Status = true;
            ClubPostDTO.ClubId = clubid;

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
                var downloadLink = await UploadFromFirebase(fileStream2, BannerFile.FileName, "ClubPost", ClubPostDTO.Title, 0);
                fileStream2.Close();
                ClubPostDTO.Flyer = downloadLink;
            }
            int index = 1;
            string pattern = @"<img.*?src=""(.*?)"".*?>";
            MatchCollection matches = Regex.Matches(ClubPostDTO.Description, pattern);
            foreach (Match match in matches)
            {
                string src = match.Groups[1].Value;
                string filenameWithoutFirebase = src.Replace("/Firebase/", "");
                string absolutePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Firebase", filenameWithoutFirebase);
                var fileStream2 = new FileStream(absolutePath, FileMode.Open);
                var downloadLink = await UploadFromFirebase(fileStream2, filenameWithoutFirebase, "ClubPost", ClubPostDTO.Title, index);
                index++;
                fileStream2.Close();
                ClubPostDTO.Description = ClubPostDTO.Description.Replace(src, downloadLink);
            }
            string directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Firebase");

            try
            {

                string[] filePaths = Directory.GetFiles(directoryPath);
                foreach (string filePath in filePaths)
                {
                    System.IO.File.Delete(filePath);
                }

                Console.WriteLine("All images in the directory have been deleted successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while deleting images: {ex.Message}");
            }
            ClubPostDTO.PostId = 1;
            var response = await client.PostAsJsonAsync($"{ApiUrl}/ClubPost/Add", ClubPostDTO);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Index");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Lỗi khi thêm tin tức.");
                return View(ClubPostDTO);
            }
        }
        [HttpGet]
        public IActionResult ClubPostDetails(int id, int? clubid)
        {

            if (clubid == null)
            {
                string email = HttpContext.Request.Cookies["Email"];
                var responseaccount = client.GetAsync($"{ApiUrl}/Account/GetAccountByEmail/{email}").Result;
                if (!responseaccount.IsSuccessStatusCode)
                {
                    ModelState.AddModelError(string.Empty, "Không thể lấy thông tin tài khoản.");
                    return View();
                }
                var AccountData = responseaccount.Content.ReadAsStringAsync().Result;
                var account = JsonConvert.DeserializeObject<AccountDTO>(AccountData);
                var response2 = client.GetAsync($"{ApiUrl}/Club/GetClubByAccountId/?accountID={account.AccountID}").Result;
                if (!response2.IsSuccessStatusCode)
                {
                    if (response2.StatusCode == HttpStatusCode.NotFound)
                    {
                        return View();
                    }
                    else
                    {

                        ModelState.AddModelError(string.Empty, "Không thể lấy thông tin câu lạc bộ.");
                        return View();
                    }
                }
                var ClubData = response2.Content.ReadAsStringAsync().Result;
                var club = JsonConvert.DeserializeObject<ClubDTO>(ClubData);
                var response3 = client.GetAsync($"{ApiUrl}/ClubPost/GetByClubId/{club.ClubId}").Result;
                if (response3.StatusCode == HttpStatusCode.NotFound)
                {
                    ViewBag.ClubPost = null;
                }
                else if (response3.IsSuccessStatusCode)
                {
                    var clubPostData = response3.Content.ReadAsStringAsync().Result;
                    var clubPosts = JsonConvert.DeserializeObject<List<ClubPostDTO>>(clubPostData);
                    ViewBag.ClubPost = clubPosts;
                }
                ViewBag.Club = club;
                ViewBag.AccountEmail = email;
                return View();
            }
            else
            {
                var response4 = client.GetAsync($"{ApiUrl}/Club/{clubid}").Result;
                if (!response4.IsSuccessStatusCode)
                {
                    ModelState.AddModelError(string.Empty, "Không thể lấy thông tin câu lạc bộ.");
                    return View();
                }
                var ClubData = response4.Content.ReadAsStringAsync().Result;
                var club = JsonConvert.DeserializeObject<ClubDTO>(ClubData);
                ViewBag.Club = club;
                var response2 = client.GetAsync($"{ApiUrl}/ClubPost/GetByClubId/{club.ClubId}").Result;
                if (response2.StatusCode == HttpStatusCode.NotFound)
                {
                    ViewBag.ClubPost = null;
                }
                else if (response2.IsSuccessStatusCode)
                {
                    var clubPostData = response2.Content.ReadAsStringAsync().Result;
                    var clubPosts = JsonConvert.DeserializeObject<List<ClubPostDTO>>(clubPostData);
                    ViewBag.ClubPost = clubPosts;
                }
                var response3 = client.GetAsync($"{ApiUrl}/Account/GetAccountById/{club.AccountId}").Result;
                if (!response3.IsSuccessStatusCode)
                {
                    ModelState.AddModelError(string.Empty, "Không thể lấy thông tin tài khoản.");
                    return View();
                }
                var AccountData = response3.Content.ReadAsStringAsync().Result;
                var account = JsonConvert.DeserializeObject<AccountDTO>(AccountData);
                ViewBag.AccountEmail = account.Email;
                var response = client.GetAsync($"{ApiUrl}/ClubPost/{id}").Result;

                if (response.IsSuccessStatusCode)
                {

                    var jsonContent = response.Content.ReadAsStringAsync().Result;
                    var newsDetails = JsonConvert.DeserializeObject<ClubPostDTO>(jsonContent);
                    TempData["SuccessMessage"] = "Tạo mới bài viết" + newsDetails.Title + "thành công";
                    return View(newsDetails);
                }
                else if (response.StatusCode == HttpStatusCode.NotFound)
                {

                    return NotFound();
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Lỗi khi lấy chi tiết tin tức.");
                    return View();
                }
            }

        }
        [HttpPost]
        public async Task<IActionResult> UpdateClubPost(ClubPostDTO clubPostDTO, IFormFile BannerFile)
        {
            string firebaseFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Firebase");
            if (!Directory.Exists(firebaseFolder))
            {
                Directory.CreateDirectory(firebaseFolder);
            }

            clubPostDTO.UpdatedDate = DateTime.Now;
            if (BannerFile != null && BannerFile.Length > 0)
            {
                await DeleteFromFirebase(clubPostDTO.Title, clubPostDTO.Flyer);
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(BannerFile.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Firebase", fileName);

                using (FileStream memoryStream = new FileStream(filePath, FileMode.Create))
                {
                    BannerFile.CopyTo(memoryStream);


                }
                var fileStream2 = new FileStream(filePath, FileMode.Open);
                var downloadLink = await UploadFromFirebase(fileStream2, BannerFile.FileName, "ClubPost", clubPostDTO.Title, 0);
                fileStream2.Close();
                clubPostDTO.Flyer = downloadLink;
            }
            int index = 1;
            string pattern = @"<img.*?src=""(.*?)"".*?>";
            MatchCollection matches = Regex.Matches(clubPostDTO.Description, pattern);
            foreach (Match match in matches)
            {
                string src = match.Groups[1].Value;
                if (src.Contains("/o/"))
                {
                    continue;
                }
                string filenameWithoutFirebase = src.Replace("/Firebase/", "");
                string absolutePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Firebase", filenameWithoutFirebase);
                var fileStream2 = new FileStream(absolutePath, FileMode.Open);
                var downloadLink = await UploadFromFirebase(fileStream2, filenameWithoutFirebase, "ClubPost", clubPostDTO.Title, index);
                index++;
                fileStream2.Close();
                clubPostDTO.Description = clubPostDTO.Description.Replace(src, downloadLink);
            }
            try
            {
                string directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Firebase");
                string[] filePaths = Directory.GetFiles(directoryPath);
                foreach (string filePath in filePaths)
                {
                    System.IO.File.Delete(filePath);
                }

                Console.WriteLine("All images in the directory have been deleted successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while deleting images: {ex.Message}");
            }

            var response = client.PostAsJsonAsync($"{ApiUrl}/ClubPost/Update/{clubPostDTO.PostId}", clubPostDTO).Result;

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Chỉnh sửa bài viết" + clubPostDTO.Title + "thành công";
                return RedirectToAction("Index");
            }
            else if (response.StatusCode == HttpStatusCode.NotFound)
            {

                return NotFound();
            }
            else
            {

                ModelState.AddModelError(string.Empty, "Lỗi khi cập nhật tin tức.");
                return View(clubPostDTO);
            }
        }

        public IActionResult ClubTournament(int? id)
        {
            if (id == null)
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
                var response2 = client.GetAsync($"{ApiUrl}/Club/GetClubByAccountId/?accountID={account.AccountID}").Result;
                if (!response2.IsSuccessStatusCode)
                {
                    if (response2.StatusCode == HttpStatusCode.NotFound)
                    {
                        return View();
                    }
                    else
                    {

                        ModelState.AddModelError(string.Empty, "Không thể lấy thông tin câu lạc bộ.");
                        return View();
                    }
                }
                var ClubData = response2.Content.ReadAsStringAsync().Result;
                var club = JsonConvert.DeserializeObject<ClubDTO>(ClubData);
                var response3 = client.GetAsync($"{ApiUrl}/Tournament/GetTournamentsByClubId?clubId={club.ClubId}").Result;
                if (response3.StatusCode == HttpStatusCode.NotFound)
                {
                    ViewBag.Tournament = null;
                }
                else if (response3.IsSuccessStatusCode)
                {
                    var tournamentData = response3.Content.ReadAsStringAsync().Result;
                    var tours = JsonConvert.DeserializeObject<List<TournamentDetailDTO>>(tournamentData);
                    ViewBag.Tournament = tours;
                    bool canCreateTour = true;
                    foreach (var tour in tours)
                    {
                        if (tour.Status == Constant.TournamentInProgress 
                            || tour.Status == Constant.TournamentIncoming)
                        {
                            canCreateTour = false;
                        }
                    }
                    ViewBag.CanCreateTour = canCreateTour;
                }
                ViewBag.Club = club;
                ViewBag.AccountEmail = email;
                return View();
            }
            else
            {
                var response = client.GetAsync($"{ApiUrl}/Club/{id}").Result;
                if (!response.IsSuccessStatusCode)
                {
                    ModelState.AddModelError(string.Empty, "Không thể lấy thông tin câu lạc bộ.");
                    return View();
                }
                var ClubData = response.Content.ReadAsStringAsync().Result;
                var club = JsonConvert.DeserializeObject<ClubDTO>(ClubData);
                ViewBag.Club = club;
                var response2 = client.GetAsync($"{ApiUrl}/Tournament/GetTournamentsByClubId?clubId={club.ClubId}").Result;
                if (response2.StatusCode == HttpStatusCode.NotFound)
                {
                    ViewBag.Tournament = null;
                }
                else if (response2.IsSuccessStatusCode)
                {
                    var tournamentData = response2.Content.ReadAsStringAsync().Result;
                    var tours = JsonConvert.DeserializeObject<List<TournamentDetailDTO>>(tournamentData);
                    ViewBag.Tournament = tours;
                }
                var response3 = client.GetAsync($"{ApiUrl}/Account/GetAccountById/{club.AccountId}").Result;
                if (!response3.IsSuccessStatusCode)
                {
                    ModelState.AddModelError(string.Empty, "Không thể lấy thông tin tài khoản.");
                    return View();
                }
                var AccountData = response3.Content.ReadAsStringAsync().Result;
                var account = JsonConvert.DeserializeObject<AccountDTO>(AccountData);
                ViewBag.AccountEmail = account.Email;
                return View();
            }
        }

        public IActionResult ClubSoloMatch(int? id)
        {
            if (id == null)
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
                var response2 = client.GetAsync($"{ApiUrl}/Club/GetClubByAccountId/?accountID={account.AccountID}").Result;
                if (!response2.IsSuccessStatusCode)
                {
                    if (response2.StatusCode == HttpStatusCode.NotFound)
                    {
                        return View();
                    }
                    else
                    {

                        ModelState.AddModelError(string.Empty, "Không thể lấy thông tin câu lạc bộ.");
                        return View();
                    }
                }
                var ClubData = response2.Content.ReadAsStringAsync().Result;
                var club = JsonConvert.DeserializeObject<ClubDTO>(ClubData);
                var response3 = client.GetAsync($"{ApiUrl}/SoloMatch/ByClub/{club.ClubId}").Result;
                if (response3.StatusCode == HttpStatusCode.NotFound)
                {
                    ViewBag.SoloMatches = null;
                }
                else if (response3.IsSuccessStatusCode)
                {
                    var soloMatchInfoViewModel = new List<SoloMatchInfoViewModel>();
                    var soloMatchData = response3.Content.ReadAsStringAsync().Result;
                    var solomatches = JsonConvert.DeserializeObject<List<SoloMatchDTO>>(soloMatchData);
                    foreach(var solomatch in solomatches)
                    {
                     var  responsePlayer = client.GetAsync($"{ApiUrl}/SoloMatch/GetPlayerForSoloMatch/{solomatch.SoloMatchId}").Result;
                        var playersData = responsePlayer.Content.ReadAsStringAsync().Result;
                        var players = JsonConvert.DeserializeObject<List<PlayerDTO>>(playersData);
                        var player1 = players.FirstOrDefault();
                        var player2 = players.Skip(1).FirstOrDefault();
                        var soloMatchInfo = new SoloMatchInfoViewModel
                        {
                            SoloMatchId = solomatch.SoloMatchId,
                            StartTime = solomatch.StartTime,
                            GameTypeName = GetGameTypeName(solomatch.GameTypeId),
                            ClubId = solomatch.ClubId,
                            player1 = player1.PlayerName,
                            player2 = player2.PlayerName,
                            Description = solomatch.Description,
                            Status = solomatch.Status,
                            Flyer = solomatch.Flyer,
                            RaceTo = solomatch.RaceTo,
                            EndTime = solomatch.EndTime
                        };
                        soloMatchInfoViewModel.Add(soloMatchInfo);
                    }    
                   
                    ViewBag.SoloMatches = soloMatchInfoViewModel;
                }
                ViewBag.Club = club;
                ViewBag.AccountEmail = email;
                return View();
            }
            else
            {
                var response = client.GetAsync($"{ApiUrl}/Club/{id}").Result;
                if (!response.IsSuccessStatusCode)
                {
                    ModelState.AddModelError(string.Empty, "Không thể lấy thông tin câu lạc bộ.");
                    return View();
                }
                var ClubData = response.Content.ReadAsStringAsync().Result;
                var club = JsonConvert.DeserializeObject<ClubDTO>(ClubData);
                ViewBag.Club = club;
                var response2 = client.GetAsync($"{ApiUrl}/SoloMatch/ByClub/{club.ClubId}").Result;
                if (response2.StatusCode == HttpStatusCode.NotFound)
                {
                    ViewBag.SoloMatches = null;
                }
                else if (response2.IsSuccessStatusCode)
                {
                    var soloMatchInfoViewModel = new List<SoloMatchInfoViewModel>();
                    var soloMatchData = response2.Content.ReadAsStringAsync().Result;
                    var solomatches = JsonConvert.DeserializeObject<List<SoloMatchDTO>>(soloMatchData);
                    foreach (var solomatch in solomatches)
                    {
                        var responsePlayer = client.GetAsync($"{ApiUrl}/SoloMatch/GetPlayerForSoloMatch/{solomatch.SoloMatchId}").Result;
                        var playersData = responsePlayer.Content.ReadAsStringAsync().Result;
                        var players = JsonConvert.DeserializeObject<List<PlayerDTO>>(playersData);
                        var player1 = players.FirstOrDefault();
                        var player2 = players.Skip(1).FirstOrDefault();
                        var soloMatchInfo = new SoloMatchInfoViewModel
                        {
                            SoloMatchId = solomatch.SoloMatchId,
                            StartTime = solomatch.StartTime,
                            GameTypeName = GetGameTypeName(solomatch.GameTypeId),
                            ClubId = solomatch.ClubId,
                            player1 = player1.PlayerName,
                            player2 = player2.PlayerName,
                            Description = solomatch.Description,
                            Status = solomatch.Status,
                            Flyer = solomatch.Flyer,
                            RaceTo = solomatch.RaceTo,
                            EndTime = solomatch.EndTime
                        };
                        soloMatchInfoViewModel.Add(soloMatchInfo);
                    }

                    ViewBag.SoloMatches = soloMatchInfoViewModel;
                }
                var response3 = client.GetAsync($"{ApiUrl}/Account/GetAccountById/{club.AccountId}").Result;
                if (!response3.IsSuccessStatusCode)
                {
                    ModelState.AddModelError(string.Empty, "Không thể lấy thông tin tài khoản.");
                    return View();
                }
                var AccountData = response3.Content.ReadAsStringAsync().Result;
                var account = JsonConvert.DeserializeObject<AccountDTO>(AccountData);
                ViewBag.AccountEmail = account.Email;
                return View();
            }

        }
        private string GetGameTypeName(int gameTypeId)
        {
            switch (gameTypeId)
            {
                case 1:
                    return "8 Bi";
                case 2:
                    return "9 Bi";
                case 3:
                    return "10 Bi";
                default:
                    return "Unknown";
            }
        }

        public IActionResult ClubSoloMatchDetail(int? id,int? clubid)
        {
            var soloMatchInfo = new SoloMatchInfoViewModel();
            if (clubid == null)
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
                var response2 = client.GetAsync($"{ApiUrl}/Club/GetClubByAccountId/?accountID={account.AccountID}").Result;
                if (!response2.IsSuccessStatusCode)
                {
                    if (response2.StatusCode == HttpStatusCode.NotFound)
                    {
                        return View();
                    }
                    else
                    {

                        ModelState.AddModelError(string.Empty, "Không thể lấy thông tin câu lạc bộ.");
                        return View();
                    }
                }
                var ClubData = response2.Content.ReadAsStringAsync().Result;
                var club = JsonConvert.DeserializeObject<ClubDTO>(ClubData);
                var response3 = client.GetAsync($"{ApiUrl}/SoloMatch/{id}").Result;
                if (response3.StatusCode == HttpStatusCode.NotFound)
                {
                    ViewBag.ClubPost = null;
                }
                else if (response3.IsSuccessStatusCode)
                {
                    var SoloMatchData = response3.Content.ReadAsStringAsync().Result;
                    var soloMatch = JsonConvert.DeserializeObject<SoloMatchDTO>(SoloMatchData);
                    var responsePlayer = client.GetAsync($"{ApiUrl}/SoloMatch/GetPlayerForSoloMatch/{soloMatch.SoloMatchId}").Result;
                    var playersData = responsePlayer.Content.ReadAsStringAsync().Result;
                    var players = JsonConvert.DeserializeObject<List<PlayerDTO>>(playersData);
                    var player1 = players.FirstOrDefault();
                    var player2 = players.Skip(1).FirstOrDefault();
                    var responseScore1 = client.GetAsync($"{ApiUrl}/SoloMatch/GetScorePlayerForMatch/{soloMatch.SoloMatchId}/{player1.PlayerId}").Result;
                    var responseScore2 = client.GetAsync($"{ApiUrl}/SoloMatch/GetScorePlayerForMatch/{soloMatch.SoloMatchId}/{player2.PlayerId}").Result;
                    var Score1Data = responseScore1.Content.ReadAsStringAsync().Result;
                    var score1 = JsonConvert.DeserializeObject<PlayerInSoloMatchDTO>(Score1Data);
                    var Score2Data = responseScore2.Content.ReadAsStringAsync().Result;
                    var score2 = JsonConvert.DeserializeObject<PlayerInSoloMatchDTO>(Score2Data);
                    ViewBag.Player1 = player1;
                    ViewBag.Player2 = player2;
                    ViewBag.Score1 = score1;
                    ViewBag.Score2 = score2;
                    soloMatchInfo.SoloMatchId = soloMatch.SoloMatchId;
                    soloMatchInfo.StartTime = soloMatch.StartTime;
                    soloMatchInfo.GameTypeName = GetGameTypeName(soloMatch.GameTypeId);
                    soloMatchInfo.ClubId = soloMatch.ClubId;
                    soloMatchInfo.player1 = player1.PlayerName;
                    soloMatchInfo.player2 = player2.PlayerName;
                    soloMatchInfo.Description = soloMatch.Description;
                    soloMatchInfo.Status = soloMatch.Status;
                    soloMatchInfo.Flyer = soloMatch.Flyer;
                    soloMatchInfo.RaceTo = soloMatch.RaceTo;
                    soloMatchInfo.EndTime = soloMatch.EndTime;
                    
                    
                }
                ViewBag.Club = club;
                ViewBag.AccountEmail = email;
                return View(soloMatchInfo);
            }
            else
            {
                var response = client.GetAsync($"{ApiUrl}/Club/{clubid}").Result;
                if (!response.IsSuccessStatusCode)
                {
                    ModelState.AddModelError(string.Empty, "Không thể lấy thông tin câu lạc bộ.");
                    return View();
                }
                var ClubData = response.Content.ReadAsStringAsync().Result;
                var club = JsonConvert.DeserializeObject<ClubDTO>(ClubData);
                ViewBag.Club = club;
                var response2 = client.GetAsync($"{ApiUrl}/SoloMatch/{id}").Result;
                if (response2.StatusCode == HttpStatusCode.NotFound)
                {
                    ViewBag.ClubPost = null;
                }
                else if (response2.IsSuccessStatusCode)
                {
                    var clubPostData = response2.Content.ReadAsStringAsync().Result;
                    var clubPosts = JsonConvert.DeserializeObject<List<ClubPostDTO>>(clubPostData);
                    ViewBag.ClubPost = clubPosts;
                }
                var response3 = client.GetAsync($"{ApiUrl}/Account/GetAccountById/{club.AccountId}").Result;
                if (!response3.IsSuccessStatusCode)
                {
                    ModelState.AddModelError(string.Empty, "Không thể lấy thông tin tài khoản.");
                    return View();
                }
                var AccountData = response3.Content.ReadAsStringAsync().Result;
                var account = JsonConvert.DeserializeObject<AccountDTO>(AccountData);
                ViewBag.AccountEmail = account.Email;
                return View();
            }
        }
        public IActionResult CreateClub()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> CreateClub(ClubDTO clubDTO, IFormFile BannerFile, string ward)
        {
            string firebaseFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Firebase");
            if (!Directory.Exists(firebaseFolder))
            {
                Directory.CreateDirectory(firebaseFolder);
            }
            string email = HttpContext.Request.Cookies["Email"];
            var response = client.GetAsync($"{ApiUrl}/Account/GetAccountByEmail/{email}").Result;
            var AccountData = response.Content.ReadAsStringAsync().Result;
            var account = JsonConvert.DeserializeObject<AccountDTO>(AccountData);
            clubDTO.AccountId = account.AccountID;
            clubDTO.AccountEmail = account.Email;
            clubDTO.Status = 1;
            clubDTO.WardCode = ward;
            if (BannerFile != null && BannerFile.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(BannerFile.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Firebase", fileName);

                using (FileStream memoryStream = new FileStream(filePath, FileMode.Create))
                {
                    BannerFile.CopyTo(memoryStream);


                }
                var fileStream2 = new FileStream(filePath, FileMode.Open);
                var downloadLink = await UploadFromFirebase(fileStream2, BannerFile.FileName, "Club", clubDTO.ClubName, 0);
                fileStream2.Close();
                clubDTO.Avatar = downloadLink;
            }
            string directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Firebase");

            try
            {

                string[] filePaths = Directory.GetFiles(directoryPath);
                foreach (string filePath in filePaths)
                {
                    System.IO.File.Delete(filePath);
                }

                Console.WriteLine("All images in the directory have been deleted successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while deleting images: {ex.Message}");
            }
            clubDTO.ClubId = 1;
            var response2 = await client.PostAsJsonAsync($"{ApiUrl}/Club/Add", clubDTO);

            if (response2.IsSuccessStatusCode)
            {
                return RedirectToAction("Index");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Lỗi khi tạo câu lạc bộ.");
                return View(clubDTO);
            }
        }

        public IActionResult ClubDetails(int id)
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
            var response2 = client.GetAsync($"{ApiUrl}/Club/GetClubByAccountId/?accountID={account.AccountID}").Result;
            if (!response2.IsSuccessStatusCode)
            {
                if (response2.StatusCode == HttpStatusCode.NotFound)
                {
                    return View();
                }
                else
                {

                    ModelState.AddModelError(string.Empty, "Không thể lấy thông tin câu lạc bộ.");
                    return View();
                }
            }
            var ClubData = response2.Content.ReadAsStringAsync().Result;
            var club = JsonConvert.DeserializeObject<ClubDTO>(ClubData);

            if (!string.IsNullOrEmpty(club.WardCode))
            {
                var responseWard = client.GetAsync($"{ApiUrl}/Address/getwardsBywardCode/{club.WardCode}").Result;
                var wardData = responseWard.Content.ReadAsStringAsync().Result;
                var ward = JsonConvert.DeserializeObject<WardDTO>(wardData);

                var responseDistrict = client.GetAsync($"{ApiUrl}/Address/GetdistrictsByDistrictCode/{ward.DistrictCode}").Result;
                var districtData = responseDistrict.Content.ReadAsStringAsync().Result;
                var district = JsonConvert.DeserializeObject<DistrictDTO>(districtData);

                var responseProvince = client.GetAsync($"{ApiUrl}/Address/getProvincesByProvinceCode/{district.ProvinceCode}").Result;
                var provinceData = responseProvince.Content.ReadAsStringAsync().Result;
                var province = JsonConvert.DeserializeObject<ProvinceDTO>(provinceData);

                ViewBag.Club = club;
                ViewBag.AccountEmail = email;
                ViewBag.Ward = ward;
                ViewBag.District = district;
                ViewBag.Province = province;
                return View(club);
            }
            return RedirectToAction("Index");
        }
        [HttpPost]
        public async Task<IActionResult> ClubDetails(ClubDTO ClubDTO, IFormFile BannerFile, string ward)
        {
            string firebaseFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Firebase");
            if (!Directory.Exists(firebaseFolder))
            {
                Directory.CreateDirectory(firebaseFolder);
            }
            if (BannerFile != null && BannerFile.Length > 0)
            {
                await DeleteFromFirebase(ClubDTO.ClubName, ClubDTO.Avatar);
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(BannerFile.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Firebase", fileName);

                using (FileStream memoryStream = new FileStream(filePath, FileMode.Create))
                {
                    BannerFile.CopyTo(memoryStream);


                }
                var fileStream2 = new FileStream(filePath, FileMode.Open);
                var downloadLink = await UploadFromFirebase(fileStream2, BannerFile.FileName, "Club", ClubDTO.ClubName, 0);
                fileStream2.Close();
                ClubDTO.Avatar = downloadLink;
            }
            string directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Firebase");
            try
            {

                string[] filePaths = Directory.GetFiles(directoryPath);
                foreach (string filePath in filePaths)
                {
                    System.IO.File.Delete(filePath);
                }

                Console.WriteLine("All images in the directory have been deleted successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while deleting images: {ex.Message}");
            }
            if (ward != null)
            {
                ClubDTO.WardCode = ward;
            }
            ClubDTO.AccountEmail = "1";
            var response = await client.PostAsJsonAsync($"{ApiUrl}/Club/UpdateClub?id={ClubDTO.ClubId}", ClubDTO);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Chỉnh sửa thông tin câu lạc bộ thành công.";
                return RedirectToAction("Index");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Lỗi khi sửa thông tin câu lạc bộ.");
                return View(ClubDTO);
            }
        }

        public IActionResult ClubTable(int? id)
        {
            if (id == null)
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
                var response2 = client.GetAsync($"{ApiUrl}/Club/GetClubByAccountId/?accountID={account.AccountID}").Result;
                if (!response2.IsSuccessStatusCode)
                {
                    if (response2.StatusCode == HttpStatusCode.NotFound)
                    {
                        return View();
                    }
                    else
                    {

                        ModelState.AddModelError(string.Empty, "Không thể lấy thông tin câu lạc bộ.");
                        return View();
                    }
                }
                var ClubData = response2.Content.ReadAsStringAsync().Result;
                var club = JsonConvert.DeserializeObject<ClubDTO>(ClubData);
                var response3 = client.GetAsync($"{ApiUrl}/Table/GetTablesByClubId/{club.ClubId}").Result;
                if (response3.StatusCode == HttpStatusCode.NotFound)
                {
                    ViewBag.Table = null;
                }
                else if (response3.IsSuccessStatusCode)
                {
                    var TableData = response3.Content.ReadAsStringAsync().Result;
                    var tables = JsonConvert.DeserializeObject<List<TableDTO>>(TableData);
                    var tableCounts = tables
                    .GroupBy(t => t.TableName)
                    .Select(g => new TableInfoViewModel
                    {
                        TableName = g.Key,
                        Quantity = g.Count(),
                        Size = g.First().Size,
                        Price = g.First().Price,
                        Image = g.First().Image,
                    })
                    .ToList();
                    ViewBag.Table = tableCounts;
                }
                ViewBag.Club = club;
                ViewBag.AccountEmail = email;
                return View();
            }
            else
            {
                var response = client.GetAsync($"{ApiUrl}/Club/{id}").Result;
                if (!response.IsSuccessStatusCode)
                {
                    ModelState.AddModelError(string.Empty, "Không thể lấy thông tin câu lạc bộ.");
                    return View();
                }
                var ClubData = response.Content.ReadAsStringAsync().Result;
                var club = JsonConvert.DeserializeObject<ClubDTO>(ClubData);
                ViewBag.Club = club;
                var response2 = client.GetAsync($"{ApiUrl}/Table/GetTablesByClubId/{club.ClubId}").Result;
                if (response2.StatusCode == HttpStatusCode.NotFound)
                {
                    ViewBag.Table = null;
                }
                else if (response2.IsSuccessStatusCode)
                {
                    var TableData = response2.Content.ReadAsStringAsync().Result;
                    var Tables = JsonConvert.DeserializeObject<List<TableDTO>>(TableData);
                    var tableCounts = Tables
                   .GroupBy(t => t.TableName)
                   .Select(g => new TableInfoViewModel
                   {
                       TableName = g.Key,
                       Quantity = g.Count(),
                       Size = g.First().Size,
                       Price = g.First().Price,
                       Image = g.First().Image,
                   })
                    .ToList();
                    ViewBag.Table = tableCounts;
                }
                var response3 = client.GetAsync($"{ApiUrl}/Account/GetAccountById/{club.AccountId}").Result;
                if (!response3.IsSuccessStatusCode)
                {
                    ModelState.AddModelError(string.Empty, "Không thể lấy thông tin tài khoản.");
                    return View();
                }
                var AccountData = response3.Content.ReadAsStringAsync().Result;
                var account = JsonConvert.DeserializeObject<AccountDTO>(AccountData);
                ViewBag.AccountEmail = account.Email;
                return View();

            }
        }
        [HttpPost]
        public async Task<IActionResult> AddTable(TableDTO tableDTO, int clubid, IFormFile Image)
        {
            string firebaseFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Firebase");
            if (!Directory.Exists(firebaseFolder))
            {
                Directory.CreateDirectory(firebaseFolder);
            }
            if (string.IsNullOrEmpty(tableDTO.TableName))
            {
                ModelState.AddModelError("TableName", "Tên bàn không được trống.");
                return RedirectToAction("ClubTableManage");
            }

            if (string.IsNullOrEmpty(tableDTO.TagName))
            {
                ModelState.AddModelError("TagName", "Nhãn bàn không được trống.");
                return RedirectToAction("ClubTableManage");
            }

            if (string.IsNullOrEmpty(tableDTO.Size))
            {
                ModelState.AddModelError("Size", "Kích thước không được trống.");
                return RedirectToAction("ClubTableManage");
            }

            if (tableDTO.Price <= 0)
            {
                ModelState.AddModelError("Price", "Giá giờ chơi phải lớn hơn 0.");
                return RedirectToAction("ClubTableManage");
            }
            if (Image == null)
            {
                ModelState.AddModelError("", "Ảnh không được trống.");
                return RedirectToAction("ClubTableManage");
            }
            var responseTag = client.GetAsync($"{ApiUrl}/Table/IsTagNameExists/{clubid}/{tableDTO.TagName}").Result;
            if (responseTag.IsSuccessStatusCode)
            {
                var responseContent = responseTag.Content.ReadAsStringAsync().Result;

                if (bool.TryParse(responseContent, out var isTagNameExists) && isTagNameExists)
                {
                    ModelState.AddModelError("", "Tên đã tồn tại trong cơ sở dữ liệu.");
                    return RedirectToAction("ClubTableManage");
                }
            }
            else
            {
                ModelState.AddModelError("", "Đã xảy ra lỗi khi kiểm tra tên.");
                return RedirectToAction("ClubTableManage");
            }
            if (Image != null && Image.Length > 0)
            {

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(Image.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Firebase", fileName);

                using (FileStream memoryStream = new FileStream(filePath, FileMode.Create))
                {
                    Image.CopyTo(memoryStream);


                }
                var fileStream2 = new FileStream(filePath, FileMode.Open);
                var downloadLink = await UploadFromFirebase(fileStream2, Image.FileName, "Table", $"{tableDTO.ClubId}", 0);
                fileStream2.Close();
                tableDTO.Image = downloadLink;
            }
            string directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Firebase");
            try
            {

                string[] filePaths = Directory.GetFiles(directoryPath);
                foreach (string filePath in filePaths)
                {
                    System.IO.File.Delete(filePath);
                }

                Console.WriteLine("All images in the directory have been deleted successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while deleting images: {ex.Message}");
            }
            tableDTO.IsUseInTour = false;
            tableDTO.ClubId = clubid;
            tableDTO.IsScheduling = false;
            tableDTO.Status = true;
            var responseData = await client.PostAsJsonAsync($"{ApiUrl}/Table/AddNewTable", tableDTO);
            return RedirectToAction("ClubTableManage");
        }
        [HttpPost]
        public async Task<IActionResult> UpdateTable(int TableIdDetails,int ClubIdDetails,string ImageDetails,bool IsSchedulingDetails,bool StatusDetails,bool IsUseInTourDetails,string TagNameDetails,string TableNameDetails,string SizeDetails,int PriceDetails, IFormFile Image)
        {
            var tableDTO = new TableDTO();
            tableDTO.TableId = TableIdDetails;
            tableDTO.ClubId = ClubIdDetails;
            tableDTO.Image = ImageDetails;
            tableDTO.IsScheduling = IsSchedulingDetails;
            tableDTO.Status = StatusDetails;
            tableDTO.IsUseInTour = IsUseInTourDetails;
            tableDTO.TagName = TagNameDetails;
            tableDTO.Size = SizeDetails;
            tableDTO.Price = PriceDetails;
            tableDTO.TableName = TableNameDetails;

            string firebaseFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Firebase");
            if (!Directory.Exists(firebaseFolder))
            {
                Directory.CreateDirectory(firebaseFolder);
            }

            if (Image != null && Image.Length > 0)
            {
                await DeleteFromFirebase(tableDTO.TableId.ToString(), tableDTO.Image);

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(Image.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Firebase", fileName);

                using (FileStream memoryStream = new FileStream(filePath, FileMode.Create))
                {
                    Image.CopyTo(memoryStream);


                }
                var fileStream2 = new FileStream(filePath, FileMode.Open);
                var downloadLink = await UploadFromFirebase(fileStream2, Image.FileName, "Table", tableDTO.TableId.ToString(), 0);
                fileStream2.Close();
                tableDTO.Image = downloadLink;
            }
           
            try
            {
                string directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Firebase");
                string[] filePaths = Directory.GetFiles(directoryPath);
                foreach (string filePath in filePaths)
                {
                    System.IO.File.Delete(filePath);
                }

                Console.WriteLine("All images in the directory have been deleted successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while deleting images: {ex.Message}");
            }

            var response = client.PostAsJsonAsync($"{ApiUrl}/Table/Update/{tableDTO.TableId}", tableDTO).Result;

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Chỉnh sửa" + tableDTO.TagName + "thành công";
                return RedirectToAction("ClubTableManage");
            }
            else if (response.StatusCode == HttpStatusCode.NotFound)
            {

                return NotFound();
            }
            else
            {

                ModelState.AddModelError(string.Empty, "Lỗi khi cập nhật bàn.");
                return RedirectToAction("ClubTableManage");
            }
            
        }
        [HttpGet]
        public async Task<IActionResult> DeleteTable(int tableid)
        {
            var response = client.GetAsync($"{ApiUrl}/Table/{tableid}").Result;

            var TableData = response.Content.ReadAsStringAsync().Result;
            var tables = JsonConvert.DeserializeObject<TableDTO>(TableData);
            await DeleteFromFirebase($"{tables.ClubId}", tables.Image);
            var response2 = client.GetAsync($"{ApiUrl}/Table/Delete/{tableid}").Result;
            return RedirectToAction("ClubTableManage");
        }

        public IActionResult ClubTableManage(int? id)
        {
            if (id == null)
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
                var response2 = client.GetAsync($"{ApiUrl}/Club/GetClubByAccountId/?accountID={account.AccountID}").Result;
                if (!response2.IsSuccessStatusCode)
                {
                    if (response2.StatusCode == HttpStatusCode.NotFound)
                    {
                        return View();
                    }
                    else
                    {

                        ModelState.AddModelError(string.Empty, "Không thể lấy thông tin câu lạc bộ.");
                        return View();
                    }
                }
                var ClubData = response2.Content.ReadAsStringAsync().Result;
                var club = JsonConvert.DeserializeObject<ClubDTO>(ClubData);
                var response3 = client.GetAsync($"{ApiUrl}/Table/GetTablesByClubId/{club.ClubId}").Result;
                if (response3.StatusCode == HttpStatusCode.NotFound)
                {
                    ViewBag.Table = null;
                }
                else if (response3.IsSuccessStatusCode)
                {
                    var TableData = response3.Content.ReadAsStringAsync().Result;
                    var tables = JsonConvert.DeserializeObject<List<TableDTO>>(TableData);
                    var tableNames = tables.Select(t => t.TableName).Distinct().ToList();
                    ViewBag.TableNames = tableNames;
                    ViewBag.Table = tables;
                }
                ViewBag.Club = club;
                ViewBag.AccountEmail = email;
                return View();
            }
            else
            {
                var response = client.GetAsync($"{ApiUrl}/Club/{id}").Result;
                if (!response.IsSuccessStatusCode)
                {
                    ModelState.AddModelError(string.Empty, "Không thể lấy thông tin câu lạc bộ.");
                    return View();
                }
                var ClubData = response.Content.ReadAsStringAsync().Result;
                var club = JsonConvert.DeserializeObject<ClubDTO>(ClubData);
                ViewBag.Club = club;
                var response2 = client.GetAsync($"{ApiUrl}/Table/GetTablesByClubId/{club.ClubId}").Result;
                if (response2.StatusCode == HttpStatusCode.NotFound)
                {
                    ViewBag.Table = null;
                }
                else if (response2.IsSuccessStatusCode)
                {
                    var TableData = response2.Content.ReadAsStringAsync().Result;
                    var tables = JsonConvert.DeserializeObject<List<TableDTO>>(TableData);
                    ViewBag.Table = tables;
                }
                var response3 = client.GetAsync($"{ApiUrl}/Account/GetAccountById/{club.AccountId}").Result;
                if (!response3.IsSuccessStatusCode)
                {
                    ModelState.AddModelError(string.Empty, "Không thể lấy thông tin tài khoản.");
                    return View();
                }
                var AccountData = response3.Content.ReadAsStringAsync().Result;
                var account = JsonConvert.DeserializeObject<AccountDTO>(AccountData);
                ViewBag.AccountEmail = account.Email;
                return View();

            }
        }
        [HttpPost("ImportTables")]
        public async Task<IActionResult> ImportTables(IFormFile ImportTables,int clubId)
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
            var response2 = client.GetAsync($"{ApiUrl}/Club/GetClubByAccountId/?accountID={account.AccountID}").Result;
            if (!response2.IsSuccessStatusCode)
            {
                if (response2.StatusCode == HttpStatusCode.NotFound)
                {
                    return View();
                }
                else
                {

                    ModelState.AddModelError(string.Empty, "Không thể lấy thông tin câu lạc bộ.");
                    return View();
                }
            }
            var ClubData = response2.Content.ReadAsStringAsync().Result;
            var club = JsonConvert.DeserializeObject<ClubDTO>(ClubData);
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
            try
            {
                if (ImportTables == null || ImportTables.Length <= 0)
                {
                    return View("Error");
                }

                var fileExtension = Path.GetExtension(ImportTables.FileName)?.ToLower();
                var importedTables = new List<TableDTO>();

                if (fileExtension == ".xls" || fileExtension == ".xlsx")
                {
                    using (var package = new ExcelPackage(ImportTables.OpenReadStream()))
                    {
                        var worksheet = package.Workbook.Worksheets[0];

                        for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
                        {
                            var tableName = worksheet.Cells[row, 1].Text?.Trim();
                            var tag = worksheet.Cells[row, 2].Text?.Trim();
                            var size = worksheet.Cells[row, 3].Text?.Trim();
                            var hourlyPriceText = worksheet.Cells[row, 4].Text?.Trim();

                            if (string.IsNullOrEmpty(tableName) || string.IsNullOrEmpty(tag) ||
                                string.IsNullOrEmpty(size) || string.IsNullOrEmpty(hourlyPriceText))
                            {
                                ModelState.AddModelError("", "Dữ liệu nhập thiếu trong tệp.");
                                return RedirectToAction("ClubTableManage");
                            }

                            if (!int.TryParse(hourlyPriceText, out int hourlyPrice))
                            {
                                ModelState.AddModelError("", "Dữ liệu giá tiền không hợp lệ trong tệp.");
                                return RedirectToAction("ClubTableManage");
                            }
                            var responseTag = client.GetAsync($"{ApiUrl}/Table/IsTagNameExists/{clubId}/{tag}").Result;
                            if (responseTag.IsSuccessStatusCode)
                            {
                                var responseContent = responseTag.Content.ReadAsStringAsync().Result;

                                if (bool.TryParse(responseContent, out var isTagNameExists) && isTagNameExists)
                                {
                                    ModelState.AddModelError("", "Tên đã tồn tại trong cơ sở dữ liệu.");
                                    return RedirectToAction("ClubTableManage");
                                }
                            }
                            else
                            {
                                ModelState.AddModelError("", "Đã xảy ra lỗi khi kiểm tra tên.");
                                return RedirectToAction("ClubTableManage");
                            }
                            var table = new TableDTO
                            {
                                TableName = tableName,
                                TagName = tag,
                                Size = size + " Feet",
                                Price = hourlyPrice,
                                IsScheduling = false,
                                IsUseInTour = false,
                                Image = "https://firebasestorage.googleapis.com/v0/b/poolcomvn-82664.appspot.com/o/Table%2Fdefault.jpg?alt=media&token=62241ee6-7893-4bbe-bc1b-09f2072c9df3",
                                ClubId = club.ClubId
                            };

                            importedTables.Add(table);
                        }

                        var responseData = await client.PostAsJsonAsync($"{ApiUrl}/Table/AddListTable", importedTables);
                        return RedirectToAction("ClubTableManage");
                    }
                }
                else
                {
                    return View("ErrorView");
                }
            }
            catch (IOException ex)
            {
                return RedirectToAction("InternalServerError", "Error", new { message = ex.Message });
            }
        }
        public IActionResult CreateSoloMatch(int clubid)
        {
            string email = HttpContext.Request.Cookies["Email"];
            var responseaccount = client.GetAsync($"{ApiUrl}/Account/GetAccountByEmail/{email}").Result;
            if (!responseaccount.IsSuccessStatusCode)
            {
                ModelState.AddModelError(string.Empty, "Không thể lấy thông tin tài khoản.");
                return View();
            }
            var AccountData = responseaccount.Content.ReadAsStringAsync().Result;
            var account = JsonConvert.DeserializeObject<AccountDTO>(AccountData);
            var response2 = client.GetAsync($"{ApiUrl}/Club/GetClubByAccountId/?accountID={account.AccountID}").Result;
            if (!response2.IsSuccessStatusCode)
            {
                if (response2.StatusCode == HttpStatusCode.NotFound)
                {
                    return View();
                }
                else
                {

                    ModelState.AddModelError(string.Empty, "Không thể lấy thông tin câu lạc bộ.");
                    return View();
                }
            }
            var ClubData = response2.Content.ReadAsStringAsync().Result;
            var club = JsonConvert.DeserializeObject<ClubDTO>(ClubData);
            ViewBag.ClubId = club.ClubId;
            var response = client.GetAsync($"{ApiUrl}/Player").Result;
            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(string.Empty, "Không thể lấy danh sách người chơi.");
                return View();
            }
            var PlayerData = response.Content.ReadAsStringAsync().Result;
            var players = JsonConvert.DeserializeObject<List<PlayerDTO>>(PlayerData);
            ViewBag.Players = players;
            ViewBag.Club = club;
            ViewBag.AccountEmail = email;
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> CreateSoloMatches(SoloMatchDTO soloMatchDTO, IFormFile BannerFile,string player1,string player2)
        {
            string firebaseFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Firebase");
            if (!Directory.Exists(firebaseFolder))
            {
                Directory.CreateDirectory(firebaseFolder);
            }
            var player1InSoloMatch = new PlayerInSoloMatchDTO();
            var player2InSoloMatch = new PlayerInSoloMatchDTO();
            soloMatchDTO.Status = 1;
            if (BannerFile != null && BannerFile.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(BannerFile.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Firebase", fileName);

                using (FileStream memoryStream = new FileStream(filePath, FileMode.Create))
                {
                    BannerFile.CopyTo(memoryStream);


                }
                var fileStream2 = new FileStream(filePath, FileMode.Open);
                var downloadLink = await UploadFromFirebase(fileStream2, BannerFile.FileName, "SoloMatch", soloMatchDTO.Description, 1);
                fileStream2.Close();
                soloMatchDTO.Flyer = downloadLink;
            }
            
            string directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Firebase");

            try
            {

                string[] filePaths = Directory.GetFiles(directoryPath);
                foreach (string filePath in filePaths)
                {
                    System.IO.File.Delete(filePath);
                }

                Console.WriteLine("All images in the directory have been deleted successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while deleting images: {ex.Message}");
            }
            var response = await client.PostAsJsonAsync($"{ApiUrl}/SoloMatch", soloMatchDTO);
            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                int newSoloMatchId = JsonConvert.DeserializeObject<int>(responseBody);
                player1InSoloMatch.SoloMatchId = newSoloMatchId;
                player1InSoloMatch.PlayerId = int.Parse(player1);
                player2InSoloMatch.SoloMatchId = newSoloMatchId;
                player2InSoloMatch.PlayerId = int.Parse(player2);
                player1InSoloMatch.Score =0;
                player2InSoloMatch.Score = 0;
            }
            
            var responsePlayer1 = await client.PostAsJsonAsync($"{ApiUrl}/Player/AddPlayerToSoloMatch", player1InSoloMatch);
            var responsePlayer2 = await client.PostAsJsonAsync($"{ApiUrl}/Player/AddPlayerToSoloMatch", player2InSoloMatch);
            return RedirectToAction("CLubSoloMatch");
        }

    }
}
