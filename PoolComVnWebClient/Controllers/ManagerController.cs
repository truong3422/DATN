using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PoolComVnWebClient.Common;
using PoolComVnWebClient.DTO;
using System.Net;
using System.Net.Http.Headers;

namespace PoolComVnWebClient.Controllers
{
    public class ManagerController : Controller
    {

        private readonly HttpClient client = null;
        private string ApiUrl = Constant.ApiUrl;
        public ManagerController()
        {
            client = new HttpClient();
            var contentType = new MediaTypeWithQualityHeaderValue("application/json");
            client.DefaultRequestHeaders.Accept.Add(contentType);
        }
        public IActionResult Index(int? page, string searchQuery)
        {
            try
            {
                var viewModel = new ManagerDTO();
                int pageNumber = page ?? 1;
                int pageSize = 6;
                ViewBag.SearchQuery = searchQuery;
                var responseManageAccount = client.GetAsync($"{ApiUrl}" + "/Account/GetManagerAccounts").Result;

                if (responseManageAccount.IsSuccessStatusCode)
                {
                    var jsonContentManageAccount = responseManageAccount.Content.ReadAsStringAsync().Result;
                    viewModel.Accounts = JsonConvert.DeserializeObject<List<AccountDTO>>(jsonContentManageAccount);
                    searchQuery = searchQuery?.Trim().ToLower();
                    if (!string.IsNullOrEmpty(searchQuery))
                    {
                        viewModel.Accounts = viewModel.Accounts.
                            Where(a => a.Email.ToLower().Contains(searchQuery)).ToList();
                    }
                    
                    if (viewModel.Accounts.Any())
                    {
                        viewModel.PaginatedManagerAccounts = PaginatedList<AccountDTO>.CreateAsync(viewModel.Accounts, pageNumber, pageSize);
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Không tìm thấy!";
                        return RedirectToAction("Index");
                    }
                }
                else
                {
                    return View("Error");
                }

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error: {ex.Message}";
                return View();
            }
        }


        public IActionResult ClubAccount(int? page, string searchQuery)
        {
            try
            {
                var viewModel = new ManagerDTO();
                int pageNumber = page ?? 1;
                int pageSize = 6;
                var responseAccount = client.GetAsync($"{ApiUrl}" + "/Account").Result;
                var responseClub = client.GetAsync($"{ApiUrl}" + "/Club").Result;
                ViewBag.SearchQuery = searchQuery;
                if (responseClub.IsSuccessStatusCode && responseAccount.IsSuccessStatusCode)
                {
                    var jsonContentAccount = responseAccount.Content.ReadAsStringAsync().Result;
                    var jsonContentClub = responseClub.Content.ReadAsStringAsync().Result;
                    var account = JsonConvert.DeserializeObject<IEnumerable<AccountDTO>>(jsonContentAccount);
                    viewModel.Clubs = JsonConvert.DeserializeObject<List<ClubDTO>>(jsonContentClub);
                    foreach (var club in viewModel.Clubs)
                    {
                        var ownerAccount = account.FirstOrDefault(a => a.AccountID == club.AccountId);
                        if (ownerAccount != null)
                        {
                            club.AccountEmail = ownerAccount.Email;
                        }
                    }
                    searchQuery = searchQuery?.Trim().ToLower();

                    if (!string.IsNullOrEmpty(searchQuery))
                    {
                        viewModel.Clubs = viewModel.Clubs.Where(club => club.AccountEmail.ToLower().Contains(searchQuery) || club.ClubName.ToLower().Contains(searchQuery) || club.Address.ToLower().Contains(searchQuery)).ToList();

                    }
                    if (viewModel.Clubs.Any())
                    {
                        viewModel.PaginatedClubAccounts = PaginatedList<ClubDTO>.CreateAsync(viewModel.Clubs, pageNumber, pageSize);
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Không tìm thấy!";
                        return RedirectToAction("ClubAccount");
                    }
                }
                else
                {
                    return View("Error");
                }

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error: {ex.Message}";
                return View();
            }
        }
        public IActionResult UserNormalAccount(int? page, string searchQuery)
        {        
            try
            {
                var viewModel = new ManagerDTO();
                int pageNumber = page ?? 1;
                int pageSize = 6;
                ViewBag.SearchQuery = searchQuery;
                var responseAccount = client.GetAsync($"{ApiUrl}" + "/Account").Result;
                var responseUser = client.GetAsync($"{ApiUrl}" + "/User/GetAllUser").Result;
                if (responseAccount.IsSuccessStatusCode && responseUser.IsSuccessStatusCode)
                {
                    searchQuery = searchQuery?.Trim().ToLower();
                    var jsonContentAccount = responseAccount.Content.ReadAsStringAsync().Result;
                    var jsonContentUser = responseUser.Content.ReadAsStringAsync().Result;
                    var account = JsonConvert.DeserializeObject<IEnumerable<AccountDTO>>(jsonContentAccount);
                    viewModel.Users = JsonConvert.DeserializeObject<List<UserDTO>>(jsonContentUser);
                    foreach (var user in viewModel.Users)
                    {
                        var ownerAccount = account.FirstOrDefault(a => a.AccountID == user.AccountId);
                        if (ownerAccount != null)
                        {
                            user.AccountEmail = ownerAccount.Email;
                            user.PhoneNumber = ownerAccount.PhoneNumber;
                            user.Status = ownerAccount.Status;
                        }
                    }               

                    if (!string.IsNullOrEmpty(searchQuery))
                    {
                        viewModel.Users = viewModel.Users.Where(u => u.AccountEmail.ToLower().Contains(searchQuery) || u.FullName.ToLower().Contains(searchQuery) || u.Address.ToLower().Contains(searchQuery)).ToList();

                    }
                    if (viewModel.Users.Any())
                    {
                        viewModel.PaginatedUserAccounts = PaginatedList<UserDTO>.CreateAsync(viewModel.Users, pageNumber, pageSize);

                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Không tìm thấy!";
                        return RedirectToAction("UserNormalAccount");
                    }
                }
                else
                {
                    return View("Error");
                }

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error: {ex.Message}";
                return View();
            }
        }

        public IActionResult CreateManageAccount()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateManageAccount(CreateManageAccountDTO businessDTO)
        {
            var responseAccount = await client.GetAsync($"{ApiUrl}" + "/Account");
            if (!responseAccount.IsSuccessStatusCode)
            {
                ModelState.AddModelError(string.Empty, "Không thể lấy thông tin tài khoản.");
                return View();
            }
            var accountList = JsonConvert.DeserializeObject<List<AccountDTO>>(await responseAccount.Content.ReadAsStringAsync());

            // Kiểm tra xem email đã tồn tại trong danh sách account hay chưa
            if (accountList.Any(a => a.Email == businessDTO.Email))
            {
                TempData["ErrorMessage"] = "Tài khoản đã tồn tại.";
                return RedirectToAction("CreateManageAccount");
            }

            if (businessDTO.NewPassword != businessDTO.ConfirmNewPassword)
            {
                TempData["ErrorMessage"] = "Mật khẩu mới và xác nhận mật khẩu mới không khớp.";
                return RedirectToAction("CreateManageAccount");
            }
            try
            {
                var response = await client.PostAsJsonAsync(ApiUrl + "/Account/CreateBusinessManagerAccount", businessDTO);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Add new business manager successfully!";
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to add new business manager.";
                    return View();
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error: {ex.Message}";
                return View();
            }
        }
    }
}
