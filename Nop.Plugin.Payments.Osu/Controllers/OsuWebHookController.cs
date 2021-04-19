using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Payments.Osu.Models;
using Nop.Plugin.Payments.Osu.Services;

namespace Nop.Plugin.Payments.Osu.Controllers
{
    public class OsuWebHookController : Controller
    {
        #region Fields

        private readonly IWebHookPaymentProcessor _webHookProcessor; 

        #endregion

        #region Ctor

        public OsuWebHookController(IWebHookPaymentProcessor webHookProcessor)
        {
            _webHookProcessor = webHookProcessor;
        }

        #endregion

        #region Methods

        [HttpPost]
        public async Task<IActionResult> HandleSuccess([FromBody]WebHookSuccessPaymentRequest request)
        {
            if (ModelState.IsValid)
                await _webHookProcessor.ProcessSuccessPaymentAsync(HttpContext, request);

            return Ok();
        }

        #endregion
    }
}
