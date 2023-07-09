#region + -- IMPORTS -- +

using Microsoft.Win32;
//using NStandard;
using PortProxyGooey.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
//using System.Net.Sockets;
//using System.Net;
//using System.Runtime.InteropServices;
//using System.Text.RegularExpressions;
//using System.Windows.Forms;

#endregion

namespace PortProxyGooey.Utils {

    public static partial class PortProxyUtil {

        #region + -- VAR DECLARATIONS -- +

        internal static readonly string ServiceName = "iphlpsvc";
        internal static readonly string ServiceFriendlyName = "IP Helper";

        private static InvalidOperationException InvalidPortProxyType(string type) => new($"Invalid port proxy type ({type}).");
        private static readonly string[] ProxyTypes = new[] { "v4tov4", "v4tov6", "v6tov4", "v6tov6" };

        #endregion

        /// <summary>
        /// Gets the path to the registry key for the passed string
        /// </summary>
        /// <param name="type">Proxy Type (v4tov4, etc.)</param>
        /// <returns>(string)Path to registry key</returns>
        public static string GetKeyName(string type)
        {
            return $@"SYSTEM\CurrentControlSet\Services\PortProxy\{type}\tcp";
        }

        /// <summary>
        /// Read proxies from registry
        /// </summary>
        /// <returns>Array of Proxy Rules</returns>
        public static Rule[] GetProxies()
        {
            List<Rule> ruleList = new();

            foreach (string type in ProxyTypes) {

                string keyName = GetKeyName(type);
                RegistryKey key = Registry.LocalMachine.OpenSubKey(keyName);

                if (key is not null) {

                    foreach (string name in key.GetValueNames()) {

                        string[] listenParts = name.Split('/');
                        string listenOn = listenParts[0];
                        
                        if (!int.TryParse(listenParts[1], out int listenPort)) continue;

                        string[] connectParts = key.GetValue(name).ToString().Split('/');
                        string connectTo = connectParts[0];
                        
                        if (!int.TryParse(connectParts[1], out int connectPort)) continue;

                        ruleList.Add(new Rule {
                            Type = type,
                            ListenOn = listenOn,
                            ListenPort = listenPort,
                            ConnectTo = connectTo,
                            ConnectPort = connectPort,
                        });
                    }
                }
            }
            return ruleList.ToArray();
        }

        /// <summary>
        /// Write proxy to registry
        /// </summary>
        /// <param name="rule"></param>
        public static void AddOrUpdateProxy(Rule rule) {

            // $"netsh interface portproxy add {rule.Type} listenaddress={rule.ListenOn} listenport={rule.ListenPort} connectaddress={rule.ConnectTo} connectport={rule.ConnectPort}"

            if (!ProxyTypes.Contains(rule.Type)) throw InvalidPortProxyType(rule.Type);

            string keyName = GetKeyName(rule.Type);
            RegistryKey key = Registry.LocalMachine.OpenSubKey(keyName, true);
            string name = $"{rule.ListenOn}/{rule.ListenPort}";
            string value = $"{rule.ConnectTo}/{rule.ConnectPort}";

            if (key is null) Registry.LocalMachine.CreateSubKey(keyName);
            key = Registry.LocalMachine.OpenSubKey(keyName, true);
            key?.SetValue(name, value);

        }

        /// <summary>
        /// Delete proxy from registry
        /// </summary>
        /// <param name="rule"></param>
        public static void DeleteProxy(Rule rule) {

            // $"netsh interface portproxy delete {rule.Type} listenaddress={rule.ListenOn} listenport={rule.ListenPort}"

            if (!ProxyTypes.Contains(rule.Type)) throw InvalidPortProxyType(rule.Type);

            string keyName = GetKeyName(rule.Type);
            RegistryKey key = Registry.LocalMachine.OpenSubKey(keyName, true);
            string name = $"{rule.ListenOn}/{rule.ListenPort}";

            try {
                key?.DeleteValue(name);
            }
            catch {}

        }

        #region MISC

        /// <summary>
        /// Central function to open an URL/App, whatev.
        /// </summary>
        /// <param name="strFileOrURL">URL/App to open</param>
        /// <param name="strArgs">[optional] Any arguments to pass to the file/app. Defaults to empty string.</param>
        /// <param name="strStartIn">[optional] Directory to start in. Defaults to empty string.</param>
        public static void Launch(string strFileOrURL, string strArgs = "", string strStartIn = "") {

            ProcessStartInfo psi = new() {

                FileName = strFileOrURL,
                Arguments = strArgs,
                WorkingDirectory = strStartIn,
                UseShellExecute = true

            };

            Process.Start(psi);

        }

        #endregion

    }
}
