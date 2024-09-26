using Microsoft.AspNetCore.Mvc;
using PoolComVnWebClient.DTO;
using PoolComVnWebClient.Common;
using System.Net.Http.Headers;
using Newtonsoft.Json;

namespace PoolComVnWebClient.Controllers
{
    public class TournamentController : Controller
    {
        private readonly HttpClient client = null;
        private string ApiUrl = Constant.ApiUrl;

        public TournamentController()
        {
            client = new HttpClient();
            var contentType = new MediaTypeWithQualityHeaderValue("application/json");
            client.DefaultRequestHeaders.Accept.Add(contentType);
            ApiUrl = ApiUrl + "/Tournament";
        }

        [HttpGet]
        public async Task<IActionResult> TournamentList(int? page, string searchQuery, string? gameType, DateTime? startDate, DateTime? endDate)
        {
            try
            {
                string email = HttpContext.Request.Cookies["Email"];
                var responseAccount = client.GetAsync($"https://localhost:5000/api/Account/GetAccountByEmail/{email}").Result;
                var AccountData = responseAccount.Content.ReadAsStringAsync().Result;
                var account = JsonConvert.DeserializeObject<AccountDTO>(AccountData);
                ViewBag.Account = account;
                int pageNumber = page ?? 1;
                int pageSize = 6;
                ViewBag.SearchQuery = searchQuery;
                ViewBag.GameType = gameType;
                ViewBag.StartDate = startDate;
                ViewBag.EndDate = endDate;
                List<TournamentOutputDTO> tournamentsList = null;

                // Kiểm tra nếu có thông số lọc được cung cấp
                if (!string.IsNullOrEmpty(gameType) || startDate.HasValue || endDate.HasValue)
                {
                    tournamentsList = await GetFilteredTournamentsListAsync(gameType, startDate, endDate);
                }
                else if (!string.IsNullOrEmpty(searchQuery))
                {
                    tournamentsList = await GetSearchTournamentsListAsync(searchQuery);
                    
                }
                else
                {
                    var response = await client.GetAsync(ApiUrl + "/GetAllTour");
                    if (response.IsSuccessStatusCode)
                    {
                        tournamentsList = await response.Content.ReadFromJsonAsync<List<TournamentOutputDTO>>();
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Lỗi khi lấy danh sách giải đấu từ API");
                        return RedirectToAction("InternalServerError", "Error");
                    }

                }
                if (tournamentsList != null)
                {
                    var paginatedTournamentsList = PaginatedList<TournamentOutputDTO>.CreateAsync(tournamentsList, pageNumber, pageSize);
                    return View(paginatedTournamentsList);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Lỗi khi lấy danh sách giải đấu từ API");
                    return RedirectToAction("InternalServerError", "Error");
                }
            }
            catch (HttpRequestException ex)
            {
                ModelState.AddModelError(string.Empty, "Lỗi khi kết nối đến API: " + ex.Message);
                return RedirectToAction("InternalServerError", "Error");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Lỗi khi lấy danh sách giải đấu: " + ex.Message);
                return RedirectToAction("InternalServerError", "Error");
            }
        }

        private async Task<List<TournamentOutputDTO>> GetFilteredTournamentsListAsync(string gameType, DateTime? startDate, DateTime? endDate)
        {
            try
            {
                string apiUrl = ApiUrl + "/FilterTournaments?";
                if (!string.IsNullOrEmpty(gameType) && gameType != "Thể loại")
                    apiUrl += $"gameTypeName={gameType}&";
                if (startDate.HasValue)
                    apiUrl += $"startDate={startDate.Value.ToString("MM-dd-yyyy")}&";
                if (endDate.HasValue)
                    apiUrl += $"endDate={endDate.Value.ToString("MM-dd-yyyy")}&";

                // Gửi yêu cầu tới API filter
                var response = await client.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    List<TournamentOutputDTO> lstTour = await response.Content.ReadFromJsonAsync<List<TournamentOutputDTO>>();
                    return lstTour;
                }
                else
                {
                    var status = response.StatusCode;
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while getting filtered tournaments list: {ex.Message}");
                return null;
            }
        }


        private async Task<List<TournamentOutputDTO>> GetSearchTournamentsListAsync(string searchQuery)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchQuery))
                {
                    var response = await client.GetAsync(ApiUrl + "/GetAllTour");
                    if (response.IsSuccessStatusCode)
                    {
                        List<TournamentOutputDTO> lstTour = await response.Content.ReadFromJsonAsync<List<TournamentOutputDTO>>();
                        return lstTour;
                    }
                    else
                    {
                        var status = response.StatusCode;
                        return null;
                    }
                }
                else
                {
                    var response = await client.GetAsync(ApiUrl + $"/SearchTournament?searchQuery={searchQuery}");
                    if (response.IsSuccessStatusCode)
                    {
                        List<TournamentOutputDTO> lstTour = await response.Content.ReadFromJsonAsync<List<TournamentOutputDTO>>();
                        return lstTour;
                    }
                    else
                    {
                        var status = response.StatusCode;
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while getting search tournaments list: {ex.Message}");
                return null;
            }
        }

        public async Task<IActionResult> TournamentDetailForManager(int tourId)
        {
            TournamentDetailDTO tourDetail = new TournamentDetailDTO();
            var responseGetTourdetail = await client.GetAsync(ApiUrl + "/GetTournament?tourId=" + tourId);
            if (responseGetTourdetail.IsSuccessStatusCode)
            {
                tourDetail = await responseGetTourdetail.Content.ReadFromJsonAsync<TournamentDetailDTO>();
                ViewBag.TourId = tourId;
                ViewBag.TournamentDetail = tourDetail;
            }
            else
            {
                var status = responseGetTourdetail.StatusCode;
                return RedirectToAction("InternalServerError", "Error");
            }

            var statisticResponse = await client.GetAsync(ApiUrl + "/GetTourDetailForManager?tourId=" + tourId);
            if (statisticResponse.IsSuccessStatusCode)
            {
                var statistic = await statisticResponse.Content.ReadFromJsonAsync<TourDetailForManagerDTO>();
                ViewBag.Statistic = statistic;
                ViewBag.NumberOfPlayer = statistic.GiveUpPlayer + 
                    statistic.EliminatedPlayer + statistic.NormalPlayer;
            }
            else
            {
                var status = responseGetTourdetail.StatusCode;
                return RedirectToAction("InternalServerError", "Error");
            }
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> TournamentDetail(int tourId)
        {
            Authorize authorize = new Authorize();
            bool isManager = authorize.CheckPermissionToTour(HttpContext, tourId).Result;
            if (isManager)
            {
                return RedirectToAction("TournamentDetailForManager", "Tournament", new { tourId = tourId });
            }
            string emailCookie = HttpContext.Request.Cookies["Email"];
            ViewBag.UserEmail = emailCookie;
            TournamentDetailDTO tourDetail = new TournamentDetailDTO();
            ViewBag.TourId = tourId;
            var responseGetTourdetail = await client.GetAsync(ApiUrl + "/GetTournament?tourId=" + tourId);
            if (responseGetTourdetail.IsSuccessStatusCode)
            {
                tourDetail = await responseGetTourdetail.Content.ReadFromJsonAsync<TournamentDetailDTO>();
                ViewBag.TournamentDetail = tourDetail;
            }
            else
            {
                var status = responseGetTourdetail.StatusCode;
                return RedirectToAction("InternalServerError", "Error");
            }
            List<PlayerDTO> lstPlayer;
            var responsePlayerList = client.GetAsync($"{Constant.ApiUrl}/Player/GetPlayerExBotByTourId?tourId={tourId}").Result;
            if (responsePlayerList.IsSuccessStatusCode)
            {
                var PlayerData = responsePlayerList.Content.ReadAsStringAsync().Result;
                var PlayerList = JsonConvert.DeserializeObject<List<PlayerDTO>>(PlayerData);

                ViewBag.PlayerLst = PlayerList;
                ViewBag.Top1Player = PlayerList.FirstOrDefault(p => p.Ranking == "1");
                ViewBag.Top2Player = PlayerList.FirstOrDefault(p => p.Ranking == "2");
                ViewBag.Top3_4Player = PlayerList.Where(p => p.Ranking == "3-4").ToList();

            }
            else
            {
                var status = responseGetTourdetail.StatusCode;
                return RedirectToAction("InternalServerError", "Error");
            }
            var response = client.GetAsync(Constant.ApiUrl +$"/Account/GetAccountByEmail/{emailCookie}").Result;
            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(string.Empty, "Không thể lấy thông tin tài khoản.");
                return View();
            }
            var AccountData = response.Content.ReadAsStringAsync().Result;
            var account = JsonConvert.DeserializeObject<AccountDTO>(AccountData);
            if(account.RoleID == 2)
            {
                var responsePlayerInTour = await client.GetAsync(Constant.ApiUrl + "/Player/GetPlayerInTournament?userEmail=" + emailCookie + "&tourId=" + tourId);
                if (responsePlayerInTour.IsSuccessStatusCode)
                {
                    PlayerDTO player = await responsePlayerInTour.Content.ReadFromJsonAsync<PlayerDTO>();
                    ViewBag.Player = player;
                }
                ViewBag.Role = 2;
            }
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> TournamentBracketForManager(int tourId)
        {
            var tokenFromCookie = HttpContext.Request.Cookies["TokenJwt"];
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenFromCookie);
            var response = await client
                .GetAsync(Constant.ApiUrl + "/Tournament/GetTourKnockoutNumber?tourId=" + tourId);
            if (response.IsSuccessStatusCode && response.StatusCode == System.Net.HttpStatusCode.NoContent)
            {
                return RedirectToAction("TournamentSingleBracketForManager", "Tournament", new { tourId = tourId });
            }
            int knockOutNumber = await response.Content.ReadFromJsonAsync<int>();
            int maxNumberOfTournament = await client
                .GetFromJsonAsync<int>(Constant.ApiUrl + "/Tournament/GetTourMaxNumberOfPlayer?tourId=" + tourId);
            int tourStatus = await client
                .GetFromJsonAsync<int>(Constant.ApiUrl + "/Tournament/GetTourStatus?tourId=" + tourId);
            int clubId = await client
                .GetFromJsonAsync<int>(Constant.ApiUrl + "/Club/GetClubId");
            ViewBag.ClubId = clubId;
            ViewBag.TourStatus = tourStatus;
            ViewBag.MaxNumberOfTournament = maxNumberOfTournament;
            ViewBag.KnockOutNumber = knockOutNumber;
            ViewBag.TourId = tourId;
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> TournamentSingleBracketForManager(int tourId)
        {
            var tokenFromCookie = HttpContext.Request.Cookies["TokenJwt"];
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenFromCookie);
            int maxNumberOfTournament = await client
                .GetFromJsonAsync<int>(Constant.ApiUrl + "/Tournament/GetTourMaxNumberOfPlayer?tourId=" + tourId);
            int tourStatus = await client
                .GetFromJsonAsync<int>(Constant.ApiUrl + "/Tournament/GetTourStatus?tourId=" + tourId);
            int clubId = await client
                .GetFromJsonAsync<int>(Constant.ApiUrl + "/Club/GetClubId");
            ViewBag.ClubId = clubId;
            ViewBag.TourStatus = tourStatus;
            ViewBag.MaxNumberOfTournament = maxNumberOfTournament;
            ViewBag.TourId = tourId;
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> TournamentBracket(int tourId)
        {
            Authorize authorize = new Authorize();
            bool isManager = authorize.CheckPermissionToTour(HttpContext, tourId).Result;
            if (isManager)
            {
                return RedirectToAction("TournamentBracketForManager", "Tournament", new {tourId = tourId});
            }

            var response = await client
                .GetAsync(Constant.ApiUrl + "/Tournament/GetTourKnockoutNumber?tourId=" + tourId);
            if (response.IsSuccessStatusCode && response.StatusCode == System.Net.HttpStatusCode.NoContent)
            {
                return RedirectToAction("TournamentSingleBracket", "Tournament", new { tourId = tourId });
            }
            int knockOutNumber = await response.Content.ReadFromJsonAsync<int>();
            int maxNumberOfTournament = await client
               .GetFromJsonAsync<int>(Constant.ApiUrl + "/Tournament/GetTourMaxNumberOfPlayer?tourId=" + tourId);
            
            ViewBag.MaxNumberOfTournament = maxNumberOfTournament;
            ViewBag.KnockOutNumber = knockOutNumber;
            ViewBag.TourId = tourId;
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> TournamentSingleBracket(int tourId)
        {
            int maxNumberOfTournament = await client
                .GetFromJsonAsync<int>(Constant.ApiUrl + "/Tournament/GetTourMaxNumberOfPlayer?tourId=" + tourId);
            ViewBag.MaxNumberOfTournament = maxNumberOfTournament;
            ViewBag.TourId = tourId;
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> TournamentMatchList(int tourId)
        {
            ViewBag.TourId = tourId;
            var responseMatchList = client.GetAsync($"{Constant.ApiUrl}/MatchOfTour/GetMatchForBracket?tourId={tourId}").Result;
            var MatchData = responseMatchList.Content.ReadAsStringAsync().Result;
            var MatchList = JsonConvert.DeserializeObject<List<MatchOfTournamentOutputDTO>>(MatchData);
            var matchedMatches = MatchList
            .GroupBy(match => match.MatchCode)
             .ToList();

            return View(matchedMatches);
        }

        [HttpGet]
        public IActionResult TournamentUpcoming(int tourId)
        {
            ViewBag.TourId = tourId;
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> TournamentPlayers(int tourId)
        {
            ViewBag.TourId = tourId;
            var responsePlayerList = client.GetAsync($"{Constant.ApiUrl}/Player/GetPlayerExBotByTourId?tourId={tourId}").Result;
            var PlayerData = responsePlayerList.Content.ReadAsStringAsync().Result;
            var PlayerList = JsonConvert.DeserializeObject<List<PlayerDTO>>(PlayerData);

            ViewBag.Players = PlayerList;
            return View();
        }

        [HttpGet]
        public IActionResult TournamentMatchListForManager(int tourId)
        {
            ViewBag.TourId = tourId;
            return View();
        }
    }
}
