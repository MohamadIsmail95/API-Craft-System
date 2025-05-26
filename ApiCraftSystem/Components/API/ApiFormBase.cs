using ApiCraftSystem.Repositories.ApiServices;
using ApiCraftSystem.Repositories.ApiServices.Dtos;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;

namespace ApiCraftSystem.Components.API
{
    public class ApiFormBase : ComponentBase
    {
        [Inject] protected IApiService _apiService { get; set; }
        [Inject] protected NavigationManager _navigationManager { get; set; }
        [Inject] protected IJSRuntime JSRuntime { get; set; }
        [Parameter] public Guid? Id { get; set; }
        protected ApiStoreDto ApiForm = new ApiStoreDto();
        [Parameter] public EventCallback<ApiStoreDto> OnSubmit { get; set; }

        protected int CurrentStep = 1;

        protected EditContext _editContext;

        protected bool _isFormValid = false;

        protected bool isError = false;



        protected override async Task OnInitializedAsync()
        {
            SetContext();

            if (Id.HasValue)
            {
                var existing = await _apiService.GetByIdAsync(Id.Value);
                if (existing != null)
                    ApiForm = existing;
                _isFormValid = true;
                SetContext();
            }

        }
        protected async Task SaveForm()
        {
            try
            {
                if (Id.HasValue)
                {
                    ApiForm.Id = Id.Value;
                    await _apiService.UpdateAsync(ApiForm);
                }
                else
                {

                    await _apiService.CreateAsync(ApiForm);

                }

                _navigationManager.NavigateTo("/Api-Craft"); // Go back to list
            }
            catch (Exception ex)
            {
                isError = true;
            }


        }
        protected void SetStep(int step) => CurrentStep = step;
        protected void NextStep() => CurrentStep++;
        protected void PrevStep() => CurrentStep--;

        protected void RemoveHeader(ApiHeaderDto header)
        {
            ApiForm.ApiHeaders.Remove(header);
        }
        protected void AddHeader()
        {
            ApiForm.ApiHeaders.Add(new ApiHeaderDto
            {
                Id = Guid.NewGuid(),
                ApiStoreId = ApiForm.Id
            });
        }
        protected void RemoveMap(ApiMapDto map)
        {
            ApiForm.ApiMaps.Remove(map);
        }
        protected void AddMap()
        {
            ApiForm.ApiMaps.Add(new ApiMapDto
            {
                Id = Guid.NewGuid(),
                ApiStoreId = ApiForm.Id
            });
        }
        private void SetContext()
        {
            _editContext = new EditContext(ApiForm);
            _editContext.OnFieldChanged += (sender, args) =>
            {
                _isFormValid = _editContext.Validate(); // Live validation on field change
                StateHasChanged(); // Refresh UI
            };

        }
        protected async Task BeautifyJson()
        {
            await JSRuntime.InvokeVoidAsync("beautifyJson", "apiBody");
        }
        protected async Task RecreateTableAPI()
        {
            await _apiService.ReCreateDynamicTableAsync(ApiForm);
        }
    }
}
