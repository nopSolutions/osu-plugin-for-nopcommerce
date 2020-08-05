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
        public IActionResult HandleSuccess([FromBody]WebHookSuccessPaymentRequest request)
        {
            if (ModelState.IsValid)
                _webHookProcessor.ProcessSuccessPayment(HttpContext, request);

            return Ok();
        }

        #endregion
    }
}
