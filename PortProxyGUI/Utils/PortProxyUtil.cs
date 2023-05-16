#region + -- NAMESPACE IMPORTS -- +

using Microsoft.Win32;
using NStandard;
using PortProxyGooey.Data;
using PortProxyGooey.Native;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows.Forms;

#endregion

namespace PortProxyGooey.Utils
{
    public static partial class PortProxyUtil
    {
        #region + -- VAR DECLARATIONS -- +

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
            foreach (string type in ProxyTypes)
            {
                string keyName = GetKeyName(type);
                RegistryKey key = Registry.LocalMachine.OpenSubKey(keyName);

                if (key is not null)
                {
                    foreach (string name in key.GetValueNames())
                    {
                        string[] listenParts = name.Split('/');
                        string listenOn = listenParts[0];
                        if (!int.TryParse(listenParts[1], out int listenPort)) continue;

                        string[] connectParts = key.GetValue(name).ToString().Split('/');
                        string connectTo = connectParts[0];
                        if (!int.TryParse(connectParts[1], out int connectPort)) continue;

                        ruleList.Add(new Rule
                        {
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
        public static void AddOrUpdateProxy(Rule rule)
        {
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
        public static void DeleteProxy(Rule rule)
        {
            // $"netsh interface portproxy delete {rule.Type} listenaddress={rule.ListenOn} listenport={rule.ListenPort}"

            if (!ProxyTypes.Contains(rule.Type)) throw InvalidPortProxyType(rule.Type);

            string keyName = GetKeyName(rule.Type);
            RegistryKey key = Registry.LocalMachine.OpenSubKey(keyName, true);
            string name = $"{rule.ListenOn}/{rule.ListenPort}";

            try
            {
                key?.DeleteValue(name);
            }
            catch { }
        }

        public static void ParamChange()
        {
            IntPtr hManager = NativeMethods.OpenSCManager(null, null, (uint)GenericRights.GENERIC_READ);
            if (hManager == IntPtr.Zero) throw new InvalidOperationException("Open SC Manager failed.");

            string serviceName = "iphlpsvc";
            IntPtr hService = NativeMethods.OpenService(hManager, serviceName, ServiceRights.SERVICE_PAUSE_CONTINUE);
            if (hService == IntPtr.Zero)
            {
                NativeMethods.CloseServiceHandle(hManager);
                throw new InvalidOperationException($"Open Service ({serviceName}) failed.");
            }

            ServiceStatus serviceStatus = new();
            bool success = NativeMethods.ControlService(hService, ServiceControls.SERVICE_CONTROL_PARAMCHANGE, ref serviceStatus);

            NativeMethods.CloseServiceHandle(hService);
            NativeMethods.CloseServiceHandle(hManager);

            if (!success)
            {
                throw new InvalidOperationException($"Control Service ({serviceName}) ParamChange failed.");
            }
        }

        /// <summary>
        /// Regex Validates IPv4 string
        /// </summary>
        /// <param name="ip"><string> IP Address to check</string></param>
        /// <returns></returns>
        public static bool IsIPv4(string ip)
        {
            return ip.IsMatch(JSE_Utils.IPValidation.IPv4RegEx);
        }

        /// <summary>
        /// Validates a potential IPv6 Address
        /// </summary>
        /// <param name="ip">IP to validate</param>
        /// <returns>True if valid; False if Invalid</returns>
        public static bool IsIPv6(string ip)
        {
            // Scott Note to original author: Is this correct? Looks more like a MAC Address regex? I added a better regex below, but leaving original for posterity.
            // return ip.IsMatch(new Regex(@"^[\dABCDEF]{2}(?::(?:[\dABCDEF]{2})){5}$"));
            return ip.IsMatch(JSE_Utils.IPValidation.IPv6RegEx);
        }

        #region DNS UTILS

        [return: MarshalAs(UnmanagedType.Bool)]
        [LibraryImport("dnsapi.dll")]
        public static partial bool DnsFlushResolverCache();

        public static void FlushCache()
        {
            bool status = DnsFlushResolverCache();
            if (status == false)
            {
                throw new InvalidOperationException("Flush DNS Cache failed.");
            }
            else
            {
                MessageBox.Show("DNS Flushed!", "Whoosh", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        #endregion

        #region MISC

        /// <summary>
        /// Central function to open an URL/App, whatev.
        /// </summary>
        /// <param name="strFileOrURL">URL/App to open</param>
        /// <param name="strArgs">[optional] Any arguments to pass to the file/app. Defaults to empty string.</param>
        /// <param name="strStartIn">[optional] Directory to start in. Defaults to empty string.</param>
        public static void Launch(string strFileOrURL, string strArgs = "", string strStartIn = "")
        {
            ProcessStartInfo psi = new()
            {
                FileName = strFileOrURL,
                Arguments = strArgs,
                WorkingDirectory = strStartIn,
                UseShellExecute = true
            };
            Process.Start(psi);
        }

        /// <summary>
        /// Check if an IPv4 IP is connectable on a specified port
        /// </summary>
        /// <param name="strHost">hostname or IP</param>
        /// <param name="intPort">[Optional] Port to test. default: 80</param>
        /// <returns>True = Connected; Else = Can't Connect</returns>
        public static bool CheckPortOpen(string strHost, int intPort = 80)
        {
            bool bIPv6 = false;

            if (IsIPv6(strHost))
            {
                bIPv6 = true;
            }
            else if (IsIPv4(strHost))
            {
                bIPv6 = false;
            }
            else 
            {
                // Not a valid IP; exit function
                Debug.WriteLine("CheckPortOpen(): Invalid IP {0}", strHost);
                return false;
            }

            // + -----------------------------------------------------------


            // Create an endpoint with the IPv6 address and port
            IPEndPoint endpoint = new IPEndPoint(IPAddress.Parse(strHost), intPort);

            Socket sock = new Socket(bIPv6 ? AddressFamily.InterNetworkV6 : AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // Set the socket options to use IPv6 and allow reusing the same port, and also allow receiving IPv4 traffic on the same port
            sock.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, 0);
            sock.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

            // Attempt to connect
            try
            {
                Debug.WriteLine("TESTING: IP{2}:Port {0}:{1}", strHost, intPort, bIPv6 ? "6" : "4");
                sock.Connect(endpoint);

                // If success, these get called, else, passed over for SocketException below.
                Debug.WriteLine("TESTING (SUCCESS): IP{2} Port {0} is open on {1}.", intPort, strHost, bIPv6 ? "6" : "4");
                return true;
            }
            catch (SocketException sx)
            {                
                Debug.WriteLine("TESTING (FAILED): IP{2} Port {0} is closed on {1}. (Code: {3})", intPort, strHost, bIPv6 ? "6" : "4", sx.ErrorCode.ToString());
            }
            finally
            {
                sock.Close();
            }
            return false;
        }

        #endregion

    }
}
