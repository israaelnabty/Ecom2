
using Ecom.BLL.ModelVM.Payment;

namespace Ecom.BLL.Service.Abstraction
{
    public interface IPaymentService
    {
        Task<ResponseResult<Payment>> CreatePaymentRecordAsync(CreatePaymentVM model, string userId);
        Task<ResponseResult<bool>> UpdatePaymentStatusAsync(PaymentResultVM model, string userModified);
        Task<ResponseResult<Payment>> GetPaymentByOrderIdAsync(int orderId);
    }
}
