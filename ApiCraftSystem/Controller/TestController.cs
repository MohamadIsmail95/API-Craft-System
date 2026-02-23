using ApiCraftSystem.Test;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ApiCraftSystem.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {

        [HttpGet("T1")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]

        public IActionResult GetAction()
        {
            ComplexObjDto complexObjDto = new ComplexObjDto();
            complexObjDto.Rows = [];
            complexObjDto.Cols = new List<ColsDto> {
                new ColsDto(1,"T1","Int","Test") ,
                new ColsDto(2,"T2","Int","Test") ,
                new ColsDto(3,"T3","Int","Test") ,
                new ColsDto(4,"T4","Int","Test") ,
                new ColsDto(6,"T5","Int","Test") ,
                new ColsDto(7,"T6","Int","Test") ,
                new ColsDto(8,"T7","Int","Test") ,
                new ColsDto(9,"T8","Int","Test") ,
                new ColsDto(10,"T9","Int","Test")

            };
            complexObjDto.Date = "Amro";
            return Ok(new { Data = complexObjDto });

        }

        [HttpGet("GetEmployees")]
        public IActionResult GetEmployees()
        {
            Employee employee = new Employee(1, "Mohammad", "Test", 30, DateTime.Now);

            return Ok(employee);

        }


        [HttpGet("T2")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]

        public IActionResult GetT2Action()
        {
            ComplexObjDto complexObjDto = new ComplexObjDto();
            complexObjDto.Rows = [];
            complexObjDto.Cols = new List<ColsDto> {
                new ColsDto(1,"T2","Int","Test") ,
                new ColsDto(2,"T2","Int","Test") ,
                new ColsDto(3,"T3","Int","Test") ,
                new ColsDto(4,"T4","Int","Test") ,
                new ColsDto(6,"T5","Int","Test") ,
                new ColsDto(7,"T6","Int","Test") ,
                new ColsDto(8,"T7","Int","Test") ,
                new ColsDto(9,"T8","Int","Test") ,
                new ColsDto(10,"T9","Int","Test")

            };
            complexObjDto.Date = "Amro";
            return Ok(new { Data = complexObjDto });

        }

        [HttpPost("GetBasicEmployees")]
        [Authorize(AuthenticationSchemes = "BasicAuthentication")]
        public IActionResult GetBasicEmployees(BasicFilter input)
        {
            List<Employee> employees = new List<Employee>();

            for (int i = 1; i < 50; i++)
            {
                Employee employee = new Employee(i, "Mohammad", "Test", i + 1, DateTime.Now.AddDays(-i));
                employees.Add(employee);
            }

            var result = employees.Where(x => x.HireDate >= input.DateFrom && x.HireDate <= input.DateTo).ToList();

            return Ok(result);

        }


        [HttpPost("SitePower")]
        public IActionResult GetCabinets()
        {
            var responseData = new CabinetResponseDto
            {
                Model = new List<CabinetModelDto>
            {
                new CabinetModelDto
                {
                    Name = "Dummar-West",
                    BATTERRYTYPE = null,
                    CABINETPOWERLIBRARY = "Huawei Solar Battery Cabinet",
                    Id = 1161,
                    NUmberOfPSU = 3,
                    RENEWABLECABINETTYPE = "Controller With Battary",
                    SpaceInstallation = 0,
                    TPVersion = null,
                    OtherBatteryType = "-",
                    BatCapacity = "-600A"
                }
            },
                Type = new List<CabinetTypeDto>
            {
                new CabinetTypeDto { Attribute = "Name", DataType = "string" },
                new CabinetTypeDto { Attribute = "BatterryTypeId", DataType = "List" },
                new CabinetTypeDto { Attribute = "CabinetPowerLibraryId", DataType = "List" },
                new CabinetTypeDto { Attribute = "Id", DataType = "int" },
                new CabinetTypeDto { Attribute = "NUmberOfPSU", DataType = "int" },
                new CabinetTypeDto { Attribute = "RenewableCabinetTypeId", DataType = "List" },
                new CabinetTypeDto { Attribute = "SpaceInstallation", DataType = "float" },
                new CabinetTypeDto { Attribute = "TPVersion", DataType = "string" },
                new CabinetTypeDto { Dynamic = "Other Battery Type", DataType = "string" },
                new CabinetTypeDto { Dynamic = "Bat Capacity", DataType = "string" }
            }
            };

            var result = ApiResponse<CabinetResponseDto>.Success(responseData, count: 2000);

            return Ok(result);
        }


    }
}
