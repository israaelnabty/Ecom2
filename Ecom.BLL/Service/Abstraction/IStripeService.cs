using Ecom.BLL.ModelVM.Order;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecom.BLL.Service.Abstraction
{
    public interface IStripeService
    {
        Task<string> CreateCheckoutSessionAsync(GetOrderVM order, Payment payment);
    }
}
