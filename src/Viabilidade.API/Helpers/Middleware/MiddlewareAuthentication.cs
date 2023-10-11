using System.Security.Claims;
using System.Text;
using Viabilidade.Domain.Exceptions;
using Viabilidade.Domain.Interfaces.Services.Host;

namespace Viabilidade.API.Helpers.Middleware
{
    public class MiddlewareAuthentication
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<MiddlewareAuthentication> _logger;

        public MiddlewareAuthentication(RequestDelegate next, ILogger<MiddlewareAuthentication> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext httpContext, IHostService hostService)
        {
            bool needToValidateSessionOnAltasHost = true;
            string[] pathValidationExclusion = { "health", "index.html", "favicon.ico", "swagger", "oidc-callback" };

            string encodedUrl = httpContext.Request.Path.ToString().ToLower();
            needToValidateSessionOnAltasHost = !pathValidationExclusion.Any(encodedUrl.Contains);

            _logger.LogInformation($"Path {encodedUrl} need to validate session on Altas Host: {needToValidateSessionOnAltasHost} ");

            if (!needToValidateSessionOnAltasHost)
                return;

            var user = await hostService.GetUserCredentialsAsync();
            if (user != null)
            {
                var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Sid, user.Sub.ToString()),
                        new Claim(ClaimTypes.Name, user.Name.ToString()),
                        new Claim(ClaimTypes.Email, user.Email.ToString())
                    };
                if (user.Roles != null)
                {
                    foreach (var role in user?.Roles)
                        claims.Add(new Claim(ClaimTypes.Role, role));
                }

                var appIdentity = new ClaimsIdentity(claims);
                httpContext.User.AddIdentity(appIdentity);

                httpContext.Response.StatusCode = StatusCodes.Status200OK;
                await _next(httpContext);
            }
            else
            {
                throw new DomainException("Unauthorized", 401);
            }

        }
    }
}
