using System;
using System.Collections.Generic;
using System.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

using Microsoft.IdentityModel.Tokens;

namespace Back_End.Clases.Seguridad
{
    internal class ValidarTokenHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
                                                               CancellationToken cancellationToken)
        {
            HttpStatusCode statusCode;
            AppSettings appSettings = new AppSettings();

            if (!TryRetrieveToken(request, out string token))
            {
                statusCode = HttpStatusCode.Unauthorized;
                return base.SendAsync(request, cancellationToken);
            }

            try
            {
                var claveSecreta = appSettings.Secret;
                var key = System.Text.Encoding.Default.GetBytes(claveSecreta);
                var issuerToken = appSettings.Issuer;
                var audienceToken = appSettings.Audience;

                var securityKey = new SymmetricSecurityKey(key);

                Microsoft.IdentityModel.Logging.IdentityModelEventSource.ShowPII = true;

                var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                TokenValidationParameters validationParameters = new TokenValidationParameters()
                {
                    //ValidateIssuerSigningKey = true,
                    //IssuerSigningKey = securityKey,
                    //ValidateLifetime = true,
                    //LifetimeValidator = this.LifetimeValidator, // DELEGADO PERSONALIZADO PERA COMPROBAR LA CADUCIDAD EL TOKEN.

                    //ValidateIssuer = true,
                    //ValidateAudience = true,

                    //ValidIssuer = issuerToken,
                    //ValidAudience = audienceToken,
                    //RequireSignedTokens = false,
                    //ValidAudience = audienceToken,
                    //ValidIssuer = issuerToken,
                    //ValidateLifetime = true,
                    //ValidateIssuerSigningKey = true,
                    //// DELEGADO PERSONALIZADO PERA COMPROBAR
                    //// LA CADUCIDAD EL TOKEN.
                    ////LifetimeValidator = this.LifetimeValidator,
                    //IssuerSigningKey = securityKey

                    ValidAudience = audienceToken,
                    ValidIssuer = issuerToken,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    // DELEGADO PERSONALIZADO PERA COMPROBAR
                    // LA CADUCIDAD EL TOKEN.
                    LifetimeValidator = this.LifetimeValidator,
                    IssuerSigningKey = securityKey
                };

                // COMPRUEBA LA VALIDEZ DEL TOKEN
                Thread.CurrentPrincipal = tokenHandler.ValidateToken(token,
                                                                     validationParameters,
                                                                     out SecurityToken securityToken);
                HttpContext.Current.User = tokenHandler.ValidateToken(token,
                                                                      validationParameters,
                                                                      out securityToken);

                return base.SendAsync(request, cancellationToken);
            }
            catch (SecurityTokenValidationException ex)
            {
                statusCode = HttpStatusCode.Unauthorized;
            }
            catch (Exception ex)
            {
                statusCode = HttpStatusCode.InternalServerError;
            }

            return Task<HttpResponseMessage>.Factory.StartNew(() =>
                        new HttpResponseMessage(statusCode) { });
        }

        // RECUPERA EL TOKEN DE LA PETICIÓN
        private static bool TryRetrieveToken(HttpRequestMessage request, out string token)
        {
            token = null;
            if (!request.Headers.TryGetValues("Authorization", out IEnumerable<string> authzHeaders) ||
                                              authzHeaders.Count() > 1)
            {
                return false;
            }

            var bearerToken = authzHeaders.ElementAt(0);

            token = bearerToken.StartsWith("Bearer ") ?
                    bearerToken.Substring(7) : bearerToken;

            return true;
        }

        // COMPRUEBA LA CADUCIDAD DEL TOKEN
        public bool LifetimeValidator(DateTime? notBefore,
                                      DateTime? expires,
                                      SecurityToken securityToken,
                                      TokenValidationParameters validationParameters)
        {
            var valid = false;

            if ((expires.HasValue && DateTime.UtcNow < expires)
                && (notBefore.HasValue && DateTime.UtcNow > notBefore))
            { valid = true; }

            return valid;
        }
    }
}