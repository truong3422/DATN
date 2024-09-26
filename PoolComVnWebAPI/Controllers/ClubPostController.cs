using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using AutoMapper;
using BusinessObject.Models;
using DataAccess;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PoolComVnWebAPI.DTO;

namespace PoolComVnWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClubPostController : ControllerBase
    {
        private readonly ClubPostDAO _clubPostDAO;
        private readonly IMapper _mapper;

        public ClubPostController(ClubPostDAO clubPostDAO, IMapper mapper)
        {
            _clubPostDAO = clubPostDAO;
            _mapper = mapper;
        }

        [HttpGet]
        public ActionResult<IEnumerable<ClubPostDTO>> Get()
        {
            var clubPosts = _clubPostDAO.GetAllClubPosts();
            var clubPostsDto = _mapper.Map<List<ClubPostDTO>>(clubPosts);
            return Ok(clubPostsDto);
        }

        [HttpGet("{id}")]
        public ActionResult<ClubPostDTO> Get(int id)
        {
            var clubPost = _clubPostDAO.GetClubPostById(id);

            if (clubPost == null)
            {
                return NotFound();
            }

            var clubPost2 = new ClubPost
            {
                PostId = clubPost.PostId,
                ClubId = clubPost.ClubId,
                Title = clubPost.Title,
                Description = clubPost.Description,
                CreatedDate = clubPost.CreatedDate,
                UpdatedDate = clubPost.UpdatedDate,
                Flyer = clubPost.Flyer,
                Link = clubPost.Link,
                Status = clubPost.Status,
            };
            return Ok(clubPost2);
        }
        [HttpGet("ChangeStatus")]
        public ActionResult<ClubPostDTO> ChangeStatus(int id)
        {
            var clubPost = _clubPostDAO.GetClubPostById(id);

            if (clubPost == null)
            {
                return NotFound();
            }

            if (clubPost.Status == true) { clubPost.Status = false; }
            else
            {
                clubPost.Status = true;
            }
            _clubPostDAO.UpdateClubPost(clubPost);    
            return Ok();
        }

        [HttpPost("Add")]
        public IActionResult Post([FromBody] ClubPostDTO clubPostDTO)
        {
            try
            {
              
                var clubPost = new ClubPost
                {
                    ClubId = clubPostDTO.ClubID,
                    Title = clubPostDTO.Title,
                    Description = clubPostDTO.Description,
                    CreatedDate = clubPostDTO.CreatedDate,
                    UpdatedDate = clubPostDTO.UpdatedDate,
                    Flyer = clubPostDTO.Flyer,
                    Link = clubPostDTO.Link,
                    Status = true
                };


                _clubPostDAO.AddClubPost(clubPost);
                return Ok(clubPost);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpPost("Update/{id}")]
        public ActionResult Put([FromBody] ClubPostDTO clubPostDTO)
        {
            try
            {
               

                var existingNews = _clubPostDAO.GetClubPostById(clubPostDTO.PostID);

                if (existingNews == null)
                {
                    return NotFound();
                }

                existingNews.Title = clubPostDTO.Title;
                existingNews.Description = clubPostDTO.Description;
                existingNews.CreatedDate = clubPostDTO.CreatedDate;
                existingNews.UpdatedDate = clubPostDTO.UpdatedDate;
                existingNews.Link = clubPostDTO.Link;
                existingNews.Flyer = clubPostDTO.Flyer;
                existingNews.Status = clubPostDTO.Status;
                _clubPostDAO.UpdateClubPost(existingNews);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("GetByClubId/{clubId}")]
        public IActionResult GetByClubId(int clubId)
        {
            try
            {
                var clubPosts = _clubPostDAO.GetClubPostByClubId(clubId);

                if (clubPosts == null)
                {
                    return NotFound("Câu lạc bộ chưa có bài post nào");
                }
                return Ok(clubPosts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("SearchByTitleAndClubId")]
        public IActionResult SearchByTitleAndClubId(string title, int clubId)
        {
            try
            {
                var clubPosts = _clubPostDAO.SearchClubPostsByTitleAndClubId(title, clubId);

                if (clubPosts == null || clubPosts.Count == 0)
                {
                    return NotFound("Không tìm thấy bài đăng nào cho tiêu đề và câu lạc bộ đã cung cấp.");
                }

                List<ClubPostDTO> clubPostsDto = new List<ClubPostDTO>();

                foreach (var post in clubPosts)
                {
                    clubPostsDto.Add(new ClubPostDTO
                    {
                        PostID = post.PostId,
                        ClubID = post.ClubId,
                        Title = post.Title,
                        Description = post.Description,
                        CreatedDate = post.CreatedDate,
                        UpdatedDate = post.UpdatedDate,
                        Flyer = post.Flyer,
                        Link = post.Link,
                        Status = post.Status
                    });
                }

                return Ok(clubPostsDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi máy chủ nội bộ: {ex.Message}");
            }
        }




    }
}
