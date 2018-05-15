using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace InetTaskVkApi
{
    internal class VkResponse<T>
    {
        public T response { get; set; }
    }

    internal class VkUsersResponse
    {
        public int count { get; set; }

        public User[] items { get; set; }
    }

    internal class User
    {
        public int id { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string nickname { get; set; }
        public int online { get; set; }
        public bool IsOnline => online == 1;

        public override string ToString()
        {
            return
                $"{(IsOnline ? " " : "*")}[{id}] {first_name} {last_name} {(!HasNickname ? $"\"{nickname}\"" : string.Empty)}";
        }

        private bool HasNickname
        {
            get { return string.IsNullOrWhiteSpace(nickname); }
        }
    }
}