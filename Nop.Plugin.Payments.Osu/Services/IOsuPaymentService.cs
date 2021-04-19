using System.Threading.Tasks;
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
        /// <returns>
        /// A task that represents the asynchronous operation
        /// </returns>
        Task CaptureTransactionIdAsync(Customer customer, string transactionId);

        /// <summary>
        /// Gets the captured payment transaction id for user
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the payment transaction id
        /// </returns>
        Task<string> GetCapturedTransactionIdAsync(Customer customer);

        /// <summary>
        /// Captures the order with specified transaction id
        /// </summary>
        /// <param name="order">The order</param>
        /// <param name="transactionId">The payment transaction id</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// </returns>
        Task CaptureOrderAsync(Order order, string transactionId);
    }
}
