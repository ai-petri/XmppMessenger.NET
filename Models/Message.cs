using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XmppMessenger.Models
{

    enum MessageType
    {
        Chat,
        Error,
        Groupchat,
        Headline,
        Normal
    }

    class Message
    {
        public DateTime Date { get; } = DateTime.Now;
        public MessageType Type { get; }
        public string From { get; }
        public string Text { get; }

        public bool Incoming { get; }
        public string Jid
        {
            get => From.Split("/")[0];
        }
        public string Resource
        {
            get => From.Split("/")[1];
        }
        public Message(string type, string from, string text, bool incoming = true)
        {
            From = from;
            Text = text;

            Incoming = incoming;

            switch (type)
            {
                case "chat": Type = MessageType.Chat;
                    break;
                case "error": Type = MessageType.Error;
                    break;
                case "groupchat": Type = MessageType.Groupchat;
                    break;
                case "headline": Type = MessageType.Headline;
                    break;
                default:
                    Type = MessageType.Normal;
                    break;
            }

        }
    }
}
