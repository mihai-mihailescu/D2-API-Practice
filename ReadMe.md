//Using the D2 Bungie API

/*
For this we will be using C# in Visual Studio 2019. To use the Bungie.net API we need to register our would be app with bungie. This can be done
https://www.bungie.net/en/Application
After this we will be given an API which is the UNIQUE identifier our application will use. Keep this key hidden.

We can now try to make a simple HTTP GET request to get the ID of a desired user. Since the API returns JSON objects, a simple understanding of JSON objects was needed.

https://www.json.org/json-en.html

To make an HTTP GET Request we first initialize a HttpClient(). 
"var" is used so as to not have to worry about the data type of client.
"using" in c# is used when we perform asynchronous operations within in a function. At the end of the code block it also ensures this instance of client is properly disposed of.
*/
public static string initialize(String username)
    {
        using var client = new HttpClient();
        {
			client.BaseAddress = new Uri("https://www.bungie.net/platform/");
			client.DefaultRequestHeaders.Add("X-API-Key", API_Key);

            var responseTask = client.GetAsync("Destiny2/SearchDestinyPlayer/-1/" + username).Result;   
            var content = responseTask.Content.ReadAsStringAsync().Result;

            dynamic item = Newtonsoft.Json.JsonConvert.DeserializeObject(content);

            return item.Response[0].membershipId;
        }
    }

/*
This is a simple GET request to get basic information about a Destiny 2 player. For reference we will be using
https://bungie-net.github.io/multi/operation_get_Destiny2-SearchDestinyPlayer.html#operation_get_Destiny2-SearchDestinyPlayer
as a guide.

client.BaseAddress = new Uri("https://www.bungie.net/platform/");
Sets the root path for our client to communicate with the Bungie.net API.

client.DefaultRequestHeaders.Add("X-API-Key", API_Key);
Adds a header to our Http request. In this context, the variable API_Key is declared as a global constant String with the API key that was provided when we registered our application.

var responseTask = client.GetAsync("Destiny2/SearchDestinyPlayer/-1/" + username).Result;   
This is the Http request.
**Very important to note here is the structure of the String.
GetAsync("Destiny2/SearchDestinyPlayer/-1/" + username)
It cannot have any preceding '/' likewise it must end with a '/' if  we want to provide any username. The Http request now looks like

GET https://www.bungie.net/platform/Destiny2/SearchDestinyPlayer/-1/medulex
X-API-Key ....

Looking at the API documentation we see that the GET Destiny2.SearchDestinyPlayer must have the following structure:

/Destiny2/SearchDestinyPlayer/{membershipType}/{displayName}/
Our structure matches this template. In our request membershipType is -1. This is an enum that is defined here:
https://bungie-net.github.io/multi/schema_BungieMembershipType.html#schema_BungieMembershipType

None: 0
TigerXbox: 1
TigerPsn: 2
TigerSteam: 3
TigerBlizzard: 4
TigerStadia: 5
TigerDemon: 10
BungieNext: 254
All: -1

From this we should expect the Request to respond with All types of accounts that a user has along with different membershipId's.

var content = responseTask.Content.ReadAsStringAsync().Result;
dynamic item = Newtonsoft.Json.JsonConvert.DeserializeObject(content);
"dynamic" Since at compile-time "content" will not have any data, dynamic allows us to "bypass" any compile time errors, and treat "item" like a dynamically typed variable. For more information:
https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/types/using-type-dynamic

These two lines allow the returned JSON object to be made readable by us. The returned jSON object will look like:

{
    "Response": [
        {
            "iconPath": "/img/theme/bungienet/icons/psnLogo.png",
            "crossSaveOverride": 0,
            "isPublic": false,
            "membershipType": 2,
            "membershipId": "4611686018442623539",
            "displayName": "Medulex"
        }
    ],
    "ErrorCode": 1,
    "ThrottleSeconds": 0,
    "ErrorStatus": "Success",
    "Message": "Ok",
    "MessageData": {}
}
*To display this response I am using Postman to make HTTP requests.
**https://www.postman.com/

We notice that the Response field is an array of objects, specifically DeserializeObject will return an IList<string> for arrays within the JSON object. This allows us to use Properties and methods of IList, such as IList.Count. 
Treating the Response member as an array, we can do Response[0].membershipId to get the desired Id. To prove this works for users with multiple account types, we can look at my friend's, camcam1k, account. From our code, we simply call initialize("camcam1k"). I will note here that this function will return only the membershipId of the first account camcam1k has BUT simply looping through the Response array will give us the desired results. The request

GET https://www.bungie.net/platform/Destiny2/SearchDestinyPlayer/-1/camcam1k
will respond with

{
    "Response": [
        {
            "iconPath": "/img/theme/bungienet/icons/xboxLiveLogo.png",
            "crossSaveOverride": 0,
            "isPublic": false,
            "membershipType": 1,
            "membershipId": "4611686018455291474",
            "displayName": "camcam1k"
        },
        {
            "iconPath": "/img/theme/bungienet/icons/psnLogo.png",
            "crossSaveOverride": 0,
            "isPublic": false,
            "membershipType": 2,
            "membershipId": "4611686018460596103",
            "displayName": "camcam1k"
        }
    ],
    "ErrorCode": 1,
    "ThrottleSeconds": 0,
    "ErrorStatus": "Success",
    "Message": "Ok",
    "MessageData": {}
}

We see that camcam1k has an XBox account and a PSN account that are linked to his Bungie.net account.

If we alter the request to get just the XBox account, we alter the membershipId to 1 instead of -1

GET https://www.bungie.net/platform/Destiny2/SearchDestinyPlayer/1/camcam1k
will respond with

{
    "Response": [
        {
            "iconPath": "/img/theme/bungienet/icons/xboxLiveLogo.png",
            "crossSaveOverride": 0,
            "isPublic": false,
            "membershipType": 1,
            "membershipId": "4611686018455291474",
            "displayName": "camcam1k"
        }
    ],
    "ErrorCode": 1,
    "ThrottleSeconds": 0,
    "ErrorStatus": "Success",
    "Message": "Ok",
    "MessageData": {}
}

From this we can see that it becomes trivial both lookup any user on any desired platform. 

Changing: var responseTask = client.GetAsync("Destiny2/SearchDestinyPlayer/-1/" + username).Result;
To:    	  var responseTask = client.GetAsync("Destiny2/SearchDestinyPlayer/" + accountType + "/" + username).Result;   
Allows for variable accountType more flexibility when searching.

To push this search further, we must also be able to handle the event that we cannot find a user with this search. We will look for a random user that doesn't exist: asd098f40s9dafj
We confirm by performing a query at bungie.net:
https://www.bungie.net/en/Search?query=asd098f40s9dafj&type=5


GET https://www.bungie.net/Platform/Destiny2/SearchDestinyPlayer/-1/asd098f40s9dafj/
will respond with

{
    "Response": [],
    "ErrorCode": 1,
    "ThrottleSeconds": 0,
    "ErrorStatus": "Success",
    "Message": "Ok",
    "MessageData": {}
}

Since the request we made was totally valid, it succeeded but Response came back empty. Thus if we consistently make VALID requests, we can expect the request to succeed but an empty reponse will mean either the user doesn't exist or they don't have an account of the specified type linked to their Bungie.net. We must also be careful because this assumption can only be made when membershipType is -1 in our request. This can be visualized by the follwing request:

GET https://www.bungie.net/Platform/Destiny2/SearchDestinyPlayer/3/camcam1k/
will respond with

{
    "Response": [],
    "ErrorCode": 1,
    "ThrottleSeconds": 0,
    "ErrorStatus": "Success",
    "Message": "Ok",
    "MessageData": {}
}

We know from our previous requests that camcam1k has XBox and PSN accounts but does not have a [3] Steam account meaning Response will be empty. Since we established that the Response array is in fact an IList doing:

item.Response.Count == 0 will return true when the response array is empty.
