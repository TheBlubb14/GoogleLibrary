using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Responses;
using System.Collections.Generic;
using System.Threading;

namespace GoogleLibrary.User
{
    public class UserManager
    {
        /// <summary>
        /// client secrets
        /// </summary>
        public ClientSecrets ClientSecrets { get; set; }

        /// <summary>
        /// scopes
        /// </summary>
        public List<string> Scope { get; set; }

        /// <summary>
        /// authenticate when refresh token expires
        /// </summary>
        public bool StayAuthenticated { get; set; }

        /// <summary>
        /// retry time in seconds if authentication fails 
        /// </summary>
        public int RetryIn { get; set; }

        /// <summary>
        /// name to regonize in google developer console
        /// </summary>
        public string User { get; set; }

        /// <summary>
        /// enpoint for used google api
        /// </summary>
        public string Endpoint { get; set; }

        /// <summary>
        /// folder name(not the path) where auth token will be stored
        /// </summary>
        public string DataStoreFolder { get; set; }

        /// <summary>
        /// UserCredentials Token
        /// with which you can authenticate with other google apis
        /// </summary>

        public TokenResponse UserToken => _userCredential?.Token;


        private CancellationToken _cancellationToken;
        private System.Timers.Timer _refreshTimer;
        private UserCredential _userCredential;

        public UserManager()
        {

        }
    }
}
