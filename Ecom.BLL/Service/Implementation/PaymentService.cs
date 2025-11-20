
//using Ecom.BLL.ModelVM.Payment;
//using Ecom.DAL.Enum;

//namespace Ecom.BLL.Service.Implementation
//{
//    public class PaymentService : IPaymentService
//    {
//        private readonly IPaymentRepo _paymentRepo;
//        private readonly IMapper _mapper;

//        public PaymentService(IPaymentRepo paymentRepo, IMapper mapper)
//        {
//            _paymentRepo = paymentRepo;
//            _mapper = mapper;
//        }

//        public async Task<ResponseResult<Payment>> CreatePaymentRecordAsync(CreatePaymentVM paymentVM)
//        {
//            try
//            {
//                // 1. Validate Order exists and belongs to user (Security Check)
//                // Call order service to get order by orderId
//                //var order = await _context.Orders
//                //    .FirstOrDefaultAsync(o => o.Id == model.OrderId && o.AppUserId == userId);

//                //if (order == null)
//                //    return new ResponseResult<Payment>(null, "Order not found or does not belong to user.", false);

//                // 2. Check if payment already exists
//                var existingPayment = await _paymentRepo.GetByOrderIdAsync(paymentVM.OrderId);
//                if (existingPayment != null)
//                {
//                    return new ResponseResult<Payment>(existingPayment, "Payment already initiated.", true);
//                }

//                // 3. Create new Payment Entity
//                // We use the Order's TotalAmount. Never trust the frontend for amounts!
//                //paymentVM.CreatedBy = order.AppUserId;
//                //paymentVM.TotalAmount = paymentVM.TotalAmount; //order.TotalAmount;
//                var p = _mapper.Map<Payment>(paymentVM);

//                // 4. Save to DB
//                var success = await _paymentRepo.AddAsync(payment);
//                if (!success)
//                    return new ResponseResult<Payment>(null, "Failed to create payment record.", false);

//                return new ResponseResult<Payment>(payment, null, true);
//            }
//            catch (Exception ex)
//            {
//                return new ResponseResult<Payment>(null, ex.Message, false);
//            }
//        }

//        public async Task<ResponseResult<bool>> UpdatePaymentStatusAsync(PaymentResultVM model, string userModified)
//        {
//            try
//            {
//                // 1. Get the payment
//                var payment = await _paymentRepo.GetByIdAsync(model.PaymentId);
//                if (payment == null)
//                    return new ResponseResult<bool>(false, "Payment not found.", false);

//                // 2. Use the DDD Method to update status
//                // Note: We handle the logic of "Pending -> Completed" here
//                var p = new Payment();
//                bool updated = _paymentRepo.UpdateAsync();

//                if (!updated)
//                    return new ResponseResult<bool>(false, "Failed to update payment status (invalid data).", false);

//                // 3. If Payment is Completed, we should probably update the Order Status too!
//                if (model.Status == PaymentStatus.Completed)
//                {
//                    // Load the order
//                    var order = await _context.Orders.FindAsync(payment.OrderId);
//                    if (order != null)
//                    {
//                        // Update order status to "PaymentReceived" or "Processing"
//                        // Assuming you have an OrderStatus enum for this
//                        // order.Update(OrderStatus.Processing, userModified, ...);
//                    }
//                }

//                // 4. Save Changes
//                var result = await _paymentRepo.UpdateAsync(payment);
//                if (!result)
//                    return new ResponseResult<bool>(false, "Database update failed.", false);

//                return new ResponseResult<bool>(true, null, true);
//            }
//            catch (Exception ex)
//            {
//                return new ResponseResult<bool>(false, ex.Message, false);
//            }
//        }

//        public async Task<ResponseResult<Payment>> GetPaymentByOrderIdAsync(int orderId)
//        {
//            var payment = await _paymentRepo.GetByOrderIdAsync(orderId);
//            if (payment == null)
//                return new ResponseResult<Payment>(null, "Payment not found.", false);

//            return new ResponseResult<Payment>(payment, null, true);
//        }
//    }
//}
