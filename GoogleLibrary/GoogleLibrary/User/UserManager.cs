using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Util.Store;
using Grpc.Auth;
using Grpc.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

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
        /// authenticate when auth token expires
        /// </summary>
        public bool StayAuthenticated { get; set; }

        /// <summary>
        /// time to wait in seconds after next attempt to get auth token
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
        public string DataStoreFolder
        {
            get => _dataStoreFolder;
            set
            {
                _fileDataStore = new FileDataStore(value);
                _dataStoreFolder = value;
            }
        }

        private string _dataStoreFolder;

        /// <summary>
        /// UserCredentials Token
        /// with which you can authenticate with other google apis
        /// </summary>
        public TokenResponse UserToken => UserCredential?.Token;

        /// <summary>
        /// ChannelCredentials
        /// </summary>
        public ChannelCredentials ChannelCredential => UserCredential?.ToChannelCredentials();

        public static UserManager Instance;

        private CancellationToken _cancellationToken;
        private System.Timers.Timer _refreshTimer;
        public UserCredential UserCredential;
        private FileDataStore _fileDataStore;

        #region Timer
        private void StartRefreshTimer(TimeSpan time)
        {
            _refreshTimer = new System.Timers.Timer(time.TotalMilliseconds);
            _refreshTimer.Elapsed += this._refreshTimer_Elapsed;
            _refreshTimer.Start();
        }

        private async void _refreshTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            _refreshTimer.Dispose();
            await GetOrRefreshCredential();
        }
        #endregion

        /// <summary>
        /// get the client secrets from a json file
        /// </summary>
        /// <param name="Path">json file path</param>
        /// <returns>client secrets</returns>
        public static ClientSecrets GetClientSecretsFromFile(string Path)
        {
            using var stream = new FileStream(Path, FileMode.Open, FileAccess.Read);
            return GoogleClientSecrets.Load(stream).Secrets;
        }

        /// <summary>
        /// new instance of <cref="UserManager">
        /// </summary>
        /// <param name="ClientSecrets">client secrets</param>
        /// <param name="User">application name</param>
        /// <param name="Endpoint">endpoint provided by the api</param>
        /// <param name="Scope">scope</param>
        /// <param name="DataStoreFolder">foldername where auth token will be stored</param>
        /// <param name="StayAuthenticated">authenticate when auth token expires</param>
        /// <param name="RetryIn">time to wait in seconds after next attempt to get auth token</param>
        public async static Task<UserManager> Initialize(ClientSecrets ClientSecrets, string User, string Endpoint, List<string> Scope, string DataStoreFolder, bool StayAuthenticated = true, int RetryIn = 30)
        {
            // TODO: better solution to check for invalid input?
            ClientSecrets.ClientId.CheckNull(nameof(ClientSecrets.ClientId));
            ClientSecrets.ClientSecret.CheckNull(nameof(ClientSecrets.ClientSecret));
            Endpoint.CheckNull(nameof(Endpoint));
            Scope.CheckNull(nameof(Scope));
            User.CheckNull(nameof(User));
            DataStoreFolder.CheckNull(nameof(DataStoreFolder));

            Instance = new UserManager(ClientSecrets, User, Endpoint, Scope, DataStoreFolder, StayAuthenticated, RetryIn);
            await Instance.GetOrRefreshCredential();
            return Instance;
        }


        /// <summary>
        /// new instance of <cref="UserManager">
        /// </summary>
        /// <param name="ClientSecrets">client secrets</param>
        /// <param name="User">application name</param>
        /// <param name="Endpoint">endpoint provided by the api</param>
        /// <param name="Scope">scope</param>
        /// <param name="DataStoreFolder">foldername where auth token will be stored</param>
        /// <param name="StayAuthenticated">authenticate when auth token expires</param>
        /// <param name="RetryIn">time to wait in seconds after next attempt to get auth token</param>
        private UserManager(ClientSecrets ClientSecrets, string User, string Endpoint, List<string> Scope, string DataStoreFolder, bool StayAuthenticated = true, int RetryIn = 30)
        {
            this.ClientSecrets = ClientSecrets;
            this.User = User;
            this.Endpoint = Endpoint;
            this.Scope = Scope;
            this.DataStoreFolder = DataStoreFolder;
            this.StayAuthenticated = StayAuthenticated;
            this.RetryIn = RetryIn;
        }

        private async Task GetOrRefreshCredential()
        {
            if (UserCredential == null)
                UserCredential = await GoogleWebAuthorizationBroker.AuthorizeAsync(ClientSecrets, Scope, User, _cancellationToken, _fileDataStore);

            await UserCredential.RefreshTokenAsync(_cancellationToken);
            StartRefreshTimer(TimeSpan.FromSeconds(UserCredential.Token.ExpiresInSeconds ?? RetryIn));
        }

        /// <summary>
        /// get the avaible google user data
        /// </summary>
        /// <param name="AccessToken">the access token in the auth token</param>
        /// <param name="SaveOnDisc">save it in the application folder as a json file</param>
        /// <returns>user data</returns>
        public async Task<GoogleUserData> GetUserData(string AccessToken, bool SaveOnDisc = false)
        {
            GoogleUserData userData = null;

            try
            {
                using var client = new HttpClient();
                // TODO: do i need it?
                client.CancelPendingRequests();

                var profileUrl = "https://www.googleapis.com/oauth2/v1/userinfo?access_token=" + AccessToken;
                var output = await client.GetAsync(profileUrl);

                if (output.IsSuccessStatusCode)
                {
                    var result = await output.Content.ReadAsStringAsync();
                    userData = JsonConvert.DeserializeObject<GoogleUserData>(result);

                    if (SaveOnDisc && userData != null)
                        File.WriteAllText(Path.Combine(_fileDataStore.FolderPath, "GoogleUserData.json"), result);
                }
                else
                {
                    userData = new GoogleUserData()
                    {
                        id = "-1",
                        family_name = $"status code: {output.StatusCode}",
                        name = output.Content?.ReadAsStringAsync().Result,
                    };
                }
            }
            catch (Exception ex)
            {
                userData = new GoogleUserData()
                {
                    id = "-1",
                    name = ex.ToString()
                };
            }

            return userData;
        }

        /// <summary>
        /// logout and revoke authtoken
        /// </summary>
        public void Logout()
        {
            _cancellationToken = new CancellationToken(true);
            _refreshTimer.Dispose();
            UserCredential?.RevokeTokenAsync(CancellationToken.None).Wait();
            UserCredential = null;
        }
    }
}
