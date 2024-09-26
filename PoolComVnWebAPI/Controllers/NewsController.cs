using BusinessObject.Models;
using DataAccess;
using Firebase.Auth;
using Firebase.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PoolComVnWebAPI.DTO;
using System.IO;
using System.Net.Sockets;

namespace PoolComVnWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NewsController : ControllerBase
    {
        private readonly NewsDAO _newsDAO;


        public NewsController(NewsDAO newsDAO)
        {
            _newsDAO = newsDAO;
        }

        [HttpGet]
        public ActionResult<IEnumerable<NewsDTO>> Get()
        {
            try
            {

                var allNews = _newsDAO.GetAllNews();

                var result = allNews.Select(news => new NewsDTO
                {
                    NewsId = news.NewsId,
                    Title = news.Title,
                    Description = news.Description,
                    AccId = news.AccId,
                    CreatedDate = news.CreatedDate,
                    UpdatedDate = news.UpdatedDate,
                    Flyer = news.Flyer,
                    Link = news.Link,
                    AccountName = news.Acc?.Email,
                    Status = news.Status
                }).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {

                return BadRequest("Error while retrieving all news: " + ex.Message);
            }


        }

        [HttpGet("GetLatestNews")]
        public ActionResult<IEnumerable<NewsDTO>> GetLatestNews()
        {
            try
            {
                var latestNews = _newsDAO.GetLatestNews()
                                         .OrderByDescending(news => news.CreatedDate) // Hoặc news.UpdatedDate nếu muốn sắp xếp theo ngày cập nhật
                                         .ToList();

                var result = latestNews.Select(news => new NewsDTO
                { 
                    NewsId = news.NewsId,
                    Title = news.Title,
                    Description = news.Description,
                    AccId = news.AccId,
                    CreatedDate = news.CreatedDate,
                    UpdatedDate = news.UpdatedDate,
                    Flyer = news.Flyer,
                    Link = news.Link,
                    AccountName = news.Acc?.Email,
                    Status = news.Status
                }).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest("Error while retrieving latest news: " + ex.Message);
            }
        }


        [HttpGet("Search")]
        public ActionResult<IEnumerable<NewsDTO>> Search(string searchQuery)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchQuery))
                {
                    return BadRequest("Search query is empty.");
                }

                // Thực hiện tìm kiếm tin tức dựa trên tiêu đề chứa từ khóa tìm kiếm
                var filteredNews = _newsDAO.GetNewsByTitle(searchQuery);

                // Chuyển đổi kết quả thành đối tượng NewsDTO và trả về
                var result = filteredNews.Select(news => new NewsDTO
                {
                    NewsId = news.NewsId,
                    Title = news.Title,
                    Description = news.Description,
                    AccId = news.AccId,
                    CreatedDate = news.CreatedDate,
                    UpdatedDate = news.UpdatedDate,
                    Flyer = news.Flyer,
                    Link = news.Link,
                    AccountName = news.Acc?.Email,
                    Status = news.Status
                }).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest("Error while searching news: " + ex.Message);
            }
        }


        [HttpGet("{id}")]
        public ActionResult<NewsDTO> Get(int id)
        {
            try
            {
                var news = _newsDAO.GetNewsById(id);

                if (news == null)
                {
                    return NotFound();
                }
                var result = new NewsDTO
                {
                    NewsId = news.NewsId,
                    Title = news.Title,
                    Description = news.Description,
                    AccId = news.AccId,
                    CreatedDate = news.CreatedDate,
                    UpdatedDate = news.UpdatedDate,
                    Link = news.Link,
                    Flyer = news.Flyer,
                    AccountName = news.Acc?.Email,
                    Status = news.Status
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpPost("Add")]
        public  ActionResult Post([FromBody] NewsDTO newsDTO)
        {
            try
            {
                var account = _newsDAO.GetAccount(newsDTO.AccId);

                if (account == null)
                {

                    return BadRequest("Invalid AccID. No matching Account found.");
                }

                var news = new News
                {
                    Title = newsDTO.Title,
                    Description = newsDTO.Description,
                    AccId = newsDTO.AccId,
                    CreatedDate = newsDTO.CreatedDate,
                    UpdatedDate = newsDTO.UpdatedDate,
                    Link = newsDTO.Link,
                    Acc = account,
                    Status = newsDTO.Status
                };

                news.Flyer = newsDTO.Flyer;
                _newsDAO.AddNews(news);
                return CreatedAtAction(nameof(Get), new { id = news.NewsId }, newsDTO);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpPost("Update")]
        public ActionResult Put([FromBody] NewsDTO updatedNewsDTO)
        {
            try
            {
                var account = _newsDAO.GetAccount(updatedNewsDTO.AccId);

                if (account == null)
                {

                    return BadRequest("Invalid AccID. No matching Account found.");
                }

                var existingNews = _newsDAO.GetNewsById(updatedNewsDTO.NewsId);

                if (existingNews == null)
                {
                    return NotFound();
                }

                existingNews.Title = updatedNewsDTO.Title;
                existingNews.Description = updatedNewsDTO.Description;
                existingNews.AccId = updatedNewsDTO.AccId;
                existingNews.CreatedDate = updatedNewsDTO.CreatedDate;
                existingNews.UpdatedDate = updatedNewsDTO.UpdatedDate;
                existingNews.Link = updatedNewsDTO.Link;
                existingNews.Flyer = updatedNewsDTO.Flyer;
                existingNews.Acc = account; 
                existingNews.Status = updatedNewsDTO.Status;
                _newsDAO.UpdateNews(existingNews);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpPost("ChangeStatus")]
        public ActionResult ChangeStatus(int id)
        {
            try
            {
                _newsDAO.ChangeNewsStatus(id); 
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
