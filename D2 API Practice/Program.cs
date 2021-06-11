using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Collections.Specialized;


namespace D2_API_Practice
{
    class Program
    {
        public class Profile
        {
            public string destinyMembershipId;
            public string membershipType;
            public Profile(dynamic id, dynamic type)
            {
                destinyMembershipId = id;
                membershipType = type;
            }

            public Profile()
            {
                destinyMembershipId = "-1";
                membershipType = "-1";
            }
        }



        const String API_Key = "d336c093b0e64bd69ce14481ca7a6e12";

        /*initialize()
         * Input: username, accountType: default=-1
         * Return: membershipId of the first found from the username
         */
        public static Profile initialize(String username, String accountType = "-1")
        {
            using var client = new HttpClient();
            {
                client.BaseAddress = new Uri("https://www.bungie.net/platform/");
                client.DefaultRequestHeaders.Add("X-API-Key", API_Key);

                var responseTask = client.GetAsync("Destiny2/SearchDestinyPlayer/" + accountType + "/" + username).Result;   
                var content = responseTask.Content.ReadAsStringAsync().Result;

                dynamic item = Newtonsoft.Json.JsonConvert.DeserializeObject(content);

                if (item.Response.Count != 0)
                    return new Profile(item.Response[0].membershipId, item.Response[0].membershipType);
                else
                    return new Profile();
            }
        }

        

        static void Main(string[] args)
        {
            //Console.WriteLine("Enter Profile ID: ");
            String userID = "medulex";  //Change to Profile request
                                        //String accountType = "1";
                                        //Console.WriteLine(initialize(userID,accountType));

            Profile user = initialize(userID);
            if(user.destinyMembershipId == "-1")
            {
                Console.WriteLine("No Profile found for the specified Profile name AND account type.");
                return;
            }

            
        }
    }
}

