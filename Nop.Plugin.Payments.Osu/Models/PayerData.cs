using Newtonsoft.Json;

namespace Nop.Plugin.Payments.Osu.Models
{
    public class PayerData
    {
        [JsonProperty("firstName")]
        public string FirstName { get; set; }

        [JsonProperty("lastName")]
        public string LastName { get; set; }

        [JsonProperty("phoneNumber")]
        public string PhoneNumber { get; set; }

        [JsonProperty("postCode")]
        public string PostCode { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }
    }
}
