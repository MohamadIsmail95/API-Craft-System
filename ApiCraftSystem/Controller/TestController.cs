using ApiCraftSystem.Test;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ApiCraftSystem.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {

        [HttpGet("T1")]

        public async Task<IActionResult> GetAction()
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
    }
}
