using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.InkML;
using Microsoft.AspNetCore.Components;

namespace GiamSat.UI.Components
{
    public partial class DialogPageProfile
    {
        [Parameter] public int ProfileId { get; set; }

        //Order order;
        //IEnumerable<OrderDetail> orderDetails;

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();

            //order = dbContext.Orders.Where(o => o.OrderID == OrderID)
            //.Include("Customer")
            //                  .Include("Employee").FirstOrDefault();

            //orderDetails = dbContext.OrderDetails.Include("Order").ToList();

            var r = 100;
        }
    }
}
