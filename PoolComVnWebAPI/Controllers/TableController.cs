using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using BusinessObject.Models;
using DataAccess;
using PoolComVnWebAPI.DTO;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;

namespace PoolComVnWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TableController : ControllerBase
    {
        private readonly TableDAO _tableDAO;
        private readonly IMapper _mapper;
        private readonly ClubDAO _clubDAO;
        public TableController(TableDAO tableDAO, IMapper mapper, ClubDAO clubDAO)
        {
            _tableDAO = tableDAO;
            _clubDAO = clubDAO;
            _mapper = mapper;
        }

        // GET: api/Table
        [HttpGet]
        public IActionResult GetAllTables()
        {
            var tables = _tableDAO.GetAllTables();
            var tableDTOs = _mapper.Map<List<TableDTO>>(tables);
            return Ok(tableDTOs);
        }
        [HttpGet("IsTagNameExists/{clubId}/{tagName}")]
        public async Task<IActionResult> IsTagNameExists(int clubId, string tagName)
        {
            try
            {
                if (string.IsNullOrEmpty(tagName))
                {
                    return BadRequest("Tag name cannot be null or empty.");
                }
                bool isExists =  _tableDAO.IsTagNameExistsAsync(clubId, tagName);

                return Ok(isExists);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("AddListTable")]
        public IActionResult AddListTable(List<TableDTO> tables)
        {
            try
            {
                if (tables == null || !tables.Any())
                {
                    return BadRequest("No tables provided.");
                }

                var tablesToAdd = tables.Select(t => new Table
                {
                    TableName = t.TableName,
                    ClubId = t.ClubId,
                    TagName = t.TagName,
                    Status = t.Status,
                    Size = t.Size,
                    Image = t.Image,
                    IsScheduling = t.IsScheduling,
                    IsUseInTour = t.IsUseInTour,
                    Price = t.Price
                }).ToList();

                _tableDAO.AddListTable(tablesToAdd);

                return Ok("Tables added successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
        // GET: api/Table/5
        [HttpGet("{id}")]
        public IActionResult GetTableById(int id)
        {
            var table = _tableDAO.GetTableById(id);

            if (table == null)
            {
                return BadRequest();
            }

            var tableDTO = _mapper.Map<TableDTO>(table);
            return Ok(tableDTO);
        }

        // GET: api/Table/GetAllTablesForClub
        [HttpGet("GetAllTablesForClub")]
        [Authorize]
        public IActionResult GetAllTablesForClub()
        {
            // Lấy giá trị token từ header
            var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

            // Giải mã token để lấy các claims
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(token) as JwtSecurityToken;

            // Xử lý logic của bạn với các claims
            var roleClaim = jsonToken?.Claims.FirstOrDefault(claim => claim.Type.Equals("Role"));
            var account = jsonToken?.Claims.FirstOrDefault(claim => claim.Type.Equals("Account"));
            if (!Constant.BusinessRole.ToString().Equals(roleClaim?.Value))
            {
                return BadRequest("Unauthorized");
            }

            if (!Int32.TryParse(account?.Value, out int accountId))
            {
                return BadRequest("Invalid AccountId claim");
            }
            var club = _clubDAO.GetClubByAccountId(accountId);

            int clubId = club.ClubId;

            var tables = _tableDAO.GetAllTablesForClub(clubId);

            // Map to TableDTO and return
            var tableDTOs = _mapper.Map<List<TableDTO>>(tables);
            return Ok(tableDTOs);
        }

        // GET: api/Table/GetAllTablesForTournament
        [HttpGet("GetAllTablesForTournament")]
        [Authorize]
        public IActionResult GetAllTablesForTournament()                    
        {
            // Lấy giá trị token từ header
            var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

            // Giải mã token để lấy các claims
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(token) as JwtSecurityToken;

            // Xử lý logic của bạn với các claims
            var roleClaim = jsonToken?.Claims.FirstOrDefault(claim => claim.Type.Equals("Role"));
            var account = jsonToken?.Claims.FirstOrDefault(claim => claim.Type.Equals("Account"));
            if (!Constant.BusinessRole.ToString().Equals(roleClaim?.Value))
            {
                return BadRequest("Unauthorized");
            }

            if (!Int32.TryParse(account?.Value, out int accountId))
            {
                return BadRequest("Invalid AccountId claim");
            }
            var club = _clubDAO.GetClubByAccountId(accountId);

            int clubId = club.ClubId;

            var tables = _tableDAO.GetAllTablesForClub(clubId);
            tables = tables.Where(t => t.IsUseInTour.Value == false).ToList();
            // Map to TableDTO and return
            var tableDTOs = _mapper.Map<List<TableDTO>>(tables);
            return Ok(tableDTOs);
        }

        // GET: api/Table/AddTableToTournament
        [HttpPost("AddTableToTournament")]
        public IActionResult AddTableToTournament(List<int> lstTableId)
        {
            _tableDAO.AddTableToTournament(lstTableId);

            return Ok();
        }

        [HttpPost("AddNewTable")]
        public IActionResult AddTable([FromBody] TableDTO tableDTO)
        {
            if (tableDTO == null)
            {
                return BadRequest("TableDTO cannot be null");
            }

            var table = _mapper.Map<Table>(tableDTO);
            _tableDAO.AddTable(table);

            return CreatedAtAction("GetTableById", new { id = table.TableId }, tableDTO);
        }

        [HttpPost("UpdateTableToTournament")]
        public IActionResult UpdateTableToTournament([FromBody] List<TableDTO> tableDtos)
        {
            try
            {
                if (tableDtos == null || tableDtos.Count == 0)
                {
                    return BadRequest("No table provided.");
                }

                // Update existing tables to set IsUseInTour to true
                var tableIds = tableDtos.Select(t => t.TableId).ToList();
                _tableDAO.UpdateIsUseInTourStatus(tableIds, true);

                return Ok("Tables updated in the tournament successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        [HttpGet("GetTablesByClubId/{clubId}")]
        public IActionResult GetTablesByClubId(int clubId)
        {
            try
            {
                var tables = _tableDAO.GetAllTablesForClub(clubId);

                if (tables == null)
                {
                    return NotFound("No tables found for the provided ClubId.");
                }

                return Ok(tables.Select(table => new TableDTO
                {
                    TableId = table.TableId,
                    TableName = table.TableName,
                    ClubId = table.ClubId,
                    TagName = table.TagName,
                    Status = table.Status,
                    Size = table.Size,
                    Image = table.Image,
                    IsScheduling = table.IsScheduling,
                    IsUseInTour = table.IsUseInTour,
                    Price = table.Price
                }).ToList());
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }


        [HttpPost("Update/{id}")]
        public IActionResult UpdateTable([FromBody] TableDTO updatedTableDTO)
        {
            var existingTable = _tableDAO.GetTableById(updatedTableDTO.TableId);

            if (existingTable == null)
            {
                return BadRequest();
            }

            _mapper.Map(updatedTableDTO, existingTable);
            _tableDAO.UpdateTable(existingTable);

            return Ok();
        }

        [HttpGet("Delete/{id}")]
        public IActionResult DeleteTable(int id)
        {
            var existingTable = _tableDAO.GetTableById(id);

            if (existingTable == null)
            {
                return BadRequest();
            }

            _tableDAO.DeleteTable(id);

            return Ok();
        }

        [HttpGet("GetTableNotInSchedule")]
        public IActionResult GetTableNotInSchedule(int clubId)
        {
            List<Table> lstTable = _tableDAO.TableNotInSchedule(clubId);
            List<TableDTO> tableDTOs = new List<TableDTO>();
            foreach (var table in lstTable)
            {
                TableDTO tableDTO = new TableDTO()
                {
                    TableId = table.TableId,
                    TableName = table.TableName, 
                    TagName = table.TagName
                };
                tableDTOs.Add(tableDTO);
            }

            return Ok(tableDTOs);
        }

        [HttpGet("GetTableNotInUse")]
        public IActionResult GetTableNotInUse(int clubId)
        {
            List<Table> lstTable = _tableDAO.TableNotInUse(clubId);
            List<TableDTO> tableDTOs = new List<TableDTO>();
            foreach (var table in lstTable)
            {
                TableDTO tableDTO = new TableDTO()
                {
                    TableId = table.TableId,
                    TableName = table.TableName,
                    TagName = table.TagName
                };
                tableDTOs.Add(tableDTO);
            }

            return Ok(tableDTOs);
        }
    }
}
