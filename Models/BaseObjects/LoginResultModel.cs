using Newtonsoft.Json;

namespace Models
{
    public class LoginResultModel
    {
        [JsonProperty("_id")]
        public string characterId { get; set; }
        public string auth_key { get; set; }
    }
}