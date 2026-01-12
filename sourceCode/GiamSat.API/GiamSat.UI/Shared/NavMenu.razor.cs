using GiamSat.APIClient;
using GiamSat.Models;
using GiamSat.UI.Model;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using Radzen;
using System;

namespace GiamSat.UI.Shared
{
    public partial class NavMenu
    {
        [Parameter]
        public bool sidebarExpanded { get; set; } = true;

        private List<OvenSystemModel> _ovenSystems = new List<OvenSystemModel>();
        private int _currentExpandedOvenId = -1;

        protected override async Task OnInitializedAsync()
        {
            await LoadOvenSystems();
        }

        private async Task LoadOvenSystems()
        {
            try
            {
                // TODO: Uncomment when API is ready
                // var res = await _ovenSystemClient.GetAllAsync();
                // if (res.Succeeded)
                // {
                //     _ovenSystems = res.Data.ToList();
                //     if (_ovenSystems.Any())
                //     {
                //         _ovenSystems[0].IsExpanded = true;
                //         _currentExpandedOvenId = _ovenSystems[0].Id;
                //     }
                // }

                // FAKE DATA - Remove when API is ready
                _ovenSystems = new List<OvenSystemModel>
                {
                    new OvenSystemModel
                    {
                        Id = 1,
                        Name = "Hệ thống lò 1",
                        Description = "Lò sản xuất khu A",
                        IsActive = true,
                        IsExpanded = true // Mặc định mở lò đầu tiên
                    },
                    new OvenSystemModel
                    {
                        Id = 2,
                        Name = "Hệ thống lò 2",
                        Description = "Lò sản xuất khu B",
                        IsActive = true,
                        IsExpanded = false
                    },
                    new OvenSystemModel
                    {
                        Id = 3,
                        Name = "Hệ thống lò 3",
                        Description = "Lò sản xuất khu C",
                        IsActive = true,
                        IsExpanded = false
                    }
                };

                _currentExpandedOvenId = 1; // Lò đầu tiên
            }
            catch (Exception ex)
            {
                _notificationService.Notify(new NotificationMessage 
                { 
                    Severity = NotificationSeverity.Error, 
                    Summary = "Lỗi", 
                    Detail = $"Không thể tải danh sách hệ thống lò: {ex.Message}", 
                    Duration = 4000 
                });
            }
        }

        private void OnOvenClicked(OvenSystemModel oven, MenuItemEventArgs args)
        {
            // Collapse tất cả các lò khác
            foreach (var o in _ovenSystems)
            {
                o.IsExpanded = false;
            }

            // Expand lò được chọn và navigate về trang chủ
            oven.IsExpanded = true;
            _currentExpandedOvenId = oven.Id;

            // Luôn navigate về trang chủ của lò khi click vào parent menu
            _navigation.NavigateTo($"/oven/{oven.Id}", forceLoad: false);

            // Update breadcrumb - chỉ hiển thị tên lò và "Trang chủ"
            UpdateBreadcrumb(oven.Name, "Trang chủ");
            
            // Force re-render
            StateHasChanged();
        }

        private void OnOvenMenuItemClicked(OvenSystemModel oven, string menuName, MenuItemEventArgs args)
        {
            // Đảm bảo lò này đang được expand
            if (!oven.IsExpanded)
            {
                foreach (var o in _ovenSystems)
                {
                    o.IsExpanded = false;
                }
                oven.IsExpanded = true;
                _currentExpandedOvenId = oven.Id;
            }

            // Update breadcrumb
            UpdateBreadcrumb(oven.Name, menuName);
            
            // Force re-render để update active state
            StateHasChanged();
        }

        private void UpdateBreadcrumb(string ovenName, string? menuName)
        {
            GlobalVariable.BreadCrumbData = new List<BreadCrumbModel>();
            
            GlobalVariable.BreadCrumbData.Add(new BreadCrumbModel
            {
                Text = ovenName,
                Path = null
            });

            if (!string.IsNullOrEmpty(menuName))
            {
                GlobalVariable.BreadCrumbData.Add(new BreadCrumbModel
                {
                    Text = menuName,
                    Path = null
                });
            }
        }
    }
}
