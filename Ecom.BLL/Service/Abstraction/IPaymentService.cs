
using Ecom.DAL.Enum;

namespace Ecom.BLL.Service.Abstraction
{
    public interface IPaymentService
    {
        Task<ResponseResult<GetPaymentVM>> CreatePaymentRecordAsync(CreatePaymentVM model, string userId);
        Task<ResponseResult<bool>> UpdatePaymentStatusAsync(PaymentResultVM model, string UpdatedBy);
        Task<ResponseResult<Payment>> GetPaymentByOrderIdAsync(int orderId);
        Task<ResponseResult<IEnumerable<GetPaymentVM>>> GetAllPaymentsAsync();
        Task<ResponseResult<bool>> ToggleDeleteStatusAsync(int id, string userModified);

        Task<ResponseResult<string>> CreateStripeSessionAsync(int orderId, string userId);
        Task<ResponseResult<bool>> MarkPaymentPaidAsync(int orderId, string paymentIntentId);
        Task<ResponseResult<bool>> MarkPaymentFailedAsync(int orderId, string paymentIntentId, PaymentStatus status);

    }
}
