using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolidDotNetClient
{
    internal class RegisterData
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
}
