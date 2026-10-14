using System.Net;
using System.Net.Http;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;

namespace PhotoCenterWeb.Helpers
{
    public class JwtAuthenticationHandler
        : System.Net.Http.DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var authorization =
                request.Headers.Authorization;

            if (authorization != null &&
                authorization.Scheme == "Bearer" &&
                !string.IsNullOrWhiteSpace(
                    authorization.Parameter))
            {
                var principal =
                    JwtHelper.ValidateToken(
                        authorization.Parameter
                    );

                if (principal != null)
                {
                    Thread.CurrentPrincipal = principal;

                    if (request.GetRequestContext() != null)
                    {
                        request.GetRequestContext().Principal =
                            principal;
                    }
                }
                else
                {
                    return request.CreateResponse(
                        HttpStatusCode.Unauthorized,
                        new
                        {
                            message = "Недействительный JWT-токен."
                        }
                    );
                }
            }

            return await base.SendAsync(
                request,
                cancellationToken
            );
        }
    }
}