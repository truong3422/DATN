using AutoMapper;
using BusinessObject.Models;
using DataAccess;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PoolComVnWebAPI.DTO;

namespace PoolComVnWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContactController : ControllerBase
    {
        private readonly ContactDAO _contactDAO;
        public ContactController(ContactDAO contactDAO)
        {
           _contactDAO = contactDAO;
        }
        [HttpPost("Add")]
        public ActionResult Post([FromBody] ContactDTO contactDTO)
        {
            try
            {


                var contact = new Contact
                {
                    FullName = contactDTO.Name,
                    Email = contactDTO.Email,
                    Description = contactDTO.Message
                };

                _contactDAO.AddContact(contact);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
