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
        protected bool IsSave { get; set; } = false;
        protected bool IsReCreate { get; set; } = false;

        private CancellationTokenSource _cts = new();
        protected List<int> hours = Enumerable.Range(0, 24).ToList();
        protected List<int> minutes = Enumerable.Range(0, 60).ToList();

        protected bool isSchJob = true;

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
            await SaveFormAsync(_cts.Token);
        }
        protected async Task SaveFormAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                if (!isSchJob)
                {
                    ApiForm.ScHour = null;
                    ApiForm.ScMin = null;

                }


                IsSave = false;
                isError = false;

                if (ApiForm == null)
                {
                    throw new ArgumentNullException(nameof(ApiForm));
                }

                if (Id.HasValue)
                {
                    ApiForm.Id = Id.Value;
                    await _apiService.UpdateAsync(ApiForm, cancellationToken);
                }
                else
                {
                    await _apiService.CreateAsync(ApiForm, cancellationToken);
                }

                IsSave = true;
                _navigationManager.NavigateTo("/Api-Craft");
            }
            catch (OperationCanceledException)
            {
                // Optional: handle cancellation separately
                isError = true;
                // You could show a message: "Operation was canceled."
            }
            catch (Exception ex)
            {
                isError = true;
                // Log the error or show feedback to the user
                Console.Error.WriteLine($"Error saving form: {ex.Message}");
            }
        }
        protected void SetStep(int step) => CurrentStep = step;
        protected void NextStep() => CurrentStep++;
        protected void PrevStep() => CurrentStep--;
        protected void RemoveHeader(ApiHeaderDto header)
        {
            ApiForm?.ApiHeaders?.Remove(header);
        }
        protected void AddHeader()
        {
            ApiForm?.ApiHeaders?.Add(new ApiHeaderDto
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
            IsReCreate = true;
            _navigationManager.NavigateTo(_navigationManager.Uri, forceLoad: true);
        }
    }
}
