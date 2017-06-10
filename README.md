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

var accessToken = UserManager.Instance.UserToken.AccessToken;
var userData = await UserManager.Instance.GetUserData(accessToken);
```

You can create your own ClientSecrets [here](https://console.cloud.google.com/apis/credentials)


##### Thanks to [this project](https://github.com/ac87/GoogleAssistantWindows) for the example to get started with google apis
