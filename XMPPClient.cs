using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net.Security;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Windows;

namespace XmppMessenger
{
    class XMPPClient
    {
        string hostname = "";
        string resource = "messenger";

        NetworkStream stream;
        SslStream sslStream;
        TcpClient client = new TcpClient();

        Thread thread;

        volatile bool running;

        public bool Connected
        {
            get => client.Connected;
        }

        string Read()
        {
            while (!stream.DataAvailable)
            {
                Thread.Sleep(500);
            }

            byte[] bytes = new byte[2048];

            if (sslStream == null)
            {
                stream.Read(bytes, 0, bytes.Length);
            }
            else
            {
                sslStream.Read(bytes, 0, bytes.Length);
            }

            return Encoding.UTF8.GetString(bytes);
        }

        XElement ReadXML(string expected = "")
        {
            string text = Read();
            while (!text.Contains(expected)) text = Read();

            string repaired = "";

            foreach (char c in text)
            {
                if (XmlConvert.IsXmlChar(c))
                {
                    repaired += c;
                }
            }

            XDocument doc = XDocument.Parse(repaired);
            return doc.Root;
        }


        void Write(string str)
        {
            if (sslStream == null)
            {
                stream.Write(Encoding.UTF8.GetBytes(str));
            }
            else
            {
                sslStream.Write(Encoding.UTF8.GetBytes(str));
            }
        }


        Dictionary<string, string> ParseKeyValuePairs(string str)
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            string[] pairs = str.Split(',');
            foreach (string pair in pairs)
            {
                string[] parts = pair.Split('=', 2);
                dictionary.Add(parts[0], parts[1]);
            }

            return dictionary;
        }


        public void Close()
        {
            client.Close();
            running = false;
            if (thread != null)
            {
                thread.Join();

                MessageBox.Show(thread.ThreadState.ToString());
            }
        }


        public bool Connect(string hostname, string username, string password)
        {
            try
            {
                client.Connect(hostname, 5222);
            }
            catch (Exception)
            {
                Console.WriteLine("Can't connect");
                client.Close();
                return false;
            }
            stream = client.GetStream();

            this.hostname = hostname;

            Write($"<?xml version=\"1.0\"?><stream:stream xmlns=\"jabber:client\" xmlns:stream=\"http://etherx.jabber.org/streams\" version=\"1.0\" to=\"{hostname}\">");


            // stream:stream
            Read();


            //StartTLS -------------------------------------------------------------------------------

            Write("<starttls xmlns='urn:ietf:params:xml:ns:xmpp-tls'/>");

            string serverResponse = Read();

            while (!serverResponse.Contains("proceed"))
                serverResponse = Read();

            sslStream = new SslStream(stream);

            sslStream.AuthenticateAsClient(hostname);

            Write($"<stream:stream xmlns=\"jabber:client\" xmlns:stream=\"http://etherx.jabber.org/streams\" version=\"1.0\" to=\"{hostname}\">");

            //---------------------------------------------------------------------------------------------


            string clientNonce;
            using (var generator = RandomNumberGenerator.Create())
            {
                byte[] bytes = new byte[32];
                generator.GetBytes(bytes);
                clientNonce = Convert.ToBase64String(bytes);
            }

            string str = $"n,,n={username},r={clientNonce}";

            string message1 = $"<auth xmlns=\"urn:ietf:params:xml:ns:xmpp-sasl\" mechanism=\"SCRAM-SHA-1\">{Convert.ToBase64String(Encoding.UTF8.GetBytes(str))}</auth>";
            Write(message1);
            Read();

            // challenge

            XElement XMLFromServer1 = ReadXML("challenge");

            if (XMLFromServer1.Name.LocalName != "challenge")
            {
                client.Close();
                return false;
            }

            string challenge = XMLFromServer1.Value;

            string decoded = Encoding.UTF8.GetString(Convert.FromBase64String(challenge));

            Dictionary<string, string> parsed = ParseKeyValuePairs(decoded);

            string r, s, i = "";

            parsed.TryGetValue("r", out r);
            parsed.TryGetValue("s", out s);
            parsed.TryGetValue("i", out i);


            if (!r.StartsWith(clientNonce))
            {
                Console.WriteLine("error");
                return false;
            }


            string response = $"c=biws,r={r}";

            byte[] salt = Convert.FromBase64String(s);

            byte[] saltedPassword = KeyDerivation.Pbkdf2(password, salt, KeyDerivationPrf.HMACSHA1, Int32.Parse(i), 20);



            byte[] clientKey;
            using (var hmac = new HMACSHA1(saltedPassword))
            {
                clientKey = hmac.ComputeHash(Encoding.UTF8.GetBytes("Client Key"));
            }



            byte[] storedKey;
            using (var sha1 = SHA1.Create())
            {
                storedKey = sha1.ComputeHash(clientKey);
            }

            string authMessage = str.Substring(3) + "," + decoded + "," + response;



            byte[] clientSignature;
            using (var hmac = new HMACSHA1(storedKey))
            {
                clientSignature = hmac.ComputeHash(Encoding.UTF8.GetBytes(authMessage));
            }


            byte[] proof = new byte[clientKey.Length];

            for (int j = 0; j < clientKey.Length; j++)
            {
                proof[j] = (byte)(clientKey[j] ^ clientSignature[j]);
            }

            response += ",p=" + Convert.ToBase64String(proof);

            string message2 = $"<response xmlns = \"urn:ietf:params:xml:ns:xmpp-sasl\" >{Convert.ToBase64String(Encoding.UTF8.GetBytes(response))}</response>";

            Write(message2);



            // success

            XElement XMLFromServer2 = ReadXML();

            if (XMLFromServer2.Name.LocalName == "failure")
            {
                Console.WriteLine(XMLFromServer2.Value);
            }

            if (XMLFromServer2.Name.LocalName != "success")
            {
                client.Close();
                return false;
            }

            Dictionary<string, string> parsed2 = ParseKeyValuePairs(Encoding.UTF8.GetString(Convert.FromBase64String(XMLFromServer2.Value)));

            string v = "";

            parsed2.TryGetValue("v", out v);

            byte[] serverKey;
            using (var hmac = new HMACSHA1(saltedPassword))
            {
                serverKey = hmac.ComputeHash(Encoding.UTF8.GetBytes("Server Key"));
            }

            byte[] serverSignature;
            using (var hmac = new HMACSHA1(serverKey))
            {
                serverSignature = hmac.ComputeHash(Encoding.UTF8.GetBytes(authMessage));
            }

            if (v == Convert.ToBase64String(serverSignature))
            {
                // start stream
                Write($"<?xml version=\"1.0\"?><stream:stream to=\"{hostname}\" xml:lang=\"en\" version=\"1.0\" xmlns=\"jabber:client\" xmlns:stream=\"http://etherx.jabber.org/streams\">");
                Read();

                // bind resourse
                Write($"<iq id=\"_xmpp_bind1\" type=\"set\"><bind xmlns=\"urn:ietf:params:xml:ns:xmpp-bind\"><resource>${resource}</resource></bind></iq>");
                Read();

                // start session
                Write($"<iq to=\"{hostname}\" type=\"set\" id=\"sess_1\"><session xmlns=\"urn:ietf:params:xml:ns:xmpp-session\"/></iq>");
                ReadXML("sess_1");

                // send presence
                Write("<presence />");
                ReadXML("presence");

                return true;
            }

            return false;

        }

        public void Listen()
        {
            thread = new Thread(new ThreadStart(() =>
            {
                while (Connected && running)
                {
                    try
                    {
                        XElement element = ReadXML();
                        MessageBox.Show(element.Name.ToString());

                    }
                    catch (Exception) { }

                }
            }));

            running = true;
            thread.Start();


        }


    }
}
