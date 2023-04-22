#region + -- NAMESPACE IMPORTS -- +

using Microsoft.Win32;
using NStandard;
using PortProxyGooey.Data;
using PortProxyGooey.Native;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows.Forms;

#endregion

namespace PortProxyGooey.Utils
{
    public static class PortProxyUtil
    {
        private static InvalidOperationException InvalidPortProxyType(string type) => new($"Invalid port proxy type ({type}).");
        private static readonly string[] ProxyTypes = new[] { "v4tov4", "v4tov6", "v6tov4", "v6tov6" };

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
        /// Fetches the current WSL IP
        /// </summary>
        /// <returns>Success: (string) IP Address; Fail: Empty string</returns>
        public static string GetWSLIP()
        {
            Process p = new();
            {
                ProcessStartInfo withBlock = p.StartInfo;
                withBlock.Verb = "runas";
                withBlock.RedirectStandardOutput = true;
                withBlock.RedirectStandardError = true;
                withBlock.FileName = "bash.exe";
                withBlock.Arguments = string.Format("{0} {1}", "-c", "\"ifconfig eth0 | grep 'inet '\"");
                withBlock.UseShellExecute = false;
                withBlock.CreateNoWindow = true;
            }

            p.Start();
            string strOutput = p.StandardOutput.ReadToEnd();
            p.WaitForExit();

            string strWSLIP = Regex.Match(strOutput, "\\d{1,3}\\.\\d{1,3}\\.\\d{1,3}\\.\\d{1,3}").ToString();
            return strWSLIP.Length > 0 ? strWSLIP : string.Empty;
        }

        /// <summary>
        /// Regex Validates IPv4 string
        /// </summary>
        /// <param name="ip"><string> IP Address to check</string></param>
        /// <returns></returns>
        public static bool IsIPv4(string ip)
        {
            return ip.IsMatch(new Regex(@"^\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}$"));
        }

        #region DNS UTILS

        [DllImport("dnsapi.dll", EntryPoint = "DnsFlushResolverCache")]
        static extern uint DnsFlushResolverCache();

        public static void FlushCache()
        {
            uint status = DnsFlushResolverCache();
            if (status == 0)
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

        #endregion

    }
}
