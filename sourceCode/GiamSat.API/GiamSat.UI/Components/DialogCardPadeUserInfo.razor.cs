using GiamSat.APIClient;
using Microsoft.AspNetCore.Components;

namespace GiamSat.UI.Components
{
    public partial class DialogCardPadeUserInfo
    {
        [Parameter] public Guid UserId { get; set; }

        UpdateModel _userMode = new UpdateModel();

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            var s = UserId;
        }

        async void Submit(UpdateModel arg)
        {

        }
    }
}
