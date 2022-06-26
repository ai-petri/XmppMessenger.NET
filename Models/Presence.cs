using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XmppMessenger.Models
{
    class Presence
    {
        public string Jid { get; }
        public string Resource { get; }
        public Presence(string from)
        {
            string[] parts = from.Split("/");

            Jid = parts[0];

            if(parts.Length > 1)
            {
                Resource = parts[1];
            }
        }
    }
}
