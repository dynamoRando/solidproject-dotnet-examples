﻿using IdentityModel.Client;
using IdentityModel.OidcClient;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace SolidDotNet
{
    public class SolidClient
    {
        #region Private Fields
        private static RSAParameters _privateKey;
        private static RSAParameters _publicKey;
        private static RsaSecurityKey _privateRSAKey;
        private static RsaSecurityKey _publicRSAKey;

        private static bool _hasGeneratedKeys = false;
        private static string _identityProviderUrl = string.Empty;
        private static HttpClient _client;
        private static string _appName;
        private static string _appScopes = "openid offline_access webid";
        private static bool _useDebug = true;
        private static string[] _redirectUris;

        // private values
        private static string _clientId;
        private static string _clientSecret;
        private static string _clientCodeVerifier;
        private static string _clientAppCode;
        private static string _clientIdToken;
        private static string _clientAccessToken;
        #endregion

        #region Public Properties
        public bool HasGeneratedKeys => _hasGeneratedKeys;
        public string IdentityProviderUrl => _identityProviderUrl;
        public string AppCode => _clientAppCode;
        #endregion

        #region Constructors
        public SolidClient()
        {
            _client = new HttpClient();
        }
        public SolidClient(string identityProvider) : this()
        {
            _identityProviderUrl = identityProvider;
        }
        #endregion

        #region Public Methods
        public async Task GetAccessAndIdTokens(string appCode, string issuerUrl, string audienceUrl, string authorizationUrl)
        {
            _clientAppCode = appCode;
            await GetAccessAndIdTokens(issuerUrl, audienceUrl, authorizationUrl);
        }

        public async Task GetAccessAndIdTokens(string issuerUrl, string audienceUrl, string authorizationUrl)
        {
            string url = _identityProviderUrl + "/idp/token";
            string jwtToken = BuildJwt(issuerUrl, audienceUrl, authorizationUrl);
            var tokenHandler = new JwtSecurityTokenHandler();

            var validationParameters = new TokenValidationParameters
            {
                ClockSkew = TimeSpan.FromMinutes(5),
                IssuerSigningKey = _privateRSAKey,
                RequireSignedTokens = true,
                RequireExpirationTime = true,
                ValidateLifetime = true,
                ValidateAudience = true,
                ValidAudience = audienceUrl,
                ValidateIssuer = true,
                ValidIssuer = issuerUrl
            };

            SecurityToken token;
            tokenHandler.ValidateToken(jwtToken, validationParameters, out token);

            if (tokenHandler.CanReadToken(jwtToken))
            {
                DebugOut("Get Token: Token Is Valid");
            }

            DebugOut($"Get Token: Address {url}");
            DebugOut($"Get Token: ClientId {_clientIdToken}");
            DebugOut($"Get Token: Secret {_clientSecret}");
            DebugOut($"Get Token: AppCode {_clientAppCode}");
            DebugOut($"Get Token: CodeVerifier {_clientCodeVerifier}");
            DebugOut($"Get Token: JWTToken {jwtToken}");

            var client = new HttpClient();
            client.DefaultRequestHeaders.Clear();

            // per https://solid.github.io/solid-oidc/primer/#authorization-code-pkce-flow
            // send the token in the header
            client.DefaultRequestHeaders.Add("DPoP", jwtToken);

            DebugOut("Get Token: Request started...");

            var response = await client.RequestAuthorizationCodeTokenAsync(new AuthorizationCodeTokenRequest
            {
                // TO DO: This should be looked up, rather than manually built
                Address = _identityProviderUrl + "/idp/token",
                ClientId = _clientId,
                ClientSecret = _clientSecret,
                Code = _clientAppCode,
                RedirectUri = _redirectUris.First(),
                CodeVerifier = _clientCodeVerifier,
                GrantType = "authorization_code",
                ClientCredentialStyle = ClientCredentialStyle.PostBody
            }); ;

            DebugOut("Token Response:" + response.HttpStatusCode.ToString());

            if (response.HttpStatusCode == HttpStatusCode.OK)
            {
                var id_token = response.IdentityToken;
                var access_token = response.AccessToken;

                var jwt_id = new JwtSecurityToken(id_token);
                var jwt_access = new JwtSecurityToken(access_token);

                _clientIdToken = id_token;
                _clientAccessToken = access_token;

                DebugOut("Get Token: Tokens Saved To Storage");
            }
            else
            {
                throw new Exception(response.ErrorDescription);
            }
        }

        public async Task<string> GetLoginUrl(string redirectUrl)
        {
            // https://identitymodel.readthedocs.io/en/latest/native/manual.html?highlight=OidcClientOptions
            var options = new OidcClientOptions
            {
                // who is authenticating us, in this case, the community server
                Authority = _identityProviderUrl,

                // our client id from earlier when we identfied our app (registered via dynamic registration)
                ClientId = _clientId,

                // once we've logged in, tell the identity server where to send the user back to (in this case, our app)
                RedirectUri = redirectUrl,

                // the scope
                Scope = _appScopes
            };

            var client = new OidcClient(options);
            var state = await client.PrepareLoginAsync();

            string url = state.StartUrl;

            // IdentityMode.OdicClient generates a code challenge and verifier for us
            // this is step 4 of here https://solid.github.io/solid-oidc/primer/#authorization-code-pkce-flow
            // we will need the Code Verifier later, so let's save it
            _clientCodeVerifier = state.CodeVerifier;

            url += "&prompt=consent&response_mode=query";

            return url;
        }

        public async Task RegisterAppAsync(string identityProvider, string[] redirectUris, string appName)
        {
            _identityProviderUrl = identityProvider;
            await RegisterAppAsync(redirectUris, appName);
        }

        public async Task RegisterAppAsync(string[] redirectUris, string appName)
        {
            if (string.IsNullOrEmpty(_appName))
            {
                _appName = appName;
            }

            _redirectUris = redirectUris;

            // TO DO: This should be looked up, rather than manually built
            string url = _identityProviderUrl + "/idp/reg";

            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var contentBuilder = new StringBuilder();
            contentBuilder.Append("{");
            contentBuilder.Append(@"""application_type"": ""web"",");
            contentBuilder.Append(@"""redirect_uris"":[");

            if (redirectUris.Length == 1)
            {
                contentBuilder.Append($@"""{redirectUris.First()}""");
            }
            else
            {
                foreach (var uri in redirectUris)
                {
                    contentBuilder.Append($@"""{uri}""");
                    if (redirectUris.Last() != uri)
                    {
                        contentBuilder.Append(",");
                    }
                }
            }
            contentBuilder.Append("],");
            contentBuilder.Append($@"""client_name"": ""{appName}""");
            contentBuilder.Append("}");

            var stringContent = new StringContent(contentBuilder.ToString(), Encoding.UTF8, "application/json");
            var result = await _client.PostAsync(url, stringContent);
            var data = await result.Content.ReadAsStringAsync();
            var item = JsonConvert.DeserializeObject<RegisterData>(data);

            if (item is not null)
            {
                _clientId = item.client_id;
                _clientSecret = item.client_secret;
            }
        }

        public void SetIdentityProvider(string identityProvider)
        {
            _identityProviderUrl = identityProvider;
        }

        public void SetAppName(string appName)
        {
            _appName = appName;
        }

        public void SetAppCode(string appCode)
        {
            _clientAppCode = appCode;
        }

        public void GenerateKeys()
        {
            var rsa = RSA.Create();

            _publicKey = rsa.ExportParameters(false);
            _privateKey = rsa.ExportParameters(true);

            _privateRSAKey = new RsaSecurityKey(_privateKey);
            _publicRSAKey = new RsaSecurityKey(_publicKey);

            _hasGeneratedKeys = true;
        }

        public JsonWebKey GetPrivateJsonWebKey()
        {
            if (!HasGeneratedKeys)
            {
                GenerateKeys();
            }

            return JsonWebKeyConverter.ConvertFromRSASecurityKey(_privateRSAKey);
        }

        public JsonWebKey GetPublicJsonWebKey()
        {
            if (!HasGeneratedKeys)
            {
                GenerateKeys();
            }

            return JsonWebKeyConverter.ConvertFromRSASecurityKey(_publicRSAKey);
        }

        public string BuildJwt(string issuerUrl, string audienceUrl, string authorizationUrl)
        {
            if (!HasGeneratedKeys)
            {
                GenerateKeys();
            }

            // stolen from the internet to compute iat, exp values
            var utc0 = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            var issueTime = DateTime.UtcNow;

            var iat = (int)issueTime.Subtract(utc0).TotalSeconds;
            var exp = (int)issueTime.AddMinutes(55).Subtract(utc0).TotalSeconds;

            // a secret key that we know
            var key = new RsaSecurityKey(_privateKey);

            // how this token is generated
            var creds = new SigningCredentials(key, SecurityAlgorithms.RsaSha256);

            // who issued the key, in this case, ourselves
            var issuer = issuerUrl;

            // who this for, in this case, ourselves
            var audience = audienceUrl;

            // how long is this token good for
            var jwtValidity = DateTime.Now.AddDays(1);

            // identify the type of crypto for our keys
            var algMap = new Dictionary<string, string>();
            algMap.Add("alg", "RS256");

            // I build a base token because I'm going to have to heavily modify the final JWT token,
            // so this is a starting point to get the claims in the inital token
            var baseToken = new JwtSecurityToken(issuer,
              audience,
              expires: jwtValidity,
              signingCredentials: creds);

            // https://solid.github.io/solid-oidc/primer/#authorization-code-pkce-flow
            // per the above, we need to send as "dpop+jwt"
            var header = new JwtHeader(creds, algMap, "dpop+jwt");

            // per the above link, send the public key in the header
            var jwk = GetPublicJsonWebKey();
            header.Add("jwk", jwk);

            // add the inital claims
            var payload = new JwtPayload(baseToken.Claims);

            // we want to only use this token at the Community Solid server
            payload.AddClaim(new Claim("htu", authorizationUrl));

            // only on post methods
            payload.AddClaim(new Claim("htm", "POST"));

            // unique identifier for the token
            payload.AddClaim(new Claim("jti", Guid.NewGuid().ToString()));

            // we rebuild the token with all the additional headers, claims, etc.
            var dpopToken = new JwtSecurityToken(header, payload, header.ToString(), payload.ToString(), baseToken.RawSignature);

            // the date when the token was issued, must be an integer (not a string)
            // this is required
            dpopToken.Payload.AddClaim(new Claim("iat", iat.ToString(), ClaimValueTypes.Integer));

            // debugging
            var text = new JwtSecurityTokenHandler().WriteToken(dpopToken);

            // we can validate the text at https://jwt.io/ if we want
            return text;
        }
        #endregion

        #region Private Methods
        private void DebugOut(string item)
        {
            if (_useDebug)
            {
                Console.WriteLine(item);
                Debug.WriteLine(item);
            }
        }
        #endregion

    }
}
