using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using Jint;

namespace jsParser
{
    class pacParser
    {
        static string getHostName(string host)
        {
            string[] a_host = host.Split(new Char[] { '.' });

            return (a_host.Count() > 2 ? a_host[0] : "");
        }
        static string getDnsDomain(string host)
        {
            string[] a_host = host.Split(new Char[] { '.' });

            return (a_host[0]);
        }

        static string dnsResolve(string host)
        {
            IPHostEntry hostInfo = Dns.GetHostEntry(host);
            string s = "";

            foreach (IPAddress ip in hostInfo.AddressList)
            {
                if (s.Length > 0)
                    s += ",";
                s += ip;
            }

            return (s);
        }
        static bool isInNet(string resolvedHost, string pattern, string mask)
        {
            string[] a_resolvedHost = resolvedHost.Split(new Char[1] { '.' });
            string[] a_pattern = pattern.Split(new Char[1] { '.' });
            string[] a_mask = mask.Split(new Char[1] { '.' });
            int len = a_resolvedHost.Count();

            if (len != a_pattern.Count() || len != a_mask.Count())
                return (false);

            for (int i = 0; i < len; i++)
                if (Convert.ToInt32(a_pattern[i]) != (Convert.ToInt32(a_resolvedHost[i]) & Convert.ToInt32(a_mask[i])))
                    return (false);

            return (true);
        }
        static bool isPlainHostName(string host)
        {
            return (host.IndexOf('.') == -1);
        }
        static bool localHostOrDomainIs(string host, string match)
        {
            return (host == match || host == getHostName(match));
        }
        static bool dnsDomainIs(string host, string match)
        {
            //int l_host = host.Length;
            //int l_match = match.Length;

            //if (l_host == 0 || l_match == 0 || l_host < l_match)
            //    return (false);
            //else if (l_host == l_match)
            //    return (host == match);

            //return (host.Substring(l_host - l_match) == match);

            return (getDnsDomain(host) == match);
        }
        static string myIpAddress()
        {
            IPHostEntry host;
            string localIP = "?";

            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                    localIP = ip.ToString();

            return (localIP);
        }

        static void Main(string[] args)
        {
            var engine = new Engine()
                .SetValue("dnsResolve", new Func<string, string>(dnsResolve))
                .SetValue("isInNet", new Func<string, string, string, bool>(isInNet))
                .SetValue("isPlainHostName", new Func<string, string>(dnsResolve))
                .SetValue("localHostOrDomainIs", new Func<string, string>(dnsResolve))
                .SetValue("dnsDomainIs", new Func<string, string>(dnsResolve))
                .SetValue("myIpAddress", new Func<string, string>(dnsResolve))
                .SetValue("log", new Action<object>(Console.WriteLine))
            ;

            if (args.Count() < 3)
            {
                Console.WriteLine(String.Format("Usage: ./pacParser PAC_FILE URL HOST"));
                return;
            }
            using (StreamReader streamReader = new StreamReader(args[0]))
            {
                string script = streamReader.ReadToEnd();

                engine.Execute(script);
                engine.Execute(String.Format(@"log(FindProxyForURL('{0}', '{1}'));", args[1], args[2]));

                streamReader.Close();
            }

//            engine.Execute(@"
//            log(dnsResolve('www.perdu.com'));
//            log(isInNet('198.95.249.79', '198.95.249.79', '255.255.255.255'));
//            log(isInNet('198.95.249.79', '198.95.0.0', '255.255.0.0'));
//            log(isInNet('198.95.249.12', '198.95.249.79', '255.255.255.255'));
//            log(isInNet('198.95.249.12', '198.95.0.0', '255.255.0.0'));
//            log(isInNet('198.12.249.79', '198.95.0.0', '255.255.0.0'));
//            ");

            //Console.Read();
        }
    }
}
