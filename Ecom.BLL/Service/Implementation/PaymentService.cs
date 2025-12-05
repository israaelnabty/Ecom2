using Ecom.BLL.ModelVM.Product;
using Ecom.BLL.Service.Abstraction;
using Ecom.DAL.Entity;
using Ecom.DAL.Enum;
using Stripe;
using Stripe.Checkout;
using PaymentMethod = Ecom.DAL.Enum.PaymentMethod;

namespace Ecom.BLL.Service.Implementation
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepo _paymentRepo;
        private readonly IMapper _mapper;
        private readonly IOrderService _orderService;
        private readonly IProductService _productService;
        private readonly IStripeService _stripeService;

        public PaymentService(IPaymentRepo paymentRepo, IMapper mapper, IOrderService orderService, IProductService productService,IStripeService stripeService)
        {
            _paymentRepo = paymentRepo;
            _mapper = mapper;
            _orderService = orderService;
            _productService = productService;
            _stripeService = stripeService;
        }

        public async Task<ResponseResult<GetPaymentVM>> CreatePaymentRecordAsync(CreatePaymentVM paymentVM, string userId)
        {
            try
            {
                // 1. Validate Order exists and belongs to user (user Id fom JWT)
                var result = await _orderService.GetByIdAsync(paymentVM.OrderId);
                if (!result.IsSuccess || result.Result == null)
                {
                    return new ResponseResult<GetPaymentVM>(null, "Order not found", false);
                }
                var order = result.Result;
                if (order.AppUserId != userId)
                {
                    return new ResponseResult<GetPaymentVM>(null, "Order doesn't belong to this user", false);
                }

                // 2. Check if payment already exists
                var existingPayment = await _paymentRepo.GetByOrderIdAsync(paymentVM.OrderId);
                
                if (existingPayment != null)
                {
                    //return new ResponseResult<GetPaymentVM>(existingPayment, "Payment already initiated.", true);
                }

                // 3. Create new Payment Entity
                // We use the Order's TotalAmount. Never trust the frontend for amounts!
                var payment = new Payment(
                    orderId: order.Id,
                    totalamount: order.TotalAmount, // Security: Use DB amount, not Frontend amount
                    paymentMethod: paymentVM.PaymentMethod,
                    transactionId: null,
                    createdBy: userId
                );

                // 4. Save to DB
                var success = await _paymentRepo.AddAsync(payment);
                var map = _mapper.Map<GetPaymentVM>(payment);
                if (!success)
                {
                    return new ResponseResult<GetPaymentVM>(null, "Failed to create payment record.", false);
                }

                return new ResponseResult<GetPaymentVM>(map, null, true);
            }
            catch (Exception ex)
            {
                return new ResponseResult<GetPaymentVM>(null, ex.Message, false);
            }
        }

        public async Task<ResponseResult<bool>> UpdatePaymentStatusAsync(PaymentResultVM model, string updatedBy)
        {
            try
            {
                // 1. Get the payment
                var payment = await _paymentRepo.GetByIdAsync(model.PaymentId);
                if (payment == null)
                {
                    return new ResponseResult<bool>(false, "Payment not found.", false);
                }

                //2. Get the associated order
                var orderResponse = await _orderService.GetByIdAsync(payment.OrderId);
                var order = orderResponse.Result;
                if (order == null)
                {
                    return new ResponseResult<bool>(false, "Associated order not found.", false);
                }

                // 3. Update payment status and transaction Id
                var isUpdateSuccessful = payment.Update(
                    transactionId: model.TransactionId,
                    userModified: updatedBy,
                    paymentStatus: model.Status
                );
                if (!isUpdateSuccessful)
                {
                    return new ResponseResult<bool>(false, "No changes detected to update payment.", false);
                }

                // 4. Update the Order Status, and Product QuantitySold based on Payment Status
                // If Payment is Completed
                if (model.Status == PaymentStatus.Completed)
                {
                    var updateOrderResponse = await _orderService.UpdateStatusAsync(payment.OrderId, OrderStatus.Processing, updatedBy);
                    if (!updateOrderResponse.IsSuccess)
                    {
                        return updateOrderResponse;
                    }
                    // Increase QuantitySold for each product in the order
                    foreach (var item in order.Items)
                    {
                        var addQuantityVM = new AddQuantitySoldVM
                        {
                            ProductId = item.ProductId,
                            QuantitySold = item.Quantity
                        };
                        var productUpdateResult = await _productService.AddToQuantitySoldAsync(addQuantityVM);
                        if (!productUpdateResult.IsSuccess)
                        {
                            return new ResponseResult<bool>(false, $"Failed to update quantity sold for product ID {item.ProductId}.", false);
                        }
                    }
                }
                // If Payment failed
                else if (model.Status == PaymentStatus.Failed)
                {
                    var updateOrderResponse = await _orderService.UpdateStatusAsync(payment.OrderId, OrderStatus.Pending, updatedBy);
                    if (!updateOrderResponse.IsSuccess)
                    {
                        return updateOrderResponse;
                    }
                    // Increase QuantitySold for each product in the order
                    //foreach (var item in order.Items)
                    //{
                    //    var productUpdateResult = await _productService.IncreaseStockAsync(item.ProductId, item.Quantity);
                    //    if (!productUpdateResult.IsSuccess)
                    //    {
                    //        return new ResponseResult<bool>(false, $"Failed to update quantity sold for product ID {item.ProductId}.", false);
                    //    }
                    //}
                }

                var updateResult = await _paymentRepo.UpdateAsync(payment);
                if (!updateResult)
                {
                    return new ResponseResult<bool>(false, "Failed to update payment status (invalid data).", false);
                }

                return new ResponseResult<bool>(true, null, true);
            }
            catch (Exception ex)
            {
                return new ResponseResult<bool>(false, ex.Message, false);
            }
        }

        public async Task<ResponseResult<Payment>> GetPaymentByOrderIdAsync(int orderId)
        {
            if (orderId <= 0)
            {
                return new ResponseResult<Payment>(null, "Invalid Order ID.", false);
            }

            var payment = await _paymentRepo.GetByOrderIdAsync(orderId);
            if (payment == null)
            {
                return new ResponseResult<Payment>(null, "Payment not found.", false);
            }

            return new ResponseResult<Payment>(payment, null, true);
        }

        public async Task<ResponseResult<IEnumerable<GetPaymentVM>>> GetAllPaymentsAsync()
        {
            try
            {
                var payments = await _paymentRepo.GetAllAsync(p => !p.IsDeleted, includes: e => e.Order);
                var mappedPayments = _mapper.Map<IEnumerable<GetPaymentVM>>(payments);

                return new ResponseResult<IEnumerable<GetPaymentVM>>(mappedPayments, null, true);
            }
            catch (Exception ex)
            {
                return new ResponseResult<IEnumerable<GetPaymentVM>>(null, ex.Message, false);
            }
        }

        public async Task<ResponseResult<bool>> ToggleDeleteStatusAsync(int id, string userModified)
        {
            var result = await _paymentRepo.ToggleDeleteStatusAsync(id, userModified);
            return result
                ? new ResponseResult<bool>(true, null, true)
                : new ResponseResult<bool>(false, "Failed to toggle delete status.", false);
        }

        public async Task<ResponseResult<string>> CreateStripeSessionAsync(int orderId, string userId)
        {
            // 1️⃣ Load Order
            var orderResponse = await _orderService.GetByIdAsync(orderId);

            if (!orderResponse.IsSuccess || orderResponse.Result == null)
                return new ResponseResult<string>(null, "Order not found", false);

            var order = orderResponse.Result;

            // 2️⃣ Ensure Order belongs to the user
            if (order.AppUserId != userId)
                return new ResponseResult<string>(null, "Order does not belong to this user", false);

            // 3️⃣ Retrieve existing payment or create a fresh one
            var existingPayment = await _paymentRepo.GetByOrderIdAsync(orderId);

            Payment payment;

            if (existingPayment != null)
            {
                payment = existingPayment;
            }
            else
            {
                // Create new payment record BEFORE Stripe session
                payment = new Payment(
                    orderId: order.Id,
                    totalamount: order.TotalAmount,
                    paymentMethod: PaymentMethod.Card,
                    transactionId: null,
                    createdBy: userId
                );

                var saved = await _paymentRepo.AddAsync(payment);
                if (!saved)
                    return new ResponseResult<string>(null, "Failed to create payment record", false);
            }

            // 4️⃣ Create Stripe Checkout Session WITH METADATA
            var sessionUrl = await _stripeService.CreateCheckoutSessionAsync(order, payment);

            if (sessionUrl == null)
                return new ResponseResult<string>(null, "Failed to create Stripe session", false);

            return new ResponseResult<string>(sessionUrl, null, true);
        }


        public async Task<ResponseResult<bool>> MarkPaymentPaidAsync(int orderId, string paymentIntentId)
        {
            try
            {
                // STEP 1 — Fetch Payment with Order included
                var payment = await _paymentRepo.GetByOrderIdAsync(orderId);

                if (payment == null)
                    return new ResponseResult<bool>(false, "Payment record not found for this order", false);

                // STEP 2 — Use Entity Method (Private Setters!)
                bool updated = payment.Update(
                    transactionId: paymentIntentId,
                    userModified: "StripeWebhook",
                    paymentStatus: PaymentStatus.Completed
                );

                if (!updated)
                    return new ResponseResult<bool>(false, "Failed to update payment entity", false);

                // STEP 3 — Save Changes
                bool saved = await _paymentRepo.UpdateAsync(payment);

                if (!saved)
                    return new ResponseResult<bool>(false, "Failed to save payment changes", false);

                return new ResponseResult<bool>(true, null, true);
            }
            catch (Exception ex)
            {
                return new ResponseResult<bool>(false, ex.Message, false);
            }
        }

        public async Task<ResponseResult<bool>> MarkPaymentFailedAsync(int orderId, string paymentIntentId, PaymentStatus status)
        {
            try
            {
                // 1️⃣ Fetch payment using the Order ID
                var payment = await _paymentRepo.GetByOrderIdAsync(orderId);

                if (payment == null)
                    return new ResponseResult<bool>(false, $"Payment not found for order: {orderId}", false);

                // 2️⃣ Fetch the associated order
                var orderResponse = await _orderService.GetByIdAsync(orderId);
                var order = orderResponse.Result;

                if (order == null)
                    return new ResponseResult<bool>(false, "Associated order not found.", false);

                // 3️⃣ Update Payment entity (using your domain Update method)
                bool updated = payment.Update(
                    transactionId: paymentIntentId,
                    userModified: "StripeWebhook",
                    paymentStatus: status
                );

                if (!updated)
                    return new ResponseResult<bool>(false, "No changes detected when updating payment.", false);

                // 4️⃣ Restore the Order status back to Pending
                var updateOrderResponse = await _orderService.UpdateStatusAsync(order.Id, OrderStatus.Pending, "StripeWebhook");

                if (!updateOrderResponse.IsSuccess)
                    return new ResponseResult<bool>(false, updateOrderResponse.ErrorMessage, false);

                // 5️⃣ Restore product stock (your logic)
                foreach (var item in order.Items)
                {
                    var restoreResult = await _productService.IncreaseStockAsync(item.ProductId, item.Quantity);
                    if (!restoreResult.IsSuccess)
                    {
                        return new ResponseResult<bool>(false,
                            $"Failed to restore stock for ProductID {item.ProductId}", false);
                    }
                }

                // 6️⃣ Save payment update to DB
                bool saved = await _paymentRepo.UpdateAsync(payment);
                if (!saved)
                    return new ResponseResult<bool>(false, "Failed to save failed payment update.", false);

                return new ResponseResult<bool>(true, null, true);
            }
            catch (Exception ex)
            {
                return new ResponseResult<bool>(false, ex.Message, false);
            }
        }



    }
}
