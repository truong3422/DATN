using Microsoft.AspNetCore.Authorization;
using System.Net.Http.Headers;

namespace PoolComVnWebClient.Common
{
    public class Authorize
    {

        public Authorize()
        {
        }

        public async Task<bool> CheckAuthentication(HttpContext httpContext)
        {
            var tokenFromCookie = httpContext.Request.Cookies["TokenJwt"];
            if (string.IsNullOrEmpty(tokenFromCookie))
            {
                httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return false;
            }

            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenFromCookie);

            // Gửi yêu cầu để kiểm tra quyền truy cập
            var response = await client.GetAsync(Constant.ApiUrl + "/Authorization/CheckAuthorization");

            // Kiểm tra xem có quyền truy cập hay không
            if (!response.IsSuccessStatusCode)
            {
                httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return false;
            }

            return true;
        }

        public async Task<bool> CheckPermissionToTour(HttpContext httpContext, int tourId)
        {
            var tokenFromCookie = httpContext.Request.Cookies["TokenJwt"];
            if (string.IsNullOrEmpty(tokenFromCookie))
            {
                httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return false;
            }

            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenFromCookie);

            // Gửi yêu cầu để kiểm tra quyền truy cập
            var response = await client.GetAsync(Constant.ApiUrl + "/Authorization/CheckPermissionToTour?tourId=" + tourId);

            // Kiểm tra xem có quyền truy cập hay không
            if (response.IsSuccessStatusCode)
            {
                bool result = await response.Content.ReadFromJsonAsync<bool>();
                return result;
            }

            return false;
        }
    }
}
