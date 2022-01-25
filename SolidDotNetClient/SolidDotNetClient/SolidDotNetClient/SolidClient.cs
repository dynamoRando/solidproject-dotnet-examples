using IdentityModel.Client;
using IdentityModel.OidcClient;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using SolidDotNetClient;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using VDS.RDF;
using VDS.RDF.Parsing;

namespace SolidDotNet
{
    /// <summary>
    /// Helper class for performing various actions at a Solid Pod
    /// </summary>
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
        private static EndpointInfo _endpointInfo;

        // private values
        private static string _clientId;
        private static string _clientSecret;
        private static string _clientCodeVerifier;
        private static string _clientAppCode;
        private static string _clientIdToken;
        private static string _clientAccessToken;

        // values at the pod
        private static UriCollection _folders;
        #endregion

        #region Public Properties
        public bool HasGeneratedKeys => _hasGeneratedKeys;
        public string IdentityProviderUrl => _identityProviderUrl;
        public string AppCode => _clientAppCode;
        public bool UseDebug => _useDebug;
        public string Access_Token => _clientAccessToken;
        public string Client_Token = _clientIdToken;
        public bool HasClientToken => !string.IsNullOrEmpty(_clientIdToken);
        public bool HasAccessToken => !string.IsNullOrEmpty(_clientAccessToken);
        public bool IsLoggedIn => HasAccessToken;
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
        /// <summary>
        /// Updates a document in the specified folder at the pod with the specific name and content
        /// </summary>
        /// <param name="folderName">The name of the folder at the pod</param>
        /// <param name="docName">The name of the document at the pod</param>
        /// <param name="docContent">The content of the file, as a Turtle RDF document</param>
        /// <returns></returns>
        /// <remarks>Performs an HTTP PUT</remarks>
        public async Task UpdateRdfDocumentAsync(string folderName, string docName, string docContent)
        {
            var uri = _folders.Get(folderName);

            if (uri is not null)
            {
                if (_identityProviderUrl is not null)
                {
                    if (_client is not null)
                    {
                        var docLocation = uri.OriginalString + docName;

                        if (!docLocation.EndsWith("/"))
                        {
                            docLocation = docLocation + "/";
                        }

                        // see the section "Creating Documents (Files)
                        // https://github.com/solid/solid-spec/blob/master/api-rest.md

                        /*
                        PUT / HTTP/1.1
                        Host: example.org
                        Content-Type: text/turtle
                        Link: <http://www.w3.org/ns/ldp#Resource>; rel="type"
                        Slug: test

                        <> <http://purl.org/dc/terms/title> "This is a test file" .
                        */

                        // <> <http://purl.org/dc/terms/title> "This is a test file" .
                        // not sure if we should try to construct an rdf document here?
                        string content = docContent;

                        string domain = IdentityProviderUrl.Replace("http://", string.Empty).Replace("https://", string.Empty);
                        string targetUrl = string.Empty;

                        if (!IdentityProviderUrl.EndsWith('/'))
                        {
                            targetUrl = IdentityProviderUrl + "/";
                        }
                        else
                        {
                            targetUrl = IdentityProviderUrl;
                        }

                        try
                        {
                            _client.DefaultRequestHeaders.Clear();
                            _client.DefaultRequestHeaders.Add("Host", domain);
                            _client.DefaultRequestHeaders.Add("authorization", "DPoP " + Access_Token);
                            _client.DefaultRequestHeaders.Add("DPoP", BuildJwtForContent("PUT", targetUrl));

                            var stringContent = new StringContent(content, Encoding.UTF8, "text/turtle");
                            var result = await _client.PutAsync(docLocation, stringContent);

                            DebugOut(result.StatusCode.ToString());
                        }
                        catch (Exception ex)
                        {
                            DebugOut(ex.ToString());
                            throw ex;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Deletes a document in the specified folder at the pod with the specific name
        /// </summary>
        /// <param name="folderName">The name of the folder at the pod</param>
        /// <param name="docName">The name of the document at the pod</param>
        /// <returns></returns>
        /// <remarks>Performs a HTTP DELETE</remarks>
        public async Task DeleteRdfDocumentAsync(string folderName, string docName)
        {
            var uri = _folders.Get(folderName);

            if (uri is not null)
            {
                if (_identityProviderUrl is not null)
                {
                    if (_client is not null)
                    {
                        var docLocation = uri.OriginalString + docName;

                        if (!docLocation.EndsWith("/"))
                        {
                            docLocation = docLocation + "/";
                        }

                        // https://www.w3.org/TR/ldp-primer/#deleting-a-resource-delete-on-an-ldpr

                        /*
                        DELETE /alice/avatar HTTP/1.1
                        Host: example.org       
                        */

                        string domain = IdentityProviderUrl.Replace("http://", string.Empty).Replace("https://", string.Empty);
                        string targetUrl = string.Empty;

                        if (!IdentityProviderUrl.EndsWith('/'))
                        {
                            targetUrl = IdentityProviderUrl + "/";
                        }
                        else
                        {
                            targetUrl = IdentityProviderUrl;
                        }

                        try
                        {
                            _client.DefaultRequestHeaders.Clear();
                            _client.DefaultRequestHeaders.Add("Host", domain);
                            _client.DefaultRequestHeaders.Add("authorization", "DPoP " + Access_Token);
                            _client.DefaultRequestHeaders.Add("DPoP", BuildJwtForContent("DELETE", targetUrl));

                            var result = await _client.DeleteAsync(docLocation);

                            DebugOut(result.StatusCode.ToString());
                        }
                        catch (Exception ex)
                        {
                            DebugOut(ex.ToString());
                            throw ex;
                        }
                    }
                }
            }
        }

        public async Task<string> GetRdfDocument(string folderName, string docName)
        {
            var uri = _folders.Get(folderName);

            if (uri is not null)
            {
                if (_identityProviderUrl is not null)
                {
                    if (_client is not null)
                    {                      
                        var docLocation = uri.OriginalString + docName;

                        try
                        {
                            _client.DefaultRequestHeaders.Clear();
                            _client.DefaultRequestHeaders.Add("accept", "text/turtle");
                            _client.DefaultRequestHeaders.Add("authorization", "DPoP " + Access_Token);
                            _client.DefaultRequestHeaders.Add("DPoP", BuildJwtForContent("GET", docLocation));
                            var response = await _client.GetAsync(docLocation);

                            DebugOut(response.StatusCode.ToString());

                            if (response.StatusCode == HttpStatusCode.OK)
                            {
                                var text = response.Content.ReadAsStringAsync().Result;
                                DebugOut(text);

                                return text;

                            }
                        }
                        catch (Exception ex)
                        {
                            DebugOut(ex.ToString());
                        }
                    }
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Creates a document in the specified folder at the pod with the specific name and content
        /// </summary>
        /// <param name="folderName">The name of the folder at the pod</param>
        /// <param name="docName">The name of the document at the pod</param>
        /// <param name="docContent">The content of the file, as a Turtle RDF document</param>
        /// <returns></returns>
        /// <remarks>Performs an HTTP POST</remarks>
        public async Task CreateRdfDocumentAsync(string folderName, string docName, string docContent)
        {
            var uri = _folders.Get(folderName);

            if (uri is not null)
            {
                if (_identityProviderUrl is not null)
                {
                    if (_client is not null)
                    {
                        // see the section "Creating Documents (Files)
                        // https://github.com/solid/solid-spec/blob/master/api-rest.md

                        /*
                        POST / HTTP/1.1
                        Host: example.org
                        Content-Type: text/turtle
                        Link: <http://www.w3.org/ns/ldp#Resource>; rel="type"
                        Slug: test

                        <> <http://purl.org/dc/terms/title> "This is a test file" .
                        */

                        // <> <http://purl.org/dc/terms/title> "This is a test file" .
                        // not sure if we should try to construct an rdf document here?
                        string content = docContent;

                        string contentHeader = @"<http://www.w3.org/ns/ldp#Resource>; rel=""type""";
                        string domain = IdentityProviderUrl.Replace("http://", string.Empty).Replace("https://", string.Empty);
                        string targetUrl = string.Empty;

                        if (!IdentityProviderUrl.EndsWith('/'))
                        {
                            targetUrl = IdentityProviderUrl + "/";
                        }
                        else
                        {
                            targetUrl = IdentityProviderUrl;
                        }

                        try
                        {
                            _client.DefaultRequestHeaders.Clear();
                            _client.DefaultRequestHeaders.Add("Host", domain);
                            _client.DefaultRequestHeaders.Add("Link", contentHeader);
                            _client.DefaultRequestHeaders.Add("Slug", docName);
                            _client.DefaultRequestHeaders.Add("authorization", "DPoP " + Access_Token);
                            _client.DefaultRequestHeaders.Add("DPoP", BuildJwtForContent("POST", uri.OriginalString));

                            var stringContent = new StringContent(content, Encoding.UTF8, "text/turtle");
                            var result = await _client.PostAsync(uri, stringContent);

                            DebugOut(result.StatusCode.ToString());
                        }
                        catch (Exception ex)
                        {
                            DebugOut(ex.ToString());
                            throw ex;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Checks for the existence of the specified folder at the pod. If it does not exist, it will create it and reload the internal collection
        /// </summary>
        /// <param name="folder">The folder to check for</param>
        /// <returns>The uri of the specified folder</returns>
        /// <remarks>This is verified via HTTP GET and if needed created via an HTTP POST</remarks>
        public async Task<Uri> GetOrCreateFolderAsync(string folder)
        {
            if (!_folders.Contains(folder))
            {
                // create the folder at the pod
                await CreateFolderAsync(folder);
                // then reload the folder collection
                await GetContainersAsync();
                // and then return back to the user
                return _folders.Get(folder);
            }
            else
            {
                return _folders.Get(folder);
            }

            return null;
        }

        /// <summary>
        /// Returns the user name if we have an access token (if we are logged in) or an empty string if we are not
        /// </summary>
        /// <returns>The user name if logged in, otherwise empty string</returns>
        /// <remarks>Action performed via HTTP GET</remarks>
        public string GetUserName()
        {
            if (IsLoggedIn)
            {
                IGraph g = new Graph();
                UriLoader.Load(g, new Uri(IdentityProviderUrl + "/profile/card#me"));

                var triples = g.Triples;
                foreach (var triple in triples)
                {
                    if (triple.Predicate.NodeType == NodeType.Uri)
                    {
                        var uriNode = triple.Predicate as UriNode;
                        if (uriNode.Uri.Fragment.Contains("#fn"))
                        {
                            if (triple.Object.NodeType == NodeType.Literal)
                            {
                                var literal = triple.Object as ILiteralNode;
                                return literal.Value;
                            }
                        }
                    }
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Determines if we should write to the debug console (in the IDE) and to the Console window
        /// </summary>
        /// <param name="useDebug"></param>
        public void SetDebug(bool useDebug)
        {
            _useDebug = useDebug;
        }

        /// <summary>
        /// Attempts to get our access and id tokens from the Identity Provider
        /// </summary>
        /// <param name="appCode">The app code given to us by the Identity Provider (provided usually after dynamic client registration)</param>
        /// <param name="issuerUrl">The issuer url (usually ourselves)</param>
        /// <param name="audienceUrl">The audience url (usually ourselves)</param>
        /// <returns></returns>
        public async Task GetAccessAndIdTokensAsync(string appCode, string issuerUrl, string audienceUrl)
        {
            _clientAppCode = appCode;
            await GetAccessAndIdTokensAsync(issuerUrl, audienceUrl);
        }

        /// <summary>
        /// Attempts to get our access and id tokens from the Identity Provider
        /// </summary>
        /// <param name="issuerUrl">The issuer url (usually ourselves)</param>
        /// <param name="audienceUrl">The audience url (usually ourselves)</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        /// <remarks>This action is done via HTTP POST</remarks>
        public async Task GetAccessAndIdTokensAsync(string issuerUrl, string audienceUrl)
        {
            string url = _endpointInfo.token_endpoint;
            string jwtToken = BuildJwtForLogin(issuerUrl, audienceUrl, _endpointInfo.token_endpoint);
            var tokenHandler = new JwtSecurityTokenHandler();

            // we don't actually need to do this, this is just a sanity check
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
                Address = _endpointInfo.token_endpoint,
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

            await GetContainersAsync();
        }

        /// <summary>
        /// Constructs the url to login the user, saves the code verifier challenge internally
        /// </summary>
        /// <param name="redirectUrl">The url we want the Identity Provider to redirect the user back to after logging in</param>
        /// <returns></returns>
        public async Task<string> GetLoginUrlAsync(string redirectUrl)
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

            // IdentityMode.OIDCClient generates a code challenge and verifier for us
            // this is step 4 of here https://solid.github.io/solid-oidc/primer/#authorization-code-pkce-flow
            // we will need the Code Verifier later, so let's save it
            _clientCodeVerifier = state.CodeVerifier;

            url += "&prompt=consent&response_mode=query";

            return url;
        }

        /// <summary>
        /// Registers our app via Dynamic Client Registration
        /// </summary>
        /// <param name="identityProvider">The identity provider url (usually Community Solid Server)</param>
        /// <param name="redirectUris">An array of uris that we accept for redirection after logging in</param>
        /// <param name="appName">A name for our application</param>
        /// <returns></returns>
        public async Task RegisterAppAsync(string identityProvider, string[] redirectUris, string appName)
        {
            _identityProviderUrl = identityProvider;
            await GetConfigurationAsync();
            await RegisterAppAsync(redirectUris, appName);
        }

        /// <summary>
        /// Registers our app via Dynamic Client Registration. Defaults the identity provider url to one that was identified automatically
        /// </summary>
        /// <param name="redirectUris">An array of uris that we accept for redirection after logging in</param>
        /// <param name="appName">A name for our application</param>
        /// <returns></returns>
        /// <remarks>This action is performed via HTTP POST</remarks>
        public async Task RegisterAppAsync(string[] redirectUris, string appName)
        {
            if (string.IsNullOrEmpty(_appName))
            {
                _appName = appName;
            }

            _redirectUris = redirectUris;

            string url = _endpointInfo.registration_endpoint;

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
            contentBuilder.Append($@"""client_name"": ""{appName}"",");
            contentBuilder.Append($@"""scopes"": ""{_appScopes}""");
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

        /// <summary>
        /// Sets the identity provider for the client, usually the Community Solid Server
        /// </summary>
        /// <param name="identityProvider"></param>
        /// <returns></returns>
        public async Task SetIdentityProviderAsync(string identityProvider)
        {
            _identityProviderUrl = identityProvider;
            await GetConfigurationAsync();
        }

        /// <summary>
        /// Sets the name of our app
        /// </summary>
        /// <param name="appName"></param>
        public void SetAppName(string appName)
        {
            _appName = appName;
        }

        /// <summary>
        /// Sets the app code (usually provided by the Identity Provider after Dynamic Client Registration)
        /// </summary>
        /// <param name="appCode"></param>
        public void SetAppCode(string appCode)
        {
            _clientAppCode = appCode;
        }

        /// <summary>
        /// Generates an RSA public/private set of keys
        /// </summary>
        public void GenerateKeys()
        {
            var rsa = RSA.Create();

            _publicKey = rsa.ExportParameters(false);
            _privateKey = rsa.ExportParameters(true);

            _privateRSAKey = new RsaSecurityKey(_privateKey);
            _publicRSAKey = new RsaSecurityKey(_publicKey);

            _hasGeneratedKeys = true;
        }

        /// <summary>
        /// Returns the generated RSA private key as a Json Web Key (JWK)
        /// </summary>
        /// <returns></returns>
        public JsonWebKey GetPrivateJsonWebKey()
        {
            if (!HasGeneratedKeys)
            {
                GenerateKeys();
            }

            return JsonWebKeyConverter.ConvertFromRSASecurityKey(_privateRSAKey);
        }

        /// <summary>
        /// Returns the generated RSA public key as a Json Web Key (JWK)
        /// </summary>
        /// <returns></returns>
        public JsonWebKey GetPublicJsonWebKey()
        {
            if (!HasGeneratedKeys)
            {
                GenerateKeys();
            }

            var key = JsonWebKeyConverter.ConvertFromRSASecurityKey(_publicRSAKey);

            // not sure why, but Solid wants the alg property...
            // spent all day fighting this...
            key.AdditionalData.Add("alg", "RS256");

            return key;
        }

        /// <summary>
        /// Builds a JWT (Json Web Token) with our generated RSA public key in the header, signed by our private key
        /// for the action/content we need
        /// </summary>
        /// <param name="httpMethod">The HTTP method we are using the token for</param>
        /// <param name="resourceUri">The resource we are accessing</param>
        /// <returns>A JWT for the action/object we need</returns>
        public string BuildJwtForContent(string httpMethod, string resourceUri)
        {
            if (!HasGeneratedKeys)
            {
                GenerateKeys();
            }

            // a secret key that we know
            var key = new RsaSecurityKey(_privateKey);

            // how this token is generated
            var creds = new SigningCredentials(key, SecurityAlgorithms.RsaSha256);

            // identify the type of crypto for our keys
            var algMap = new Dictionary<string, string>();
            algMap.Add("alg", "RS256");

            // https://solid.github.io/solid-oidc/primer/#authorization-code-pkce-flow
            // per the above, we need to send as "dpop+jwt"
            // we'll add this in later, but for now, add our signing creds
            var header = new JwtHeader(creds);

            // Microsoft's API automatically adds this as "jwt",
            // but we need to set it to "dpop+jwt",
            // so let's remove it and add it back later
            header.Remove("typ");

            // per the above link, send the public key in the header
            var jwk = GetPublicJsonWebKey();
            header.Add("jwk", jwk);
            header.Add("typ", "dpop+jwt");

            // add the inital claims
            var payload = new JwtPayload(new List<Claim>());

            // we want to only use this token at the specified location
            payload.AddClaim(new Claim("htu", resourceUri));

            // for the specific method
            payload.AddClaim(new Claim("htm", httpMethod));

            // unique identifier for the token
            payload.AddClaim(new Claim("jti", Guid.NewGuid().ToString()));

            // we rebuild the token with all the additional headers, claims, etc.
            var dpopToken = new JwtSecurityToken(header, payload);

            // the date when the token was issued, must be an integer (not a string)
            // this is required
            dpopToken.Payload.AddClaim(new Claim("iat", ComputeCurrentIat().ToString(), ClaimValueTypes.Integer));

            var text = new JwtSecurityTokenHandler().WriteToken(dpopToken);

            // we can validate the text at https://jwt.io/ if we want
            return text;
        }

        /// <summary>
        /// Builds a JWT (Json Web Token) signed with our generated RSA keys to authenticate ourselves to our IdP
        /// </summary>
        /// <param name="issuerUrl">The issuer of this token (usually ourselves, i.e. 'http://localhost:7030')</param>
        /// <param name="audienceUrl">Who this token is for (usually ourselves, i.e. 'http://localhost:7030')</param>
        /// <param name="authorizationUrl">Who is approving this token (usually the IdP, 
        /// in this case, Community Solid Server at 'http://localhost:3000')</param>
        /// <returns>A JWT for the specified parameters</returns>
        public string BuildJwtForLogin(string issuerUrl, string audienceUrl, string authorizationUrl)
        {
            if (!HasGeneratedKeys)
            {
                GenerateKeys();
            }

            // stolen from the internet to compute iat, exp values

            //var exp = (int)issueTime.AddMinutes(55).Subtract(utc0).TotalSeconds;

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
            dpopToken.Payload.AddClaim(new Claim("iat", ComputeCurrentIat().ToString(), ClaimValueTypes.Integer));

            // debugging
            var text = new JwtSecurityTokenHandler().WriteToken(dpopToken);

            // we can validate the text at https://jwt.io/ if we want
            return text;
        }

        /// <summary>
        /// Gets the contents of the container (folder) and checks to see if the specified file exists in it
        /// </summary>
        /// <param name="containerName">The name of container/folder</param>
        /// <param name="fileName">The name of the file in the container</param>
        /// <returns><c>TRUE</c> if the file exists in the container, otherwise <c>FALSE</c></returns>
        /// <remarks>Executed via an HTTP GET</remarks>
        public async Task<bool> ContainerHasFile(string containerName, string fileName)
        {
            var result = await GetContentsOfContainer(containerName);
            return result.Any(r => r.AbsoluteUri.Contains(fileName));
        }

        /// <summary>
        /// Returns a list of URIs of the specified container (folder) name at the pod
        /// </summary>
        /// <param name="containerName">The name of the container/folder</param>
        /// <returns></returns>
        /// <remarks>Executed via an HTTP GET</remarks>
        public async Task<List<Uri>> GetContentsOfContainer(string containerName)
        {
            string uri = string.Empty;

            if (!containerName.EndsWith('/'))
            {
                containerName = containerName + "/";
            }

            if (!containerName.StartsWith('/'))
            {
                containerName = "/" + containerName;
            }

            if (!IdentityProviderUrl.EndsWith("/"))
            {
                uri = IdentityProviderUrl + "/" + containerName;
            }

            uri = IdentityProviderUrl + containerName;

            if (!string.IsNullOrEmpty(IdentityProviderUrl))
            {
                if (_client is not null)
                {
                    try
                    {
                        _client.DefaultRequestHeaders.Clear();
                        _client.DefaultRequestHeaders.Add("accept", "text/turtle");
                        _client.DefaultRequestHeaders.Add("authorization", "DPoP " + Access_Token);
                        _client.DefaultRequestHeaders.Add("DPoP", BuildJwtForContent("GET", uri));
                        var response = await _client.GetAsync(uri);

                        DebugOut(response.StatusCode.ToString());

                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            var text = response.Content.ReadAsStringAsync().Result;
                            DebugOut(text);

                            var contents = GetSubjectUris(text);
                            return contents;

                        }
                    }
                    catch (Exception ex)
                    {
                        DebugOut(ex.ToString());
                    }

                }
            }

            return null;
        }
        #endregion

        #region Private Methods

        /// <summary>
        /// Creates a container (folder) at the pod via HTTP POST
        /// </summary>
        /// <param name="folder">The name of the folder</param>
        /// <returns></returns>
        private async Task CreateFolderAsync(string folder)
        {
            if (_identityProviderUrl is not null)
            {
                if (_client is not null)
                {
                    // see the section "Creating Containers (Directories)"
                    // https://github.com/solid/solid-spec/blob/master/api-rest.md

                    /*
                    POST / HTTP/1.1
                    Host: example.org
                    Content-Type: text/turtle
                    Link: <http://www.w3.org/ns/ldp#BasicContainer>; rel="type"
                    Slug: data
    
                    <> <http://purl.org/dc/terms/title> "Basic container" .
                    */

                    string content = @"<> <http://purl.org/dc/terms/title> ""Basic container"" .";
                    string containerHeader = @"<http://www.w3.org/ns/ldp#BasicContainer>; rel=""type""";
                    string domain = IdentityProviderUrl.Replace("http://", string.Empty).Replace("https://", string.Empty);
                    string targetUrl = string.Empty;

                    if (!IdentityProviderUrl.EndsWith('/'))
                    {
                        targetUrl = IdentityProviderUrl + "/";
                    }
                    else
                    {
                        targetUrl = IdentityProviderUrl;
                    }

                    try
                    {
                        _client.DefaultRequestHeaders.Clear();
                        _client.DefaultRequestHeaders.Add("Host", domain);
                        _client.DefaultRequestHeaders.Add("Link", containerHeader);
                        _client.DefaultRequestHeaders.Add("Slug", folder);
                        _client.DefaultRequestHeaders.Add("authorization", "DPoP " + Access_Token);
                        _client.DefaultRequestHeaders.Add("DPoP", BuildJwtForContent("POST", targetUrl));

                        var stringContent = new StringContent(content, Encoding.UTF8, "text/turtle");
                        var result = await _client.PostAsync(IdentityProviderUrl, stringContent);

                        DebugOut(result.StatusCode.ToString());
                    }
                    catch (Exception ex)
                    {
                        DebugOut(ex.ToString());
                        throw ex;
                    }
                }
            }
        }



        /// <summary>
        /// Gets the contents of the pod via HTTP GET
        /// </summary>
        /// <returns></returns>
        private async Task GetContainersAsync()
        {
            if (_folders is null)
            {
                _folders = new UriCollection();
            }

            _folders.Clear();

            if (!string.IsNullOrEmpty(IdentityProviderUrl))
            {
                if (_client is not null)
                {
                    _client.DefaultRequestHeaders.Clear();
                    var response = await _client.GetAsync(_identityProviderUrl);
                    var text = response.Content.ReadAsStringAsync().Result;

                    var folders = GetSubjectUris(text);
                    foreach (var folder in folders)
                    {
                        DebugOut($"Discovered item at server: {folder.AbsolutePath}");
                    }

                    _folders.AddRange(folders);
                }
            }
        }

        private void DebugOut(string item)
        {
            if (_useDebug)
            {
                Console.WriteLine(item);
                Debug.WriteLine(item);
            }
        }

        /// <summary>
        /// Gets the configuration of the pod via HTTP GET
        /// </summary>
        /// <returns></returns>
        private async Task GetConfigurationAsync()
        {
            if (!string.IsNullOrEmpty(_identityProviderUrl))
            {
                if (_client is not null)
                {
                    var response = _client.GetAsync(_identityProviderUrl + "/.well-known/openid-configuration");
                    var data = await response.Result.Content.ReadAsStringAsync();
                    var item = JsonConvert.DeserializeObject<EndpointInfo>(data);
                    if (item is not null)
                    {
                        _endpointInfo = item;
                    }
                }
            }
        }

        private double ComputeCurrentIat()
        {
            var utc0 = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            var issueTime = DateTime.UtcNow;

            return (int)issueTime.Subtract(utc0).TotalSeconds;
        }

        private List<Uri> GetSubjectUris(string text)
        {
            var result = new List<Uri>();

            var parser = new TurtleParser();
            var g = new Graph();
            g.BaseUri = new Uri(IdentityProviderUrl);
            var reader = new StringReader(text);
            parser.Load(g, reader);
            var triples = g.Triples;
            var subjects = triples.SubjectNodes;
            foreach (var subject in subjects)
            {
                if (subject.NodeType == NodeType.Uri)
                {
                    var item = subject as UriNode;
                    if (!string.IsNullOrEmpty(item.Uri.AbsolutePath))
                    {
                        result.Add(item.Uri);
                    }
                }
            }

            return result;
        }
        #endregion

    }
}
