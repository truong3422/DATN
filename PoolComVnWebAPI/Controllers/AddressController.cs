using BusinessObject.Models;
using DataAccess;
using Microsoft.AspNetCore.Mvc;
using PoolComVnWebAPI.DTO;

namespace PoolComVnWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AddressController : ControllerBase
    {
        private readonly AddressDAO _addressDAO;

        public AddressController(AddressDAO addressDAO)
        {
            _addressDAO = addressDAO;
        }

        [HttpGet("provinces")]
        public IActionResult GetAllProvinces()
        {
            try
            {
                var provinces = _addressDAO.GetAllProvincesAsync();
                return Ok(provinces);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("districts/{provinceCode}")]
        public IActionResult GetDistrictsByProvinceCode(string provinceCode)
        {
            try
            {
                var districts = _addressDAO.GetDistrictsByProvinceCodeAsync(provinceCode);
                return Ok(districts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("districts/byProvinceName/{provinceName}")]
        public IActionResult GetDistrictsByProvinceName(string provinceName)
        {
            try
            {
                var districts = _addressDAO.GetDistrictsByProvinceNameAsync(provinceName);
                return Ok(districts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("wards/byDistrictName/{districtName}")]
        public IActionResult GetWardsByDistrictName(string districtName)
        {
            try
            {
                var wards = _addressDAO.GetWardsByDistrictCodeAsync(districtName);
                return Ok(wards);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpGet("wards/{districtCode}")]
        public IActionResult GetWardsByDistrictCode(string districtCode)
        {
            try
            {
                var wards = _addressDAO.GetWardsByDistrictCodeAsync(districtCode);
                return Ok(wards);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpGet("GetdistrictsByDistrictCode/{districtCode}")]
        public IActionResult GetDistrictByDistrictCode(string districtCode)
        {
            try
            {
                var district = _addressDAO.GetDistrictByDistrictCode(districtCode);
                if (district == null)
                {
                    return NotFound();
                }
                var DistrictDTO = new DistrictDTO
                {
                    Code = district.Code,
                    Name = district.Name,
                    ProvinceCode = district.ProvinceCode
                };
                return Ok(DistrictDTO);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("getwardsBywardCode/{wardCode}")]
        public IActionResult GetWardByWardCode(string wardCode)
        {
            try
            {
                var ward = _addressDAO.GetWardsByWardCode(wardCode);
                if (ward == null)
                {
                    return NotFound();
                }
                var wardDTO = new WardDTO
                {
                    Code = ward.Code,
                    Name = ward.Name,
                    DistrictCode = ward.DistrictCode
                };
                return Ok(wardDTO);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpGet("getProvincesByProvinceCode/{provinceCode}")]
        public IActionResult GetProvinceByProvinceCode(string ProvinceCode)
        {
            try
            {
                var province = _addressDAO.GetProvinceByProvinceCode(ProvinceCode);
                if (province == null)
                {
                    return NotFound();
                }
                var provinceDTO = new ProvinceDTO
                {
                    Code = province.Code,
                    Name = province.Name,
                   
                };
                return Ok(provinceDTO);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
