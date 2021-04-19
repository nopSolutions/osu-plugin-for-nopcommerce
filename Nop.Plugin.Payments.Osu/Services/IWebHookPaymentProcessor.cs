using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Nop.Plugin.Payments.Osu.Models;

namespace Nop.Plugin.Payments.Osu.Services
{
    /// <summary>
    /// Provides a abstraction to process the web hooks payments
    /// </summary>
    public interface IWebHookPaymentProcessor
    {
        /// <summary>
        /// Processes the web hook success payment request
        /// </summary>
        /// <param name="httpContext">The HTTP context</param>
        /// <param name="request">The web hook success payment request</param>
        /// <returns>The <see cref="Task"/></returns>
        Task ProcessSuccessPaymentAsync(HttpContext httpContext, WebHookSuccessPaymentRequest request);
    }
}
