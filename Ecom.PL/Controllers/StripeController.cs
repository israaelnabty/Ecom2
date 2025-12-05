using Ecom.BLL.ModelVM.Payment;
using Ecom.DAL.Entity;
using Ecom.DAL.Enum;
using Ecom.DAL.Repo.Abstraction;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class StripeController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly IPaymentService _paymentService;
    private readonly IPaymentRepo _paymentRepo;
    private readonly IStripeService _stripeService;

    public StripeController(
        IOrderService orderService,
        IPaymentService paymentService,
        IPaymentRepo paymentRepo,
        IStripeService stripeService)
    {
        _orderService = orderService;
        _paymentService = paymentService;
        _paymentRepo = paymentRepo;
        _stripeService = stripeService;
    }

    [HttpPost("create-session/{orderId}")]
    public async Task<IActionResult> CreateSession(int orderId)
    {
        // 1️⃣ Get Order
        var orderResult = await _orderService.GetByIdAsync(orderId);

        if (!orderResult.IsSuccess || orderResult.Result == null)
            return NotFound(new { message = "Order not found" });

        var order = orderResult.Result;

        // 2️⃣ Get existing payment if any
        var existingPaymentVm = await _paymentService.GetPaymentByOrderIdAsync(orderId);

        Payment payment;

        if (existingPaymentVm.IsSuccess && existingPaymentVm.Result != null)
        {
            // 🔹 Load actual Payment entity from DB
            payment = await _paymentRepo.GetByOrderIdAsync(orderId);
        }
        else
        {
            // 3️⃣ Create new Payment FIRST
            var createPayment = await _paymentService.CreatePaymentRecordAsync(
                new CreatePaymentVM
                {
                    OrderId = order.Id,
                    PaymentMethod = PaymentMethod.Card
                },
                order.AppUserId
            );

            if (!createPayment.IsSuccess || createPayment.Result == null)
                return BadRequest(new { message = "Failed to create payment record" });

            // 🔹 Now load actual Payment entity
            payment = await _paymentRepo.GetByOrderIdAsync(orderId);
        }

        if (payment == null)
            return BadRequest(new { message = "Failed to load payment entity" });

        // 4️⃣ Create Stripe session with metadata
        var sessionUrl = await _stripeService.CreateCheckoutSessionAsync(order, payment);

        if (sessionUrl == null)
            return BadRequest(new { message = "Failed to create Stripe session" });

        return Ok(new { url = sessionUrl });
    }
}
