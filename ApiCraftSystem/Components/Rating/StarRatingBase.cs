
using Microsoft.AspNetCore.Components;

namespace ApiCraftSystem.Components.Rating
{
    public class StarRatingBase : ComponentBase
    {
        [Parameter]
        public int Rating { get; set; }

        protected int HoverRating = 0;

        protected void SetRating(int value)
        {
            Rating = value;
        }

    }
}
