#region + -- NAMESPACE IMPORTS -- +

using Microsoft.Win32;
using PortProxyGUI.Data;
using PortProxyGUI.Native;
using System;
using System.Collections.Generic;
using System.Linq;

#endregion

namespace PortProxyGUI.Utils
{
    public static class PortPorxyUtil
    {
        private static InvalidOperationException InvalidPortProxyType(string type) => new($"Invalid port proxy type ({type}).");
        private static readonly string[] ProxyTypes = new[] { "v4tov4", "v4tov6", "v6tov4", "v6tov6" };

        private static string GetKeyName(string type)
        {
            return $@"SYSTEM\CurrentControlSet\Services\PortProxy\{type}\tcp";
        }

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

    }
}
