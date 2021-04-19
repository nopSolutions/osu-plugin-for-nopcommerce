using System.Threading.Tasks;
using Nop.Plugin.Payments.Osu.Models;

namespace Nop.Plugin.Payments.Osu.Services
{
    /// <summary>
    /// Provides an abstraction for factory to create the Osu payment info model
    /// </summary>
    public interface IPaymentInfoFactory
    {
        /// <summary>
        /// Creates the Osu payment info model
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the Osu payment info model
        /// </returns>
        Task<PaymentInfoModel> CreatePaymentInfoAsync();
    }
}
