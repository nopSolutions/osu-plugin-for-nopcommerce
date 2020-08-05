using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using Nop.Plugin.Payments.Osu.Models;
using Nop.Services.Customers;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;

namespace Nop.Plugin.Payments.Osu.Services
{
    /// <summary>
    /// Provides a default implementation to process the web hooks payments
    /// </summary>
    public class DefaultWebHookPaymentProcessor : IWebHookPaymentProcessor
    {
        #region Fields

        private readonly ICustomerService _customerService;
        private readonly IOrderService _orderService;
        private readonly IOsuPaymentService _osuPaymentService;
        private readonly ILogger _logger;
        private readonly OsuPaymentSettings _settings;

        #endregion

        #region Ctor

        public DefaultWebHookPaymentProcessor(
            ICustomerService customerService,
            IOrderService orderService,
            IOsuPaymentService osuPaymentService,
            ILogger logger,
            OsuPaymentSettings settings)
        {
            _customerService = customerService;
            _orderService = orderService;
            _osuPaymentService = osuPaymentService;
            _logger = logger;
            _settings = settings;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Processes the web hook payment request
        /// </summary>
        /// <param name="httpContext">The HTTP context</param>
        /// <param name="request">The web hook payment request</param>
        public virtual void ProcessSuccessPayment(HttpContext httpContext, WebHookSuccessPaymentRequest request)
        {
            if (httpContext == null)
                throw new ArgumentNullException(nameof(httpContext));
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (CanProcessSuccessPayment(httpContext, request))
            {
                var paymentData = request.PaymentData.Reference.Split("::");
                var orderGuid = Guid.Parse(paymentData[0]);
                var order = _orderService.GetOrderByGuid(orderGuid);
                if (order == null)
                {
                    // it's checkout process
                    // let's just save the payment transaction id for further processing
                    var customerId = int.Parse(paymentData[1]);
                    var customer = _customerService.GetCustomerById(customerId);
                    if (customer != null)
                        _osuPaymentService.CaptureTransactionId(customer, request.PaymentId);
                }
                else
                    _osuPaymentService.CaptureOrder(order, request.PaymentId);
            }
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Checks if the web hook request can be processed
        /// </summary>
        /// <param name="httpContext">The HTTP context</param>
        /// <param name="request">The web hook payment request</param>
        /// <returns>Returns the value indicating whether the request can be processed</returns>
        protected virtual bool CanProcessSuccessPayment(HttpContext httpContext, WebHookSuccessPaymentRequest request)
        {
            if (httpContext == null)
                throw new ArgumentNullException(nameof(httpContext));
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (string.IsNullOrWhiteSpace(request.PaymentId))
                return false;

            if (request.PaymentData == null)
                return false;

            if (string.IsNullOrWhiteSpace(request.PaymentData.Reference))
                return false;

            var paymentData = request.PaymentData.Reference.Split("::");
            if (paymentData.Length != 2)
                return false;

            // check order guid
            if (!Guid.TryParse(paymentData[0], out var _))
                return false;

            // check customer id
            if (!int.TryParse(paymentData[1], out var _))
                return false;

            if (request.PaymentData.PaymentStatus != "COLLECTED")
                return false;

            if (!httpContext.Request.Headers.TryGetValue(Defaults.WebHooks.SignatureHeaderName, out var signature))
                return false;

            if (string.IsNullOrWhiteSpace(_settings.PublicKey))
                return false;

            try
            {
                var encryptedMessageHash = Convert.FromBase64String(signature);

                using var sha256 = new SHA256Managed();
                var paymentIdBytes = Encoding.UTF8.GetBytes(request.PaymentId);
                var paymentIdHash = sha256.ComputeHash(paymentIdBytes);

                using var reader = new StringReader(_settings.PublicKey);
                var pemReader = new PemReader(reader);
                var rsaKeyParameters = (RsaKeyParameters)pemReader.ReadObject();
                var rsaParameters = DotNetUtilities.ToRSAParameters(rsaKeyParameters);

                using var rsa = new RSACryptoServiceProvider();
                rsa.ImportParameters(rsaParameters);

                return rsa.VerifyHash(paymentIdHash, encryptedMessageHash, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            }
            catch (Exception exception)
            {
                _logger.Error("Osu payment plugin: Invalid signature verification. Make sure that the correct 'public key' is specified in the plugin settings.", exception);
                return false;
            }
        }

        #endregion
    }
}
