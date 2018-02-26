using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Forms.Infrastructure
{
    public class APIGatewayToken
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("token_type")]
        public string TokenType { get; set; }

        [JsonProperty("expires_in")]
        public long ExpiresIn { get; set; }

        [JsonProperty("scope")]
        public string Scope { get; set; }

        public DateTime CreatedDT { get; } = DateTime.UtcNow;

        public DateTime ExpiresDT
        {
            get { return CreatedDT.AddSeconds(this.ExpiresIn); }
        }
    }
}
