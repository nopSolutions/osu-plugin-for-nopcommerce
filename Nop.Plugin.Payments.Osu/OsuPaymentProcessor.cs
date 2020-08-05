using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Nop.Core.Domain.Cms;
using Nop.Core.Domain.Orders;
using Nop.Core.Http.Extensions;
using Nop.Plugin.Payments.Osu.Services;
using Nop.Services.Cms;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Payments;
using Nop.Services.Plugins;
using Nop.Web.Framework.Infrastructure;

namespace Nop.Plugin.Payments.Osu
{
    /// <summary>
    /// Represents a Osu payment processor
    /// </summary>
    public class OsuPaymentProcessor : BasePlugin, IPaymentMethod, IWidgetPlugin
    {
        #region Fields

        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly ICustomerService _customerService;
        private readonly IOsuPaymentService _osuPaymentService;
        private readonly ILocalizationService _localizationService;
        private readonly IPaymentService _paymentService;
        private readonly ISettingService _settingService;
        private readonly IUrlHelperFactory _urlHelperFactory;
        private readonly OsuPaymentSettings _osuPaymentSettings;
        private readonly WidgetSettings _widgetSettings;

        #endregion

        #region Ctor

        public OsuPaymentProcessor(
            IActionContextAccessor actionContextAccessor,
            ICustomerService customerService,
            IOsuPaymentService osuPaymentService,
            ILocalizationService localizationService,
            IPaymentService paymentService,
            ISettingService settingService,
            IUrlHelperFactory urlHelperFactory,
            OsuPaymentSettings osuPaymentSettings,
            WidgetSettings widgetSettings)
        {
            _actionContextAccessor = actionContextAccessor;
            _customerService = customerService;
            _osuPaymentService = osuPaymentService;
            _localizationService = localizationService;
            _paymentService = paymentService;
            _settingService = settingService;
            _urlHelperFactory = urlHelperFactory;
            _osuPaymentSettings = osuPaymentSettings;
            _widgetSettings = widgetSettings;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Process a payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>Process payment result</returns>
        public ProcessPaymentResult ProcessPayment(ProcessPaymentRequest processPaymentRequest)
        {
            return new ProcessPaymentResult();
        }

        /// <summary>
        /// Post process payment (used by payment gateways that require redirecting to a third-party URL)
        /// </summary>
        /// <param name="postProcessPaymentRequest">Payment info required for an order processing</param>
        public void PostProcessPayment(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            var customer = _customerService.GetCustomerById(postProcessPaymentRequest.Order.CustomerId);
            if (customer != null)
            {
                var transactionId = _osuPaymentService.GetCapturedTransactionId(customer);
                if (!string.IsNullOrWhiteSpace(transactionId))
                    _osuPaymentService.CaptureOrder(postProcessPaymentRequest.Order, transactionId);
            }
        }

        /// <summary>
        /// Returns a value indicating whether payment method should be hidden during checkout
        /// </summary>
        /// <param name="cart">Shopping cart</param>
        /// <returns>true - hide; false - display.</returns>
        public bool HidePaymentMethod(IList<ShoppingCartItem> cart)
        {
            //you can put any logic here
            //for example, hide this payment method if all products in the cart are downloadable
            //or hide this payment method if current customer is from certain country
            return false;
        }

        /// <summary>
        /// Gets additional handling fee
        /// </summary>
        /// <param name="cart">Shopping cart</param>
        /// <returns>Additional handling fee</returns>
        public decimal GetAdditionalHandlingFee(IList<ShoppingCartItem> cart)
        {
            if (cart == null)
                throw new ArgumentNullException(nameof(cart));

            return _paymentService.CalculateAdditionalFee(cart, _osuPaymentSettings.AdditionalFee, _osuPaymentSettings.AdditionalFeePercentage);
        }

        /// <summary>
        /// Captures payment
        /// </summary>
        /// <param name="capturePaymentRequest">Capture payment request</param>
        /// <returns>Capture payment result</returns>
        public CapturePaymentResult Capture(CapturePaymentRequest capturePaymentRequest)
        {
            return new CapturePaymentResult { Errors = new[] { "Capture method not supported" } };
        }

        /// <summary>
        /// Refunds a payment
        /// </summary>
        /// <param name="refundPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public RefundPaymentResult Refund(RefundPaymentRequest refundPaymentRequest)
        {
            return new RefundPaymentResult { Errors = new[] { "Refund method not supported" } };
        }

        /// <summary>
        /// Voids a payment
        /// </summary>
        /// <param name="voidPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public VoidPaymentResult Void(VoidPaymentRequest voidPaymentRequest)
        {
            return new VoidPaymentResult { Errors = new[] { "Void method not supported" } };
        }

        /// <summary>
        /// Process recurring payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>Process payment result</returns>
        public ProcessPaymentResult ProcessRecurringPayment(ProcessPaymentRequest processPaymentRequest)
        {
            return new ProcessPaymentResult { Errors = new[] { "Recurring payment not supported" } };
        }

        /// <summary>
        /// Cancels a recurring payment
        /// </summary>
        /// <param name="cancelPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public CancelRecurringPaymentResult CancelRecurringPayment(CancelRecurringPaymentRequest cancelPaymentRequest)
        {
            return new CancelRecurringPaymentResult { Errors = new[] { "Recurring payment not supported" } };
        }

        /// <summary>
        /// Gets a value indicating whether customers can complete a payment after order is placed but not completed (for redirection payment methods)
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>Result</returns>
        public bool CanRePostProcessPayment(Order order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            return false;
        }

        /// <summary>
        /// Validate payment form
        /// </summary>
        /// <param name="form">The parsed form values</param>
        /// <returns>List of validating errors</returns>
        public IList<string> ValidatePaymentForm(IFormCollection form)
        {
            return new List<string>();
        }

        /// <summary>
        /// Get payment information
        /// </summary>
        /// <param name="form">The parsed form values</param>
        /// <returns>Payment info holder</returns>
        public ProcessPaymentRequest GetPaymentInfo(IFormCollection form)
        {
            if (form == null)
                throw new ArgumentNullException(nameof(form));

            return _actionContextAccessor.ActionContext.HttpContext.Session.Get<ProcessPaymentRequest>(Defaults.PaymentRequestSessionKey);
        }

        /// <summary>
        /// Gets a configuration page URL
        /// </summary>
        public override string GetConfigurationPageUrl()
        {
            return _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext).RouteUrl(Defaults.ConfigurationRouteName);
        }

        /// <summary>
        /// Gets a name of a view component for displaying plugin in public store ("payment info" checkout step)
        /// </summary>
        /// <returns>View component name</returns>
        public string GetPublicViewComponentName()
        {
            return Defaults.PAYMENT_INFO_VIEW_COMPONENT_NAME;
        }

        /// <summary>
        /// Install the plugin
        /// </summary>
        public override void Install()
        {
            if (!_widgetSettings.ActiveWidgetSystemNames.Contains(Defaults.SystemName))
            {
                _widgetSettings.ActiveWidgetSystemNames.Add(Defaults.SystemName);
                _settingService.SaveSetting(_widgetSettings);
            }

            _settingService.SaveSetting(new OsuPaymentSettings
            {
                WidgetUrl = Defaults.OsuWidget.SandboxUrl
            });

            _localizationService.AddPluginLocaleResource(new Dictionary<string, string>
            {
                ["Plugins.Payments.Osu.Fields.AdditionalFee"] = "Additional fee",
                ["Plugins.Payments.Osu.Fields.AdditionalFee.Hint"] = "Enter additional fee to charge your customers.",
                ["Plugins.Payments.Osu.Fields.AdditionalFee.ShouldBeGreaterThanOrEqualZero"] = "The additional fee should be greater than or equal 0",
                ["Plugins.Payments.Osu.Fields.AdditionalFeePercentage"] = "Additional fee. Use percentage",
                ["Plugins.Payments.Osu.Fields.AdditionalFeePercentage.Hint"] = "Determines whether to apply a percentage additional fee to the order total. If not enabled, a fixed value is used.",
                ["Plugins.Payments.Osu.Fields.ApiKey"] = "API key",
                ["Plugins.Payments.Osu.Fields.ApiKey.Hint"] = "Provide the Osu API key for authentication. You can find it in the Osu dashboard.",
                ["Plugins.Payments.Osu.Fields.ApiKey.Required"] = "The API key is required",
                ["Plugins.Payments.Osu.Fields.PublicKey"] = "Public key",
                ["Plugins.Payments.Osu.Fields.PublicKey.Hint"] = "Provide the public key for verification of the transactions from Osu.",
                ["Plugins.Payments.Osu.Fields.PublicKey.Required"] = "The public key is required",
                ["Plugins.Payments.Osu.Fields.WidgetUrl"] = "Widget URL",
                ["Plugins.Payments.Osu.Fields.WidgetUrl.Hint"] = "Provide the widget URL.",
                ["Plugins.Payments.Osu.Fields.WidgetUrl.Required"] = "The widget URL is required",
                ["Plugins.Payments.Osu.Instructions"] = @"
                    <div style=""margin: 0 0 10px;"">
                        For plugin configuration, follow these steps:<br />
                        <br />
                        1. You will need a Osu Merchant account. If you don't already have one, you can sign up here: <a href=""https://www.payosu.com/"" target=""_blank"">https://www.payosu.com/</a><br />
                        2. For the 'production' enviroment you should request the production widget URL and 'Public key' from Osu support then paste them into same fields.<br />
                        3. Sign in to 'Osu Dashboard'. Go to 'Profile' &#8594; 'Webhooks' tab and paste this URL <em>{0}</em> into 'Payment success URL' field.<br />
                        <em>Notes:</em><br />
                        For this plugin to work properly ensure that your primary store currency is supported by Osu.<br />
                    </div>
                ",
                ["Plugins.Payments.Osu.PaymentMethodDescription"] = "Pay by Osu",
                ["Plugins.Payments.Osu.PaymentInfoIsNotConfigured"] = "Plugin is not configured correctly.",
                ["Plugins.Payments.Osu.Payment.Successful"] = "We have received your payment. Thanks!"
            });

            base.Install();
        }

        /// <summary>
        /// Uninstall the plugin
        /// </summary>
        public override void Uninstall()
        {
            if (_widgetSettings.ActiveWidgetSystemNames.Contains(Defaults.SystemName))
            {
                _widgetSettings.ActiveWidgetSystemNames.Remove(Defaults.SystemName);
                _settingService.SaveSetting(_widgetSettings);
            }

            _settingService.DeleteSetting<OsuPaymentSettings>();

            _localizationService.DeletePluginLocaleResources("Plugins.Payments.Osu");

            base.Uninstall();
        }

        public IList<string> GetWidgetZones()
        {
            return new List<string>
            {
                PublicWidgetZones.CheckoutPaymentInfoBottom,
                PublicWidgetZones.OpcContentBefore,
            };
        }

        public string GetWidgetViewComponentName(string widgetZone)
        {
            if (widgetZone == null)
                throw new ArgumentNullException(nameof(widgetZone));

            if (widgetZone.Equals(PublicWidgetZones.CheckoutPaymentInfoBottom) ||
                widgetZone.Equals(PublicWidgetZones.OpcContentBefore))
            {
                return Defaults.OsuWidget.SCRIPT_VIEW_COMPONENT_NAME;
            }

            return string.Empty;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a value indicating whether capture is supported
        /// </summary>
        public bool SupportCapture => false;

        /// <summary>
        /// Gets a value indicating whether partial refund is supported
        /// </summary>
        public bool SupportPartiallyRefund => false;

        /// <summary>
        /// Gets a value indicating whether refund is supported
        /// </summary>
        public bool SupportRefund => false;

        /// <summary>
        /// Gets a value indicating whether void is supported
        /// </summary>
        public bool SupportVoid => false;

        /// <summary>
        /// Gets a recurring payment type of payment method
        /// </summary>
        public RecurringPaymentType RecurringPaymentType => RecurringPaymentType.NotSupported;

        /// <summary>
        /// Gets a payment method type
        /// </summary>
        public PaymentMethodType PaymentMethodType => PaymentMethodType.Standard;

        /// <summary>
        /// Gets a value indicating whether we should display a payment information page for this plugin
        /// </summary>
        public bool SkipPaymentInfo => false;

        /// <summary>
        /// Gets a payment method description that will be displayed on checkout pages in the public store
        /// </summary>
        public string PaymentMethodDescription => _localizationService.GetResource("Plugins.Payments.Osu.PaymentMethodDescription");

        /// <summary>
        /// Gets a value indicating whether to hide this plugin on the widget list page in the admin area
        /// </summary>
        public bool HideInWidgetList => true;

        #endregion
    }
}