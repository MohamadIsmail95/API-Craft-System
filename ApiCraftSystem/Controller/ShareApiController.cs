using ApiCraftSystem.Repositories.ApiShareService;
using ApiCraftSystem.Repositories.GenericService;
using ApiCraftSystem.Shared;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ApiCraftSystem.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShareApiController : ControllerBase
    {

        private readonly IDynamicDataService _dynamicDataService;

        private readonly IApiShareService _apiShareService;
        public ShareApiController(IDynamicDataService dynamicDataService, IApiShareService apiShareService)
        {
            _dynamicDataService = dynamicDataService;
            _apiShareService = apiShareService;
        }


        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("GetShareData")]
        public async Task<IActionResult> GetData([FromQuery] string? apiKey, [FromBody] ShareApiRequest? input)
        {
            // Validate required parameters
            if (string.IsNullOrWhiteSpace(apiKey))
                return BadRequest("ApiKey is required in the query string.");

            if (input is null)
                return BadRequest("Request body is required.");

            // ✅ Extract userId from JWT token claims
            var userIdClaim = User.FindFirst("userId");
            if (userIdClaim == null)
                return Unauthorized("Invalid token: userId claim is missing.");

            var userId = userIdClaim.Value;

            // ✅ Get the ShareApi using userId
            var shareApi = await _apiShareService.GetShareApi(apiKey, userId);
            if (shareApi is null)
                return BadRequest("Invalid or unauthorized API key/token combination.");

            // ✅ Validate required DB connection info
            if (string.IsNullOrWhiteSpace(shareApi.ConnectionString) ||
                string.IsNullOrWhiteSpace(shareApi.DatabaseType.ToString()) ||
                string.IsNullOrWhiteSpace(shareApi.TableName))
            {
                return StatusCode(500, "API configuration is missing required information.");
            }

            // ✅ Get paged data
            var result = await _dynamicDataService.GetPagedDataAsync(
                shareApi.ConnectionString,
                shareApi.DatabaseType,
                shareApi.TableName,
                input.OrderBy,
                input.Ascending,
                input.PageIndex,
                input.PageSize,
                input.DateFrom,
                input.DateTo,
                shareApi.DateFilterColumnName
            );

            return Ok(new { Data = result.Data, Count = result.TotalCount });
        }


    }
}
