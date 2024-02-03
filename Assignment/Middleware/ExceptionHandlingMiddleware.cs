using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

public class ExceptionHandlingMiddleware : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        try
        {
            // Process the request
            return await base.SendAsync(request, cancellationToken);
        }
        catch (Exception)
        {           
            var response = request.CreateErrorResponse(HttpStatusCode.InternalServerError, "An unexpected error occurred.");
            return response;
        }
    }
}
