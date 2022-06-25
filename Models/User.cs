using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XmppMessenger.Models
{
    class User
    {
        private string username;
        private string hostname;

        public HashSet<string> Resources = new HashSet<string>();

        public User(string jid)
        {
            string[] parts = jid.Split('@');
            username = parts[0];
            hostname = parts[1];
        }

        public string Jid
        {
            get => $"{username}@{hostname}";
        }

        public override string ToString()
        {
            return Jid;
        }

        
    }
}
