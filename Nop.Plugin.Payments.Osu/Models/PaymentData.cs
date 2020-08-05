using Newtonsoft.Json;

namespace Nop.Plugin.Payments.Osu.Models
{
    public class PaymentData
    {
        [JsonProperty("amountInCents")]
        public string AmountInCents { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("reference")]
        public string Reference { get; set; }

        [JsonProperty("paymentType")]
        public string PaymentType { get; set; }
        
        [JsonProperty("paymentStatus")]
        public string PaymentStatus { get; set; }
    }
}
