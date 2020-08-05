using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;

namespace Nop.Plugin.Payments.Osu.Services
{
    /// <summary>
    /// Provides a abstraction to manage the Osu payment workflow
    /// </summary>
    public interface IOsuPaymentService
    {
        /// <summary>
        /// Captures the payment transaction id for user
        /// </summary>
        /// <param name="transactionId">The payment transaction id</param>
        void CaptureTransactionId(Customer customer, string transactionId);

        /// <summary>
        /// Gets the captured payment transaction id for user
        /// </summary>
        /// <returns>The payment transaction id</returns>
        string GetCapturedTransactionId(Customer customer);

        /// <summary>
        /// Captures the order with specified transaction id
        /// </summary>
        /// <param name="order">The order</param>
        /// <param name="transactionId">The payment transaction id</param>
        void CaptureOrder(Order order, string transactionId);
    }
}
