# GoogleLibrary
Some random methods for the Google APIs Client Library

#### Example UserManager
```csharp
var clientSecrets = new ClientSecrets()
{
    ClientId = "yourclientid",
    ClientSecret = "yourclientsecret"
};

var scope = new List<string>()
{
    // example scopes for the google assistant
    "openid",
    "https://www.googleapis.com/auth/assistant-sdk-prototype"
};

// example endpoint for the google assistant
string endpoint = "embeddedassistant.googleapis.com";
await UserManager.Initialize(clientSecrets, "GoogleLibrary", endpoint, scope, "GoogleLibraryFolder");

// here you have your AccessToken
// with this Token you can authenticate with the google services
var accessToken = UserManager.Instance.UserToken.AccessToken;

// i implemented for example a method to get your google user data with the AccessToken
var userData = await UserManager.Instance.GetUserData(accessToken);
```

You can create your own ClientSecrets [here](https://console.cloud.google.com/apis/credentials)

### Troubleshooting
If there is a System.Reflection.TargetInvocationException try to update all NuGet Packages





##### Thanks to [this project](https://github.com/ac87/GoogleAssistantWindows) for the example to get started with google apis
