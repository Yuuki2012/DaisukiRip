using System;
using Fiddler;
using System.IO;
using System.Net;
using Microsoft.Win32;
using System.Text.RegularExpressions;

namespace DaisukiRip
{
    class Program
    {
        static RegistryKey RegKey = Registry.CurrentUser.OpenSubKey(@"Software\\Microsoft\\Windows\\CurrentVersion\\Internet Settings", true);

        static void Main(string[] args)
        {
            int MigrateProxy = (int)RegKey.GetValue("MigrateProxy");
            int ProxyEnable = (int)RegKey.GetValue("ProxyEnable");
            int ProxyHttp11 = (int)RegKey.GetValue("ProxyHttp1.1");
            string ProxyServer = (string)RegKey.GetValue("ProxyServer");
            string ProxyOverride = (string)RegKey.GetValue("ProxyOverride");

            Console.WriteLine("Press any key to close at any time");
            Console.WriteLine();
            
            FiddlerApplication.AfterSessionComplete += FiddlerApplication_AfterSessionComplete;
            FiddlerApplication.Startup(13334, FiddlerCoreStartupFlags.Default);

            Console.ReadKey();

            //Make sure to reset IE Settings to previous values
            FiddlerApplication.Shutdown();

            RegKey.SetValue("MigrateProxy",  MigrateProxy, RegistryValueKind.DWord);
            RegKey.SetValue("ProxyEnable",   ProxyEnable, RegistryValueKind.DWord);
            RegKey.SetValue("ProxyHttp1.1",  ProxyHttp11, RegistryValueKind.DWord);
            RegKey.SetValue("ProxyServer",   (String.IsNullOrEmpty(ProxyServer) ? "http://ProxyServername:80" : ProxyServer), RegistryValueKind.String);
            RegKey.SetValue("ProxyOverride", ProxyOverride, RegistryValueKind.String);
        }

        static void FiddlerApplication_AfterSessionComplete(Session oSession)
        {
            // more fail-safe: ^*[0-9].xml

            if (Regex.IsMatch(oSession.fullUrl.ToString(), "\bbngnwww.b-ch.com|/caption/|[0-9]{10}.xml|cashPath=\b"))
            {
                WebClient w = new WebClient();

                String cashString = oSession.fullUrl.ToString().Substring(oSession.fullUrl.ToString().IndexOf(".xml") + 4);
                String xml = oSession.fullUrl.ToString().Substring(oSession.fullUrl.ToString().LastIndexOf("/") + 1).Replace(cashString, "");
                String data = w.DownloadString(oSession.fullUrl.ToString());

                TextWriter f = new StreamWriter(@Environment.CurrentDirectory + @"\" + xml);
                f.Write(data);
                f.Close();

                Console.WriteLine("Caption URL found and saved as {0}", xml);
            }
        }
    }
}
