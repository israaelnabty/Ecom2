using Ecom.BLL.Service.Implementation;
using Ecom.DAL.Enum;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;

namespace Ecom.PL.Controllers
{
    [ApiController]
    [Route("api/stripe/webhook")]
    public class StripeWebhookController : BaseApiController
    {
        private readonly IPaymentService _paymentService;
        private readonly IOrderService _orderService;
        private readonly ICartService _cartService;
        private readonly string _secret;

        public StripeWebhookController(IPaymentService paymentService, IConfiguration config, IOrderService orderService,ICartService cartService)
        {
            _paymentService = paymentService;
            _orderService = orderService;
            _cartService = cartService;
            _secret = config["Stripe:WebhookSecret"];
            Console.WriteLine("Entered webhook");
        }

        [HttpPost]
        public async Task<IActionResult> Handle()
        {
            var json = await new StreamReader(Request.Body).ReadToEndAsync();

            Event stripeEvent;

            try
            {
                stripeEvent = EventUtility.ConstructEvent(
                    json,
                    Request.Headers["Stripe-Signature"],
                    _secret
                );
            }
            catch
            {
                // MUST return 200 to avoid Stripe retry loop
                return Ok(new { message = "Invalid signature" });
            }

            // -----------------------------
            //  PAYMENT SUCCESS
            // -----------------------------
            if (stripeEvent.Type == "checkout.session.completed")
            {
                var session = stripeEvent.Data.Object as Session;

                Console.WriteLine("Stripe: session completed event");

                // Metadata approach (safe and recommended)
                if (!session.Metadata.TryGetValue("orderId", out string orderIdStr) ||
                    !int.TryParse(orderIdStr, out int orderId))
                {
                    Console.WriteLine("Missing orderId in metadata");
                    return Ok(); // Do not fail!
                }

                if (!session.Metadata.TryGetValue("paymentId", out string paymentIdStr) ||
                    !int.TryParse(paymentIdStr, out int paymentId))
                {
                    Console.WriteLine("Missing paymentId in metadata");
                    return Ok();
                }

                string paymentIntentId = session.PaymentIntentId;

                Console.WriteLine("Marking payment as PAID");

                // Update payment record
                var result = await _paymentService.MarkPaymentPaidAsync(orderId, paymentIntentId);

                if (!result.IsSuccess)
                {
                    Console.WriteLine($"Payment update failed: {result.ErrorMessage}");
                    return Ok();
                }

                // Update order status → Processing
                var orderResult = await _orderService.UpdateStatusAsync(orderId, OrderStatus.Processing, "StripeWebhook");

                Console.WriteLine("Order status updated -> Processing");
                //var cart = await _cartService.GetByUserIdAsync(CurrentUserId);
                //if(cart == null)
                //{
                //    Console.WriteLine("Couldn't Find Cart By UserID");
                //    return Ok();
                //}
                //var clearCartResult = await _cartService.ClearCartAsync(cart.Result.Id);
                //if(clearCartResult.Result == null)
                //{
                //    Console.WriteLine("Couldn't Clear Cart ");
                //    return Ok();
                //}
            }

            // -----------------------------
            //  PAYMENT EXPIRED / FAILED
            // -----------------------------
            else if (stripeEvent.Type == "checkout.session.expired")
            {
                var session = stripeEvent.Data.Object as Session;

                Console.WriteLine("Stripe: session expired");

                if (!session.Metadata.TryGetValue("orderId", out string orderIdStr) ||
                    !int.TryParse(orderIdStr, out int orderId))
                {
                    Console.WriteLine("Missing orderId in metadata");
                    return Ok();
                }

                string sessionId = session.Id;

                await _paymentService.MarkPaymentFailedAsync(
                    orderId,
                    sessionId,
                    PaymentStatus.Failed
                );

                Console.WriteLine("Payment marked as FAILED");
            }

            Console.WriteLine("Webhook processed");
            return Ok(new { message = "Webhook processed" });
        }
    }
}
