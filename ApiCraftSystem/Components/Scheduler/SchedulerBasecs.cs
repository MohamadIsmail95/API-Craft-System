using ApiCraftSystem.Repositories.SchedulerService;
using Microsoft.AspNetCore.Components;
using System.ComponentModel;

namespace ApiCraftSystem.Components.Scheduler
{
    public class SchedulerBasecs : ComponentBase
    {
        [Inject] protected NavigationManager _navigationManager { get; set; }
        [Inject] protected ISchedulerService _schedulerService { get; set; }

        protected override async Task OnInitializedAsync()
        {

        }

    }
}
