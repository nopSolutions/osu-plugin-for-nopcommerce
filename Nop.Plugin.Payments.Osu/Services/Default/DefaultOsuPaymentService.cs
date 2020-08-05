using System;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Orders;

namespace Nop.Plugin.Payments.Osu.Services
{
    /// <summary>
    /// Provides an default implementation to manage the Osu payment workflow
    /// </summary>
    public class DefaultOsuPaymentService : IOsuPaymentService
    {
        #region Fields

        private readonly ICustomerService _customerService;
        private readonly IOrderService _orderService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IStoreContext _storeContext;

        #endregion

        #region Ctor

        public DefaultOsuPaymentService(
            ICustomerService customerService,
            IOrderService orderService,
            IOrderProcessingService orderProcessingService,
            IGenericAttributeService genericAttributeService,
            IStoreContext storeContext)
        {
            _customerService = customerService;
            _orderService = orderService;
            _orderProcessingService = orderProcessingService;
            _genericAttributeService = genericAttributeService;
            _storeContext = storeContext;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Captures the payment transaction id for user
        /// </summary>
        /// <param name="transactionId">The payment transaction id</param>
        public virtual void CaptureTransactionId(Customer customer, string transactionId)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            _genericAttributeService.SaveAttribute(
                customer, Defaults.PaymentTransactionIdAttribute, transactionId, _storeContext.CurrentStore.Id);
        }

        /// <summary>
        /// Gets the captured payment transaction id for user
        /// </summary>
        /// <returns>The payment transaction id</returns>
        public virtual string GetCapturedTransactionId(Customer customer)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            return _genericAttributeService.GetAttribute<string>(
                customer, Defaults.PaymentTransactionIdAttribute, _storeContext.CurrentStore.Id);
        }

        /// <summary>
        /// Captures the order with specified transaction id
        /// </summary>
        /// <param name="order">The order</param>
        /// <param name="transactionId">The payment transaction id</param>
        public virtual void CaptureOrder(Order order, string transactionId)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            if (transactionId == null)
                throw new ArgumentNullException(nameof(transactionId));

            if (string.IsNullOrWhiteSpace(transactionId))
                throw new InvalidOperationException($"The payment transaction '{transactionId}' should not be empty or white space.");

            if (_orderProcessingService.CanMarkOrderAsPaid(order))
            {
                order.CaptureTransactionId = transactionId;
                _orderService.UpdateOrder(order);
                _orderProcessingService.MarkOrderAsPaid(order);

                var customer = _customerService.GetCustomerById(order.CustomerId);
                if (customer != null)
                    CaptureTransactionId(customer, null);
            }
        }

        #endregion
    }
}
