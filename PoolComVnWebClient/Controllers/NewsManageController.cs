using Firebase.Auth;
using Firebase.Storage;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PoolComVnWebClient.Common;
using PoolComVnWebClient.DTO;
using System.Net;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;

namespace PoolComVnWebClient.Controllers
{
    public class NewsManageController : Controller
    {
        private readonly HttpClient client = null;
        private string ApiUrl = Constant.ApiUrl;
        private string ApiKey = FirebaseAPI.ApiKey;
        private string Bucket = FirebaseAPI.Bucket;
        private string AuthEmail = FirebaseAPI.AuthEmail;
        private string AuthPassword = FirebaseAPI.AuthPassword;

        public NewsManageController()
        {
            client = new HttpClient();
            var contentType = new MediaTypeWithQualityHeaderValue("application/json");
            client.DefaultRequestHeaders.Accept.Add(contentType);
            ApiUrl = ApiUrl + "/News";
        }


        public IActionResult Index(int? page, string searchQuery)
        {
            string firebaseFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Firebase");
            if (!Directory.Exists(firebaseFolder))
            {
                Directory.CreateDirectory(firebaseFolder);
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
            string email = HttpContext.Request.Cookies["Email"];
            var responseAccount = client.GetAsync($"https://localhost:5000/api/Account/GetAccountByEmail/{email}").Result;
            var AccountData = responseAccount.Content.ReadAsStringAsync().Result;
            var account = JsonConvert.DeserializeObject<AccountDTO>(AccountData);
            ViewBag.Account = account;
            int pageNumber = page ?? 1;
            int pageSize = 6;

            var filteredNewsList = GetFilteredNewsList(searchQuery);
            ViewBag.SearchQuery = searchQuery;
            var paginatedNewsList = PaginatedList<NewsDTO>.CreateAsync(filteredNewsList, pageNumber, pageSize);

            return View(paginatedNewsList);
        }

        private List<NewsDTO> GetFilteredNewsList(string searchQuery)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchQuery))
                {
                    var response = client.GetAsync($"{ApiUrl}").Result;
                    if (response.IsSuccessStatusCode)
                    {
                        var jsonContent = response.Content.ReadAsStringAsync().Result;
                        var newsList = JsonConvert.DeserializeObject<List<NewsDTO>>(jsonContent);
                        return newsList;
                    }
                    else
                    {              
                        return new List<NewsDTO>();
                    }
                }
                else
                {
                    var response = client.GetAsync($"{ApiUrl}/Search?searchQuery={searchQuery}").Result;
                    if (response.IsSuccessStatusCode)
                    {
                        var jsonContent = response.Content.ReadAsStringAsync().Result;
                        var newsList = JsonConvert.DeserializeObject<List<NewsDTO>>(jsonContent);
                        return newsList;
                    }
                    else
                    {
                        return new List<NewsDTO>();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while getting filtered news list: {ex.Message}");
                return new List<NewsDTO>();
            }
        }

     
        [HttpPost]
        public ActionResult UploadImage(List<IFormFile> files)
        {
            var filepath2 = "";
            string firebaseFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Firebase");
            if (!Directory.Exists(firebaseFolder))
            {
                Directory.CreateDirectory(firebaseFolder);
            }
            foreach (IFormFile file in Request.Form.Files)
            {

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
                var folderPath = $"News/{title}/{filename}";


                await storage.Child(folderPath).DeleteAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occurred during deletion: {0}", ex);
            }
        }
        public IActionResult AddNews()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddNews(NewsDTO newsDTO, IFormFile BannerFile)
        {
            newsDTO.CreatedDate = DateTime.Now;
            newsDTO.UpdatedDate = DateTime.Now;
            newsDTO.Status = true;
            newsDTO.AccId = 4;

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
                var downloadLink = await UploadFromFirebase(fileStream2, BannerFile.FileName, "News", newsDTO.Title, 0);
                fileStream2.Close();
                newsDTO.Flyer = downloadLink;
            }
            int index = 1;
            string pattern = @"<img.*?src=""(.*?)"".*?>";
            MatchCollection matches = Regex.Matches(newsDTO.Description, pattern);
            foreach (Match match in matches)
            {
                string src = match.Groups[1].Value;
                string filenameWithoutFirebase = src.Replace("/Firebase/", "");
                string absolutePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Firebase", filenameWithoutFirebase);
                var fileStream2 = new FileStream(absolutePath, FileMode.Open);
                var downloadLink = await UploadFromFirebase(fileStream2, filenameWithoutFirebase, "News", newsDTO.Title, index);
                index++;
                fileStream2.Close();
                newsDTO.Description = newsDTO.Description.Replace(src, downloadLink);
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
            newsDTO.NewsId = 1;
            var response = await client.PostAsJsonAsync($"{ApiUrl}/Add", newsDTO);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Index");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Lỗi khi thêm tin tức.");
                return View(newsDTO);
            }
        }

        [HttpGet]
        public IActionResult NewsDetails(int id)
        {

            var response = client.GetAsync($"{ApiUrl}/{id}").Result;

            if (response.IsSuccessStatusCode)
            {
                var jsonContent = response.Content.ReadAsStringAsync().Result;
                var newsDetails = JsonConvert.DeserializeObject<NewsDTO>(jsonContent);
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


        [HttpPost]
        public async Task<IActionResult> EditNews(NewsDTO updatedNewsDTO, IFormFile BannerFile)
        {
            string firebaseFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Firebase");
            if (!Directory.Exists(firebaseFolder))
            {
                Directory.CreateDirectory(firebaseFolder);
            }

            updatedNewsDTO.UpdatedDate = DateTime.Now;
            if (BannerFile != null && BannerFile.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(BannerFile.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Firebase", fileName);

                using (FileStream memoryStream = new FileStream(filePath, FileMode.Create))
                {
                    BannerFile.CopyTo(memoryStream);


                }
                var fileStream2 = new FileStream(filePath, FileMode.Open);
                var downloadLink = await UploadFromFirebase(fileStream2, BannerFile.FileName, "News", updatedNewsDTO.Title, 0);
                fileStream2.Close();
                updatedNewsDTO.Flyer = downloadLink;
            }
            int index = 1;
            string pattern = @"<img.*?src=""(.*?)"".*?>";
            MatchCollection matches = Regex.Matches(updatedNewsDTO.Description, pattern);
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
                var downloadLink = await UploadFromFirebase(fileStream2, filenameWithoutFirebase, "News", updatedNewsDTO.Title, index);
                index++;
                fileStream2.Close();
                updatedNewsDTO.Description = updatedNewsDTO.Description.Replace(src, downloadLink);
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

            var response = client.PostAsJsonAsync($"{ApiUrl}/Update", updatedNewsDTO).Result;

            if (response.IsSuccessStatusCode)
            {

                return RedirectToAction("Index");
            }
            else if (response.StatusCode == HttpStatusCode.NotFound)
            {

                return NotFound();
            }
            else
            {

                ModelState.AddModelError(string.Empty, "Lỗi khi cập nhật tin tức.");
                return View(updatedNewsDTO);
            }
        }

        [HttpGet]
        public async Task<IActionResult> DeleteBanner(int id)
        {
            var response = client.GetAsync($"{ApiUrl}/{id}").Result;

            if (response.IsSuccessStatusCode)
            {
                var jsonContent = response.Content.ReadAsStringAsync().Result;
                var newsDetails = JsonConvert.DeserializeObject<NewsDTO>(jsonContent);
                await DeleteFromFirebase(newsDetails.Title, "Banner");
                newsDetails.Flyer = null;
                var response2 = client.PostAsJsonAsync($"{ApiUrl}/Update", newsDetails).Result;
                if (response2.IsSuccessStatusCode)
                {

                    return RedirectToAction("NewsDetails", new { id = newsDetails.NewsId });
                }
                else if (response.StatusCode == HttpStatusCode.NotFound)
                {

                    return NotFound();
                }
                else
                {

                    ModelState.AddModelError(string.Empty, "Lỗi khi cập nhật tin tức.");
                    return RedirectToAction("NewsDetails", newsDetails);
                }
            }

            return null;



        }

        [HttpGet]
        public IActionResult ChangeStatusNews(int id)
        {

            var apiUrl = $"{ApiUrl}/ChangeStatus?id={id}";

            var response = client.PostAsync(apiUrl, null).Result;

            if (response.IsSuccessStatusCode)
            {

                return RedirectToAction("Index");
            }
            else if (response.StatusCode == HttpStatusCode.NotFound)
            {

                return NotFound();
            }
            else
            {

                ModelState.AddModelError(string.Empty, "Lỗi khi xóa tin tức.");
                return View();
            }
        }
    }
}
