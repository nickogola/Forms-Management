using Forms.Infrastructure.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using P8ServicesV1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.ServiceModel;
using System.Threading.Tasks;

namespace Forms.Infrastructure
{
    public class P8ServicesV1Proxy : IP8ServicesV1Proxy
    {
        private readonly P8ServicesV1Options _options;
        private readonly IMemoryCache _cache;

        public P8ServicesV1Proxy(IOptions<P8ServicesV1Options> options, IMemoryCache cache)
        {
            _options = options.Value;
            _cache = cache;
        }

        public async Task<P8ServiceClient> GetClientAsync()
        {
            var binding = new BasicHttpsBinding
            {
                MaxReceivedMessageSize = 314572800,
                MaxBufferSize = 314572800,
                MessageEncoding = WSMessageEncoding.Mtom
            };

            var token = await GetTokenAsync();
            var address = new EndpointAddress(_options.Uri);
            var client = new P8ServicesV1.P8ServiceClient(binding, address);
            client.Endpoint.EndpointBehaviors.Add(new P8ServicesV1EndpointBehavior(token));
            return client;
        }

        private async Task<APIGatewayToken> GetTokenAsync()
        {
            APIGatewayToken token;
            var cacheKey = "APIGatewayToken";

            if (_cache.TryGetValue(cacheKey, out token))
                return token;

            var client = new HttpClient();
            var oAuthParams = new Dictionary<string, string>
            {
                { "grant_type", "client_credentials" },
                { "client_id", _options.ClientID },
                { "client_secret", _options.ClientSecret },
            };

            var response = await client.PostAsync(_options.APIGatewayUri, new FormUrlEncodedContent(oAuthParams));

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            token = JsonConvert.DeserializeObject<APIGatewayToken>(json);
            _cache.Set(cacheKey, token, new DateTimeOffset(token.ExpiresDT));

            return token;
        }
    }
}
