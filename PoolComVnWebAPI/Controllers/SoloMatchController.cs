using BusinessObject.Models;
using DataAccess;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PoolComVnWebAPI.DTO;

namespace PoolComVnWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SoloMatchController : ControllerBase
    {
        private readonly SoloMatchDAO _soloMatchDAO;
        public SoloMatchController(SoloMatchDAO soloMatchDAO)
        {
            _soloMatchDAO = soloMatchDAO;
        }
        [HttpGet("{solomatchId}")]
        public ActionResult<SoloMatchDTO> GetSoloMatch(int solomatchId)
        {
            var soloMatch = _soloMatchDAO.GetSoloMatchById(solomatchId);

            if (soloMatch == null)
            {
                return NotFound();
            }

            var soloMatchDTO = MapToDTO(soloMatch);
            return soloMatchDTO;
        }
        [HttpGet("GetPlayerForSoloMatch/{soloMatchId}")]
        public ActionResult<List<PlayerDTO>> GetPlayer(int soloMatchId)
        {
            var players = _soloMatchDAO.GetPlayerForSoloMatch(soloMatchId);

            if (players == null)
            {
                return NotFound();
            }
            var playerDTOs = new List<PlayerDTO>();
            foreach (var player in players)
            {
                var playerDTO = new PlayerDTO
                {
                    PlayerId = player.PlayerId,
                    PlayerName = player.PlayerName,
                    CountryId = player.CountryId,
                    Level = player.Level,
                    UserId = player.UserId,
                    TourId = player.TourId,
                    PhoneNumber = player.PhoneNumber,
                    Email = player.Email,
                    IsPayed = player.IsPayed
                };
                playerDTOs.Add(playerDTO);
            }
            return Ok(playerDTOs);
        }
        [HttpPost("UpdateScoreForSoloMatch")]
        public ActionResult UpdateScoreForSoloMatch([FromBody] UpdateScoreDTO updateScoreDTO)
        {
            try
            {

                foreach (var playerDTO in updateScoreDTO.Scores)
                {
                    var player = _soloMatchDAO.GetPlayerInMatch(playerDTO.PlayerSoloMatchId);
                    player.Score = playerDTO.Score;
                    player.GameWin = playerDTO.GameWin;
                    _soloMatchDAO.UpdateScore(player);
                }
                return Ok();

            }
            catch (Exception ex)
            {

                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
            

        }
        [HttpPost("FinishMatch")]
        public ActionResult FinishMatch([FromBody] PlayerInSoloMatchDTO playerInSoloMatchDTO)
        {
            try
            {
                if(playerInSoloMatchDTO.IsWinner == true)
                {
                    var player = _soloMatchDAO.GetPlayerInMatch(playerInSoloMatchDTO.PlayerSoloMatchId);
                    player.Score = playerInSoloMatchDTO.Score;
                    player.GameWin = playerInSoloMatchDTO.GameWin;
                    _soloMatchDAO.UpdateScore(player);
                    var solomatch =  _soloMatchDAO.GetSoloMatchById(playerInSoloMatchDTO.SoloMatchId);
                    solomatch.Status = 1;
                    solomatch.EndTime = DateTime.Now;
                    _soloMatchDAO.FinishMatch(solomatch);
                }    
                return Ok();
            }
            catch (Exception ex)
            {

                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
        [HttpGet("GetScorePLayerForMatch/{soloMatchId}/{playerId}")]
        public ActionResult<PlayerInSoloMatchDTO> GetScorePLayerForMatch(int soloMatchId,int playerId)
        {
            var score = _soloMatchDAO.GetScorePLayerForMatch(soloMatchId, playerId);
            if (score == null)
            {
                return NotFound();
            }
            var scoreDTO = new PlayerInSoloMatchDTO
            {
                PlayerSoloMatchId = score.PlayerSoloMatchId,
                SoloMatchId = score.SoloMatchId,
                PlayerId = score.PlayerId,
                Score = score.Score,
                GameWin = score.GameWin,
                IsWinner = score.IsWinner
            };
          return Ok(score);
        }

        [HttpGet("ByClub/{clubID}")]
        public ActionResult<List<SoloMatchDTO>> GetAllSoloMatchByClubID(int clubID)
        {
            var soloMatches = _soloMatchDAO.GetAllSoloMatchByClubID(clubID);

            if (soloMatches == null || soloMatches.Count == 0)
            {
                return NotFound("No solo matches found for the provided club ID.");
            }

            var soloMatchDTOs = soloMatches.Select(s => MapToDTO(s)).ToList();
            return soloMatchDTOs;
        }

        [HttpPost]
        public IActionResult AddSoloMatch(SoloMatchDTO soloMatchDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var soloMatch = MapToEntity(soloMatchDTO);

            _soloMatchDAO.AddSoloMatch(soloMatch);

            return Ok(_soloMatchDAO.GetLastestSoloMatch().SoloMatchId);
        }
        private SoloMatchDTO MapToDTO(SoloMatch soloMatch)
        {
            return new SoloMatchDTO
            {
                SoloMatchId = soloMatch.SoloMatchId,
                StartTime = soloMatch.StartTime,
                GameTypeId = soloMatch.GameTypeId,
                ClubId = soloMatch.ClubId,
                Description = soloMatch.Description,
                Status = soloMatch.Status,
                Flyer = soloMatch.Flyer,
                RaceTo = soloMatch.RaceTo,
                EndTime = soloMatch.EndTime
            };
        }
        private SoloMatch MapToEntity(SoloMatchDTO soloMatchDTO)
        {
            return new SoloMatch
            {
                StartTime = soloMatchDTO.StartTime,
                GameTypeId = soloMatchDTO.GameTypeId,
                ClubId = soloMatchDTO.ClubId,
                Description = soloMatchDTO.Description,
                Status = soloMatchDTO.Status,
                Flyer = soloMatchDTO.Flyer,
                RaceTo = soloMatchDTO.RaceTo,
                EndTime = soloMatchDTO.EndTime
            };
        }
    }
}
    


