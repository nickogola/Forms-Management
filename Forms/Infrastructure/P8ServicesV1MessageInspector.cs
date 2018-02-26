using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using System.Threading.Tasks;

namespace Forms.Infrastructure
{
    public class P8ServicesV1MessageInspector : IClientMessageInspector
    {
        private readonly APIGatewayToken _token;

        public P8ServicesV1MessageInspector(APIGatewayToken token)
        {
            _token = token;
        }

        public void AfterReceiveReply(ref Message reply, object correlationState)
        {

        }

        public object BeforeSendRequest(ref Message request, IClientChannel channel)
        {
            HttpRequestMessageProperty property = new HttpRequestMessageProperty();

            property.Headers["Authorization"] = string.Format("Bearer {0}", _token.AccessToken);
            request.Properties.Add(HttpRequestMessageProperty.Name, property);

            return null;
        }
    }
}
