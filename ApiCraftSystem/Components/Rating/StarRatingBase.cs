
using ApiCraftSystem.Repositories.RateService;
using ApiCraftSystem.Repositories.RateService.Dtos;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace ApiCraftSystem.Components.Rating
{
    public class StarRatingBase : ComponentBase
    {

        [Inject] private IRateService _rateService { get; set; }

        [Parameter]
        public int? Rating { get; set; } = null;
        protected int HoverRating = 0;

        protected override async Task OnInitializedAsync()
        {
            await GetUserRate();
        }
        protected async Task SetRating(int value)
        {
            Rating = value;
            await _rateService.CreateRateAsync(new RateDto(Guid.NewGuid(), null, value));
        }
        protected async Task GetUserRate()
        {
            var rate = await _rateService.GetUserRateAsync();

            if (rate != null)
            {
                Rating = rate.Grade;
            }

        }

    }
}
