using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Net;

namespace api.fefarm.mx.MessageHandlers
{
    public class APIKeyMessageHandlercs : DelegatingHandler
    {
        private const string APIKeyToCheck = "yaIMUjbmh749cIv20i056xhaNbUSj8Oh";

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage httpRequestMessage, CancellationToken cancellationToken)
        {
            IEnumerable<string> requestHeaders;

            bool checkAPIKeyExists = httpRequestMessage.Headers.TryGetValues("APIKey", out requestHeaders);

            bool validKey = checkAPIKeyExists ? requestHeaders.FirstOrDefault().Equals(APIKeyToCheck) : false;

            if(!validKey)
            {
                return httpRequestMessage.CreateResponse(HttpStatusCode.Forbidden, "Invalid API Key");
            }

            var response = await base.SendAsync(httpRequestMessage, cancellationToken);
            return response;
        }
    }
}