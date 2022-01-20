namespace SolidDotNet
{
    /// <summary>
    /// A generated class representing the Json sent back from the Identity Provider after dynamic client registration
    /// </summary>
    public class RegisterData
    {

        public string application_type { get; set; }
        public IList<string> grant_types { get; set; }
        public string id_token_signed_response_alg { get; set; }
        public IList<object> post_logout_redirect_uris { get; set; }
        public bool require_auth_time { get; set; }
        public IList<string> response_types { get; set; }
        public string subject_type { get; set; }
        public string token_endpoint_auth_method { get; set; }
        public string introspection_endpoint_auth_method { get; set; }
        public string revocation_endpoint_auth_method { get; set; }
        public bool require_signed_request_object { get; set; }
        public IList<object> request_uris { get; set; }
        public int client_id_issued_at { get; set; }
        public string client_id { get; set; }
        public string client_name { get; set; }
        public int client_secret_expires_at { get; set; }
        public string client_secret { get; set; }
        public IList<string> redirect_uris { get; set; }
        public string registration_client_uri { get; set; }
        public string registration_access_token { get; set; }

    }

    public class EndpointInfo
    {
        public string authorization_endpoint { get; set; }
        public bool claims_parameter_supported { get; set; }
        public List<string> claims_supported { get; set; }
        public List<string> code_challenge_methods_supported { get; set; }
        public string end_session_endpoint { get; set; }
        public List<string> grant_types_supported { get; set; }
        public List<string> id_token_signing_alg_values_supported { get; set; }
        public string issuer { get; set; }
        public string jwks_uri { get; set; }
        public string registration_endpoint { get; set; }
        public List<string> response_modes_supported { get; set; }
        public List<string> response_types_supported { get; set; }
        public List<string> scopes_supported { get; set; }
        public List<string> subject_types_supported { get; set; }
        public List<string> token_endpoint_auth_methods_supported { get; set; }
        public List<string> token_endpoint_auth_signing_alg_values_supported { get; set; }
        public string token_endpoint { get; set; }
        public List<string> request_object_signing_alg_values_supported { get; set; }
        public bool request_parameter_supported { get; set; }
        public bool request_uri_parameter_supported { get; set; }
        public bool require_request_uri_registration { get; set; }
        public string userinfo_endpoint { get; set; }
        public List<string> userinfo_signing_alg_values_supported { get; set; }
        public string introspection_endpoint { get; set; }
        public List<string> introspection_endpoint_auth_methods_supported { get; set; }
        public List<string> introspection_endpoint_auth_signing_alg_values_supported { get; set; }
        public List<string> dpop_signing_alg_values_supported { get; set; }
        public string revocation_endpoint { get; set; }
        public List<string> revocation_endpoint_auth_methods_supported { get; set; }
        public List<string> revocation_endpoint_auth_signing_alg_values_supported { get; set; }
        public List<string> claim_types_supported { get; set; }
        public string solid_oidc_supported { get; set; }
    }

}
