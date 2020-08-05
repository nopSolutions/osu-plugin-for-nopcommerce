using System.Linq;
using Microsoft.AspNetCore.Http;
using Nop.Core;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Orders;
using Nop.Core.Http.Extensions;
using Nop.Plugin.Payments.Osu.Models;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Orders;
using Nop.Services.Payments;

namespace Nop.Plugin.Payments.Osu.Services
{
    /// <summary>
    /// Provides an default implementation for factory to create the Osu payment info model
    /// </summary>
    public class DefaultPaymentInfoFactory : IPaymentInfoFactory
    {
        #region Fields

        private readonly CurrencySettings _currencySettings;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ICurrencyService _currencyService;
        private readonly ICustomerService _customerService;
        private readonly IPaymentService _paymentService;
        private readonly IOsuPaymentService _osuPaymentService;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;
        private readonly OsuPaymentSettings _osuPaymentSettings;

        #endregion

        #region Ctor

        public DefaultPaymentInfoFactory(
            CurrencySettings currencySettings,
            IHttpContextAccessor httpContextAccessor,
            ICurrencyService currencyService,
            ICustomerService customerService,
            IPaymentService paymentService,
            IOsuPaymentService osuPaymentService,
            IOrderTotalCalculationService orderTotalCalculationService,
            IShoppingCartService shoppingCartService,
            IStoreContext storeContext,
            IWorkContext workContext,
            OsuPaymentSettings osuPaymentSettings)
        {
            _currencySettings = currencySettings;
            _httpContextAccessor = httpContextAccessor;
            _currencyService = currencyService;
            _customerService = customerService;
            _paymentService = paymentService;
            _osuPaymentService = osuPaymentService;
            _osuPaymentSettings = osuPaymentSettings;
            _orderTotalCalculationService = orderTotalCalculationService;
            _shoppingCartService = shoppingCartService;
            _storeContext = storeContext;
            _workContext = workContext;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Creates the Osu payment info model
        /// </summary>
        /// <returns>The Osu payment info model</returns>
        public virtual PaymentInfoModel CreatePaymentInfo()
        {
            var customer = _workContext.CurrentCustomer;
            if (customer == null)
                return null;

            if (string.IsNullOrWhiteSpace(_osuPaymentSettings.ApiKey))
                return null;

            var model = new PaymentInfoModel
            {
                ApiKey = _osuPaymentSettings.ApiKey,

                // default to use customer email if no billing/shipping email is specified
                BuyerEmail = customer.Email
            };

            // set billing details
            var billingAddress = _customerService.GetCustomerBillingAddress(customer);
            if (billingAddress == null)
                billingAddress = _customerService.GetCustomerShippingAddress(customer);

            if (billingAddress != null)
            {
                if (!string.IsNullOrWhiteSpace(billingAddress.Email))
                    model.BuyerEmail = billingAddress.Email;

                model.BuyerFirstName = billingAddress.FirstName;
                model.BuyerLastName = billingAddress.LastName;
                model.BuyerAddress = billingAddress.Address1;
                model.BuyerPostCode = billingAddress.ZipPostalCode;
            }

            // set currency
            if (_workContext.WorkingCurrency == null)
                return null;

            var currency = _currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId);
            if (currency == null)
                return null;

            model.PaymentCurrency = currency.CurrencyCode;

            // set payment amount
            var cart = _shoppingCartService.GetShoppingCart(customer, ShoppingCartType.ShoppingCart, _storeContext.CurrentStore.Id);
            if (cart?.Any() == true)
            {
                var subTotal = _orderTotalCalculationService.GetShoppingCartTotal(cart);

                model.PaymentAmount = subTotal.HasValue
                    ? subTotal.Value * 100
                    : decimal.Zero;
            }

            // set payment transaction reference
            var httpContext = _httpContextAccessor.HttpContext;

            httpContext.Session.Remove(Defaults.PaymentRequestSessionKey);

            var processPaymentRequest = new ProcessPaymentRequest();
            _paymentService.GenerateOrderGuid(processPaymentRequest);
            model.PaymentReference = $"{processPaymentRequest.OrderGuid.ToString()}::{customer.Id}";

            httpContext.Session.Set(Defaults.PaymentRequestSessionKey, processPaymentRequest);

            // clear the payment transaction id for customer
            _osuPaymentService.CaptureTransactionId(customer, null);

            return model;
        }

        #endregion
    }
}
