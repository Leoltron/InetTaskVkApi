using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Newtonsoft.Json;

namespace InetTaskVkApi
{
    public static class Program
    {
        private const string ApiKey = "GetYourOwnKey";

        public static void Main()
        {
            while (true)
            {
                Console.Write(">");
                var commandLine = Console.ReadLine();
                if (string.IsNullOrEmpty(commandLine))
                    continue;
                var splittedCommandLine = commandLine.Split(new[] {' '}, 2);
                var command = splittedCommandLine[0].ToLowerInvariant();
                switch (command)
                {
                    case "friends" when splittedCommandLine.Length == 1:
                        Console.WriteLine("Usage: friends <user_id>");
                        break;
                    case "friends":
                    {
                        var users = GetFriendsOf(splittedCommandLine[1]);
                        if (users == null) continue;
                        foreach (var user in users)
                            Console.WriteLine("\t" + user);
                        break;
                    }
                    case "followers" when splittedCommandLine.Length == 1:
                        Console.WriteLine("Usage: followers <user_id>");
                        break;
                    case "followers":
                    {
                        var users = GetFollowersOf(splittedCommandLine[1]);
                        if (users == null) continue;
                        foreach (var user in users)
                            Console.WriteLine("\t" + user);
                        break;
                    }
                    case "user":
                    case "userInfo":
                        if (splittedCommandLine.Length == 1)
                            Console.WriteLine($"Usage: {command} <user_id>");
                        else
                        {
                            var user = GetUserInfo(splittedCommandLine[1]);
                            if (user == null) continue;
                            Console.WriteLine("\t" + user);
                        }

                        break;
                    case "help":
                    case "?":
                        Console.WriteLine("friends <user_id> - get friends of user.");
                        Console.WriteLine("followers <user_id> - get followers of user.");
                        Console.WriteLine("user <user_id> - get user info.");
                        break;
                    default:
                        Console.WriteLine(@"Invalid command. Print ""help"" to get available comamnds.");
                        break;
                }
            }
        }

        private static User GetUserInfo(string userId)
        {
            var responseString = SendVkApiRequest("users.get", new Dictionary<string, string>
            {
                {"user_ids", userId},
                {"fields", "nickname,online"}
            });
            return TryDeserializeVkApiResponse<VkResponse<User[]>>(responseString)?.response?[0];
        }

        private static User[] GetFriendsOf(string id)
        {
            var responseString = SendVkApiRequest("friends.get", new Dictionary<string, string>
            {
                {"user_id", id},
                {"fields", "nickname,online"},
                {"order", "name"}
            });
            return TryDeserializeVkApiResponse<VkResponse<VkUsersResponse>>(responseString)?.response?.items;
        }

        private static User[] GetFollowersOf(string id)
        {
            var responseString = SendVkApiRequest("users.getFollowers", new Dictionary<string, string>
            {
                {"user_id", id},
                {"fields", "nickname,online"}
            });
            return TryDeserializeVkApiResponse<VkResponse<VkUsersResponse>>(responseString)?.response?.items;
        }

        private static T TryDeserializeVkApiResponse<T>(string responseString) where T : class
        {
            if (string.IsNullOrWhiteSpace(responseString))
                return null;
            var responseT = JsonConvert.DeserializeObject<T>(responseString);
            if (responseT == null || responseString.Contains("error"))
            {
                var responseError = JsonConvert.DeserializeObject<VkErrorResponse>(responseString);
                if (responseError != null)
                    Console.WriteLine($"Error #{responseError.error.error_code}: {responseError.error.error_msg}");
            }

            return responseT;
        }

        private static string SendVkApiRequest(string method, Dictionary<string, string> parameters)
        {
            try
            {
                var client = new WebClient();
                var request = GetHttpsVkRequest(method, parameters, ApiKey);
                var stream = client.OpenRead(request);
                if (stream == null)
                    return "";

                using (var streamReader = new StreamReader(stream))
                    return streamReader.ReadToEnd();
            }
            catch (Exception e)
            {
                Console.WriteLine($"{e.GetType().Name}: {e.Message}");
                return null;
            }
        }

        private const string VkApiVersion = "5.74";

        private static string GetHttpsVkRequest(string method, Dictionary<string, string> parameters,
            string accessToken, string apiVersion = VkApiVersion)
        {
            var url = new StringBuilder("https://api.vk.com/method/");

            if (string.IsNullOrWhiteSpace(method))
                throw new ArgumentException("Method cannot be empty");
            url.Append(method);
            url.Append('?');

            var args = parameters.Select(keyValue => string.Concat(keyValue.Key, '=', keyValue.Value));
            if (!string.IsNullOrWhiteSpace(accessToken))
                args = args.Concat("access_token=" + accessToken);
            if (!string.IsNullOrWhiteSpace(apiVersion))
                args = args.Concat("v=" + apiVersion);

            url.Append(string.Join("&", args));
            return url.ToString();
        }
    }

    internal static class Extensions
    {
        public static IEnumerable<T> Concat<T>(this IEnumerable<T> enumerable, T value)
        {
            foreach (var element in enumerable)
                yield return element;

            yield return value;
        }
    }
}