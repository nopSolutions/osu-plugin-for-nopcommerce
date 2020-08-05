using Newtonsoft.Json;

namespace Nop.Plugin.Payments.Osu.Models
{
    public class WebHookSuccessPaymentRequest
    {
        [JsonProperty("paymentId")]
        public string PaymentId { get; set; }

        [JsonProperty("paymentData")]
        public PaymentData PaymentData { get; set; }

        [JsonProperty("payerData")]
        public PayerData PayerData { get; set; }
    }
}
