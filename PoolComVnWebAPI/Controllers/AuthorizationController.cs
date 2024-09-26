using DataAccess;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;

namespace PoolComVnWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthorizationController : ControllerBase
    {
        private readonly TournamentDAO _tournamentDAO;
        private readonly ClubDAO _clubDAO;

        public AuthorizationController(TournamentDAO tournamentDAO, ClubDAO clubDAO)
        {
            _tournamentDAO = tournamentDAO;
            _clubDAO = clubDAO;
        }

        [HttpPost("CheckAuthorization")]
        public IActionResult CheckAuthorization([FromBody]List<int> rolesAccess)
        {
            // Lấy giá trị token từ header
            var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

            // Giải mã token để lấy các claims
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(token) as JwtSecurityToken;

            // Xử lý logic với các claims
            var RoleClaim = jsonToken?.Claims.FirstOrDefault(claim => claim.Type == "Role");

            // Xử lý khi không tìm thấy claim "roleId"
            if (RoleClaim == null)
            {
                return Unauthorized();
            }

            foreach (var role in rolesAccess)
            {
                if (role.ToString().Equals(RoleClaim.Value))
                {
                    return Ok();
                }
            }
            
            // Không có quyền access
            return Forbid();
        }

        [HttpPost("CheckPermission")]
        public IActionResult CheckPermission([FromBody] List<int> rolesAccess)
        {
            // Lấy giá trị token từ header
            var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

            // Giải mã token để lấy các claims
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(token) as JwtSecurityToken;

            // Xử lý logic với các claims
            var RoleClaim = jsonToken?.Claims.FirstOrDefault(claim => claim.Type == "Role");

            // Xử lý khi không tìm thấy claim "roleId"
            if (RoleClaim == null)
            {
                return Unauthorized();
            }

            foreach (var role in rolesAccess)
            {
                if (role.ToString().Equals(RoleClaim.Value))
                {
                    return Ok();
                }
            }

            // Không có quyền access
            return Forbid();
        }

        [HttpGet("CheckPermissionToTour")]
        public IActionResult CheckPermissionToTour(int tourId)
        {
            // Lấy giá trị token từ header
            var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

            // Giải mã token để lấy các claims
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(token) as JwtSecurityToken;

            // Xử lý logic của bạn với các claims
            var roleClaim = jsonToken?.Claims.FirstOrDefault(claim => claim.Type.Equals("Role"));
            var account = jsonToken?.Claims.FirstOrDefault(claim => claim.Type.Equals("Account"));
            if (!Constant.BusinessRole.ToString().Equals(roleClaim.Value))
            {
                return BadRequest("Unauthorize");
            }
            var club = _clubDAO.GetClubByAccountId(Int32.Parse(account.Value));
            int clubId = club.ClubId;
            var tour = _tournamentDAO.GetTournament(tourId);
            bool canAcess = false;
            if (tour.ClubId == clubId)
            {
                canAcess = true;
            }
            return Ok(canAcess);
        }
    }
}
