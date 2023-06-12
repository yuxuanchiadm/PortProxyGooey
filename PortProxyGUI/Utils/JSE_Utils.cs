#region + -- IMPORTS -- +

using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using NStandard;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.ComponentModel;

#endregion

namespace JSE_Utils {

    public static class Dialogs {
    
        /// <summary>
        /// Creates a custom dialog form so this overall Utils class is more portable
        /// </summary>
        /// <param name="strText">The text to show the user</param>
        /// <param name="strTitle">The title text of the form itself</param>
        /// <returns>DialogResult.OK on "Ok" button click; DialogResult.Cancel on "Cancel" button or X button click.</returns>
        public static DialogResult CustomDialog(string strText, string strTitle) {

            DialogResult result = DialogResult.Cancel;

            using (Form form = new Form()) {

                // Form Props
                form.Text = strTitle;
                form.BackColor = Color.FromArgb(67, 76, 94);
                form.ShowIcon = false;
                form.StartPosition = FormStartPosition.CenterScreen;
                form.ShowInTaskbar = false;
                form.FormBorderStyle = FormBorderStyle.FixedSingle;
                form.MaximizeBox = false;
                form.MinimizeBox = false;
                form.Size = new Size(500, 400);

                // Form Controls
                Label lblInfo = new() {
                    BackColor = Color.FromArgb(67, 76, 94),
                    ForeColor = Color.FromArgb(229, 233, 240),
                    Location = new Point(20, 20),
                    Size = new Size(450, 270),
                    AutoSize = false,
                    Font = new Font("Microsoft Sans Serif", 12),
                    Text = strText,
                };

                // Buttons
                Button btnOK = new() {
                    BackColor = Color.FromArgb(46, 52, 64),
                    ForeColor = Color.FromArgb(235, 203, 139),
                    Location = new Point(150, 300),
                    Size = new Size(80, 50),
                    Font = new Font("Microsoft Sans Serif", 12, FontStyle.Bold),
                    Text = "OK",
                    DialogResult = DialogResult.OK,
                    FlatStyle = FlatStyle.Flat,
                };

                // ...
                Button btnCancel = new() {
                    BackColor = Color.FromArgb(46, 52, 64),
                    ForeColor = Color.FromArgb(191, 97, 106),
                    Location = new Point(250, 300),
                    Size = new Size(80, 50),
                    Font = new Font("Microsoft Sans Serif", 12, FontStyle.Bold),
                    Text = "Cancel",
                    DialogResult = DialogResult.Cancel,
                    FlatStyle = FlatStyle.Flat,
                };

                // Add the controls
                form.Controls.Add(lblInfo);
                form.Controls.Add(btnOK);
                form.Controls.Add(btnCancel);

                // Set the Cancel button as the focused button, essentially acting as the default button
                form.ActiveControl = btnCancel;

                // Show the form and get the button they clicked
                if (form.ShowDialog() == DialogResult.OK) result = DialogResult.OK;
            }

            // Defaults to false, gets set to true above, only if user clicked the OK button.
            return result;
        }

        /// <summary>
        /// Replicates old school VB InputDialog, for a quick, simple input form.
        /// </summary>
        /// <param name="strInput">Will be placed in the text box if pre-filled; useful for suggestion as well.</param>
        /// <param name="strTitle">Placed on the form's titlebar.</param>
        /// <returns>DialogResult. Also, the input var passed to it will contain the user text (see example).</returns>
        public static DialogResult InputDialog(ref string strInput, string strTitle = "") {

            // Example Usage:
            // 
            // string input = string.Empty;
            //
            // if (Dialogs.InputDialog(ref input, "New name:") == DialogResult.OK && !string.IsNullOrEmpty(input)) {
            //    Debug.WriteLine(input);
            // }
            //
            // input will contain what user entered after it returns.

            Size size = new Size(200, 70);
            Form inputBox = new Form();

            inputBox.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            inputBox.ShowInTaskbar = false;
            inputBox.StartPosition = FormStartPosition.CenterParent;
            inputBox.ClientSize = size;
            inputBox.Text = strTitle;

            TextBox textBox = new TextBox();
            textBox.Size = new Size(size.Width - 10, 23);
            textBox.Location = new Point(5, 5);
            textBox.Text = strInput;
            inputBox.Controls.Add(textBox);

            Button okButton = new Button();
            okButton.DialogResult = DialogResult.OK;
            okButton.Name = "okButton";
            okButton.Size = new Size(75, 23);
            okButton.Text = "&OK";
            okButton.Location = new Point(size.Width - 80 - 80, 39);
            inputBox.Controls.Add(okButton);

            Button cancelButton = new Button();
            cancelButton.DialogResult = DialogResult.Cancel;
            cancelButton.Name = "cancelButton";
            cancelButton.Size = new Size(75, 23);
            cancelButton.Text = "&Cancel";
            cancelButton.Location = new Point(size.Width - 80, 39);
            inputBox.Controls.Add(cancelButton);

            inputBox.AcceptButton = okButton;
            inputBox.CancelButton = cancelButton;

            DialogResult result = inputBox.ShowDialog();
            strInput = textBox.Text;

            return result;

        }    
    
    }

    public static partial class DNS {

        [return: MarshalAs(UnmanagedType.Bool)]
        [LibraryImport("dnsapi.dll")]
        public static partial bool DnsFlushResolverCache();

        /// <summary>
        /// Flushes the system's DNS cache
        /// </summary>
        /// <param name="bConfirm">[optional: default true] Show a confirmation MessageBox first</param>
        /// <param name="bResult">[optional: default true] Show a sussessful confirmation MessageBox when done</param>
        /// <exception cref="InvalidOperationException"></exception>
        public static void FlushCache(bool bConfirm = true, bool bResult = true) {

            // Show dialog asking for Flush confirmation, giving them a change to bail out if it was a misclick.
            if (bConfirm) {

                // If Yes, continue below; if no, bail out of method.
                if (MessageBox.Show("Flush DNS?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.No) {

                    Debug.WriteLine("FlushCache: User chose to not flush cache; exiting method.");
                    return;
                }

            }

            // If use either clicked Yes yes above, or bConfirm was False, flush.
            bool status = DnsFlushResolverCache();

            if (status == false) {
                throw new InvalidOperationException("FlushDNS Cache failed.");
            } else {

                // Show a confirmation dialog if desired
                if (bResult) {
                    MessageBox.Show("DNS Flushed!", "Whoosh", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

            }

        }

    }

    public static class Docker {

        /// <summary>
        /// Checks for 'evidence' of Docker running in WSL (async)
        /// </summary>
        /// <returns>True if Docker is running; False if not.</returns>
        /// <remarks>Will start WSL if WSL isn't already running, which may or may not be desired.</remarks>
        public static bool IsDockerRunning() {

            bool bResult = false;

            Task task = Task.Run(() => {

                //Debug.WriteLine($"IsDockerRunning: Task started (ID: {Task.CurrentId})");

                bResult = Misc.RunCommand("wsl", "docker ps", true).Contains("CONTAINER ID");

                //Debug.WriteLine($"IsDockerRunning: Task complete (ID: {Task.CurrentId})");

            });

            task.Wait();
            return bResult;

        }

    }

    public static partial class IPValidation
    {
        #region + -- REGEX -- +

        public static Regex IPv4RegEx = IPv4Pattern();
        public static Regex IPv6RegEx = IPv6Pattern();

        [GeneratedRegex("\\d{1,3}\\.\\d{1,3}\\.\\d{1,3}\\.\\d{1,3}")]
        private static partial Regex IPv4Pattern();

        [GeneratedRegex("^(([0-9a-fA-F]{1,4}:){7}([0-9a-fA-F]{1,4}|:))|(([0-9a-fA-F]{1,4}:){0,6}(:[0-9a-fA-F]{1,4}){1,6})$")]
        private static partial Regex IPv6Pattern();

        #endregion
    }

    public static class Listview {

        /// <summary>
        /// Returns number of items in a listview group
        /// </summary>
        /// <param name="listView">A ListView object</param>
        /// <param name="group">The Group to count items in</param>
        /// <returns>Number if items in that group</returns>
        public static int CountItemsInGroup(ListView listView, ListViewGroup group) {

            int count = 0;

            foreach (ListViewItem item in listView.Items) {

                if (item.Group == group) {
                    count++;
                }

            }
            return count;
        }

        /// <summary>
        /// Returns number of items in a listview group based on the group header (similar to group name)
        /// </summary>
        /// <param name="listView">A ListView object</param>
        /// <param name="strHeader">The Group Header to count items in</param>
        /// <returns>Number if items in that group</returns>
        public static int CountItemsInGroup(ListView listView, string strHeader) {
            // TODO: add Default group handling as well; also add example usage comment.
            ListViewGroup group = null;
            int itemCount = 0;

            foreach (ListViewGroup listViewGroup in listView.Groups) {

                // NOTE: This entire method could easily be modified to match something else by changing the following line; i.e. listViewGroup.Name
                if (listViewGroup.Header == strHeader) {

                    group = listViewGroup;
                    break;

                }

            }

            if (group != null) {
                itemCount = group.Items.Count;
            } else {
                // The group with the specified name was not found
            }
            return itemCount;
        }

    }

    public static class Network {

        /// <summary>
        /// Check if an IPv4 IP is connectable on a specified port
        /// </summary>
        /// <param name="strHost">hostname or IP</param>
        /// <param name="intPort">[optional: default 80] Port to test</param>
        /// <returns>True = Connected; Else = Can't Connect</returns>
        public static bool CheckPortOpen(string strHost, int intPort = 80) {

            bool bIPv6 = false;

            if (IsIPv6(strHost)) {

                bIPv6 = true;

            } else if (IsIPv4(strHost)) {

                bIPv6 = false;

            } else {

                // Not a valid IP; exit function
                Debug.WriteLine("CheckPortOpen(): Invalid IP {0}", strHost);
                return false;

            }

            // + --- +


            // Create an endpoint with the IPv6 address and port
            IPEndPoint endpoint = new IPEndPoint(IPAddress.Parse(strHost), intPort);

            Socket sock = new Socket(bIPv6 ? AddressFamily.InterNetworkV6 : AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // Set the socket options to use IPv6 and allow reusing the same port, and also allow receiving IPv4 traffic on the same port
            sock.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, 0);
            sock.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

            // Attempt to connect
            try {

                Debug.WriteLine("TESTING: IP{2}:Port {0}:{1}", strHost, intPort, bIPv6 ? "6" : "4");
                sock.Connect(endpoint);

                // If success, these get called, else, passed over for SocketException below.
                Debug.WriteLine("TESTING (SUCCESS): IP{2} Port {0} is open on {1}.", intPort, strHost, bIPv6 ? "6" : "4");
                return true;

            } catch (SocketException sx) {
                Debug.WriteLine("TESTING (FAILED): IP{2} Port {0} is closed on {1}. (Code: {3})", intPort, strHost, bIPv6 ? "6" : "4", sx.ErrorCode.ToString());
            } finally {
                sock.Close();
            }

            return false;
        }

        /// <summary>
        /// Shows the current machine's local IP
        /// </summary>
        /// <param name="bIPv6">[optional: default false] True to return the IP6 IP, False for IP4 IP.</param>
        /// <returns>Local IP</returns>
        /// <exception cref="Exception"></exception>
        public static string GetLocalIPAddress(bool bIPv6 = false) {
            // TODO: this isnt done. the list of all addresses shows multiple ip's; not sure which I want returned?
            string strIP = "N/A";
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());

            foreach (IPAddress ip in host.AddressList) {

                if (ip.AddressFamily == AddressFamily.InterNetworkV6 && bIPv6 == true || ip.AddressFamily == AddressFamily.InterNetwork && bIPv6 == false) {
                    strIP = ip.ToString();
                } else {
                    //throw new Exception("No network adapters with an IPv4 address in the system!");
                }

            }
 
            return strIP;

        }

        /// <summary>
        /// Regex Validates IPv4 string
        /// </summary>
        /// <param name="ip"><string> IP Address to check</string></param>
        /// <returns></returns>
        public static bool IsIPv4(string ip) {
            return ip.IsMatch(IPValidation.IPv4RegEx);
        }

        /// <summary>
        /// Validates a potential IPv6 Address
        /// </summary>
        /// <param name="ip">IP to validate</param>
        /// <returns>True if valid; False if Invalid</returns>
        public static bool IsIPv6(string ip) {

            // Scott Note to original author: Is this correct? Looks more like a MAC Address regex? I added a better regex below, but leaving original for posterity.
            // return ip.IsMatch(new Regex(@"^[\dABCDEF]{2}(?::(?:[\dABCDEF]{2})){5}$"));
            return ip.IsMatch(IPValidation.IPv6RegEx);

        }

        public static bool IsNetworkAvailable() {
            return NetworkInterface.GetIsNetworkAvailable();
        }

        /// <summary>
        /// Open a website in the browser to test your ports
        /// </summary>
        public static void Link_OpenPortTester() {
            Misc.RunCommand("https://www.yougetsignal.com/tools/open-ports");
        }
    
    }

    public static class Misc {

        /// <summary>
        /// Standard Process Start function
        /// </summary>
        /// <param name="strFileName">Filename|Program.exe to run</param>
        /// <param name="strArgs">[optional] Any arguments for the above file|program</param>
        /// <returns>Program output if any, else empty string</returns>
        public static string RunCommand(string strFileName, string strArgs = "", bool bGetOutput = true) {

            string strOutput = string.Empty;
            string pattern = @"(http|https|ftp)://";

            // Determine if a URL has been passed. Using this Process method requires a lil bit of special handling in such case.
            MatchCollection matches = Regex.Matches(strFileName, pattern);

            if (matches.Count > 0) {

                // Switch the filename (which is a URL) for strArgs, and append any specifically added Args after that.
                strArgs = $"{strFileName} {strArgs}";

                // Use Explorer by default
                strFileName = "explorer.exe";

            }

            try {

                using Process p = new();
                {
                    ProcessStartInfo withBlock = p.StartInfo;
                    withBlock.Verb = "runas";
                    withBlock.RedirectStandardOutput = true;
                    withBlock.RedirectStandardError = true;
                    withBlock.FileName = strFileName;
                    withBlock.Arguments = strArgs.Trim();
                    withBlock.UseShellExecute = false;
                    withBlock.CreateNoWindow = true;
                }

                p.Start();

                // If we always do the following, the app will lock up when doing something like starting WSL, so it's optional.
                if (bGetOutput) {

                    // Remove any null chars, if exist.
                    strOutput = p.StandardOutput.ReadToEnd().Replace("\0", "");
                    p.WaitForExit();

                }

            } catch (Exception e) {
                Debug.WriteLine($"RunCommand() Error: {e.Message}");
                throw;
            }

            return strOutput;
        }

        /// <summary>
        /// Standard Process Start function (async)
        /// </summary>
        /// <param name="strFileName">Filename|Program.exe to run</param>
        /// <param name="strArgs">[optional] Any arguments for the above file|program</param>
        /// <returns>Program output if any, else empty string</returns>
        public static async Task<string> RunCommandAsync(string strFileName, string strArgs = "", bool bGetOutput = true) {

            string strOutput = string.Empty;
            string pattern = @"(http|https|ftp)://";

            // Determine if a URL has been passed. Using this Process method requires a lil bit of special handling in such case.
            MatchCollection matches = Regex.Matches(strFileName, pattern);

            if (matches.Count > 0) {

                // Switch the filename (which is a URL) for strArgs, and append any specifically added Args after that.
                strArgs = $"{strFileName} {strArgs}";

                // Use Explorer by default
                strFileName = "explorer.exe";

            }

            try {

                using Process p = new Process();
                {
                    ProcessStartInfo withBlock = p.StartInfo;
                    withBlock.Verb = "runas";
                    withBlock.RedirectStandardOutput = true;
                    withBlock.RedirectStandardError = true;
                    withBlock.FileName = strFileName;
                    withBlock.Arguments = strArgs.Trim();
                    withBlock.UseShellExecute = false;
                    withBlock.CreateNoWindow = true;
                }

                p.Start();

                if (bGetOutput) {

                    // Remove any null chars, if exist.
                    strOutput = await p.StandardOutput.ReadToEndAsync();
                    await p.WaitForExitAsync();
                }

            } catch (Exception e) {

                Debug.WriteLine($"RunCommand() Error: {e.Message}");
                throw;

            }

            return strOutput;
        }

        /// <summary>
        /// Checks if a process is running
        /// </summary>
        /// <param name="strProcessName">Name of the process</param>
        /// <returns>True: running; False: not running.</returns>
        public static bool IsProcessRunning(string strProcessName) {

            Process[] p = Process.GetProcessesByName(strProcessName);
            return p.Count() > 0;

        }

    }

    public static class Winsock {

        /// <summary>
        /// Resets the Winsock
        /// </summary>
        public static void Reset() {

            string strInfo = string.Format(
                "{0}{3}{3}{1}{3}{3}{2}",
                "This command also resets other networking components like the TCP/IP stack, which can fix various network issues like connectivity problems, DNS resolution issues, and more.",
                "Note that resetting the Winsock should be used as a last resort to fix network issues, as it can affect any running network applications and may cause other issues. If you are experiencing network issues, it may be better to diagnose and fix the root cause of the problem rather than resetting the Winsock.",
                "After running the netsh winsock reset command, you will need to restart your computer for the changes to take effect.",
                Environment.NewLine
            );

            if (Dialogs.CustomDialog(strInfo, "Winsock Reset") == DialogResult.OK) {

                // Run the winsock reset command
                string strOutput = Misc.RunCommand("netsh.exe", "winsock reset");

                // Report back the result of the reset attempt
                if (strOutput.ToLower().Contains("sucessfully reset")) {

                    MessageBox.Show("You must restart the computer in order to complete the reset.", "Sucessfully reset your Winsock", MessageBoxButtons.OK, MessageBoxIcon.Information);
                
                } else {

                    MessageBox.Show(strOutput, "Couldn't reset your Winsock", MessageBoxButtons.OK, MessageBoxIcon.Error);
                
                }
            }
        }
    }

    public static class WSL {

        /// <summary>
        /// Checks if WSL is running
        /// </summary>
        /// <param name="bShowMessage">[optional: default False] will show a messagebox with the result.</param>
        /// <returns>True: WSL is running; False: WSL isn't running.</returns>
        public static bool WSL_IsRunning(bool bShowMessage = false) {

            bool bResult = false;

            //Debug.WriteLine($"WSL_IsRunning: Task started (ID: {Task.CurrentId})");
            bResult = Misc.IsProcessRunning("wsl");
            //Debug.WriteLine($"WSL_IsRunning: Task complete (ID: {Task.CurrentId})");

            if (bShowMessage) {
                MessageBox.Show(string.Format("WSL {0} running. ", bResult ? "is" : "is not"), "WSL", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            return bResult;

        }

        /// <summary>
        /// Checks if WSL is running (async)
        /// </summary>
        /// <param name="bShowMessage">[optional: default False] will show a messagebox with the result.</param>
        /// <returns>True: WSL is running; False: WSL isn't running.</returns>
        public static void WSL_IsRunning_BackgroundWorker(Action<bool> callback, bool bShowMessage = false) {

            // Example usage:
            //
            // WSL.WSL_IsRunning_BackgroundWorker((result) => {
            //     if (result) {
            //         lblWSLRunning.Text = "WSL: RUNNING";
            //         picWSL.Visible = true;
            //     } else {
            //         lblWSLRunning.Text = "WSL: N/A";
            //         picWSL.Visible = false;
            //     }
            // }, false);

            BackgroundWorker worker = new BackgroundWorker();

            worker.DoWork += (sender, e) => {

                Process[] p = Process.GetProcessesByName("wsl");
                bool isRunning = p.Length > 0;
                e.Result = isRunning;

            };

            worker.RunWorkerCompleted += (sender, e) => {

                if (e.Error != null) {
                    // Handle errors
                } else {

                    bool isRunning = (bool)e.Result;

                    if (bShowMessage) {
                        MessageBox.Show(string.Format("WSL {0} running. ", isRunning ? "is" : "is not"), "WSL", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }

                    callback?.Invoke(isRunning);

                }

            };

            worker.RunWorkerAsync();
        }

        /// <summary>
        /// Gets a list of all distros installed, their WSL version, and running state. (async)
        /// </summary>
        /// <returns>Success: WSL Distro info; Fail: Empty string.</returns>
        public static string WSL_GetDistros() {

            string strOutput = string.Empty;

            Task task = Task.Run(() => {

                Debug.WriteLine($"WSL_GetDistros: Task started (ID: {Task.CurrentId})");

               // Run the command in Bash
               strOutput = Misc.RunCommand("wsl.exe", "-l -v --all");

                Debug.WriteLine($"WSL_GetDistros: Task complete (ID: {Task.CurrentId})");

            });

            task.Wait();

            // TODO: Should be beefed up with validation (make sure proper info was fetched)
            return strOutput.Length > 0 ? strOutput : string.Empty;
        }

        /// <summary>
        /// Gets *all* version info of the currently installed WSL version (async)
        /// </summary>
        /// <returns>Success: WSL Version info; Fail: Empty string.</returns>
        public static string WSL_GetAllVersionInfo() {

            string strOutput = string.Empty;

            Task task = Task.Run(() => {

                Debug.WriteLine($"WSL_GetAllVersionInfo: Task started (ID: {Task.CurrentId})");

                // Run the command
                strOutput = Misc.RunCommand("wsl.exe", "--version");

                Debug.WriteLine($"WSL_GetAllVersionInfo: Task complete (ID: {Task.CurrentId})");

            });

            task.Wait();

            // TODO: Should be beefed up with validation (make sure proper info was fetched)
            return strOutput.Length > 0 ? strOutput : string.Empty;
        }

        /// <summary>
        /// Gets the currently installed WSL version (WSL *version only*) (async)
        /// </summary>
        /// <returns>Success: WSL Version; Fail: Empty string.</returns>
        public static string WSL_GetVersion() {

            string strOutput = string.Empty;

            Task task = Task.Run(() => {

                Debug.WriteLine($"WSL_GetVersion: Task started (ID: {Task.CurrentId})");

                // Run the command
                strOutput = Misc.RunCommand("wsl.exe", "--version");

                Debug.WriteLine($"WSL_GetVersion: Task complete (ID: {Task.CurrentId})");

            });

            task.Wait();

            string start = "wsl version: ";
            string end = "\r\n";

            // Try to parse out a version number
            int startIndex = strOutput.ToLower().IndexOf(start) + start.Length;
            int endIndex = strOutput.IndexOf(end, startIndex);
            string strResult = strOutput.Substring(startIndex, endIndex - startIndex);

            // TODO: Should be beefed up with validation (make sure an actual version number was fetched)
            return strResult.Length > 0 ? strResult : string.Empty;
        }

        /// <summary>
        /// Fetches the current WSL IP
        /// </summary>
        /// <returns>Success: (string) IP Address; Fail: Empty string</returns>
        public static string WSL_GetIP_Task() {
            // TODO: Does running this START WSL if it's not already running?
            string strOutput = string.Empty;
            string strWSLIP = string.Empty;

            Task task = Task.Run(() => {

                //Debug.WriteLine($"WSL_GetIP: Task started (ID: {Task.CurrentId})");

                // Run the command in Bash
                strOutput = Misc.RunCommand("bash.exe", string.Format("{0} {1}", "-c", "\"ifconfig eth0 | grep 'inet '\""));
                
                // Try to parse out a valid IPv4
                strWSLIP = IPValidation.IPv4RegEx.Match(strOutput).ToString();  
                
                //Debug.WriteLine($"WSL_GetIP: Task complete (ID: {Task.CurrentId})");
            });

            task.Wait();

            return strWSLIP.Length > 0 ? strWSLIP : string.Empty;

        }

        /// <summary>
        /// Fetches the current WSL IP (async Task)
        /// </summary>
        /// <returns>Success: (string) IP Address; Fail: Empty string</returns>
        public static async Task<string> WSL_GetIP_Task_Async() {

            string strOutput = string.Empty;
            string strWSLIP = string.Empty;

            await Task.Run(async () =>
            {
                //Debug.WriteLine($"WSL_GetIP: Task started (ID: {Task.CurrentId})");

                // Run the command in Bash
                strOutput = await Misc.RunCommandAsync("bash.exe", string.Format("{0} {1}", "-c", "\"ifconfig eth0 | grep 'inet '\""));

                // Try to parse out a valid IPv4
                strWSLIP = IPValidation.IPv4RegEx.Match(strOutput).ToString();

                //Debug.WriteLine($"WSL_GetIP: Task complete (ID: {Task.CurrentId}) Result: {strWSLIP}");
            });

            return strWSLIP.Length > 0 ? strWSLIP : string.Empty;
        }

        /// <summary>
        /// Fetches the current WSL IP (async BackGroundWorker)
        /// </summary>
        /// <returns>Success: (string) IP Address; Fail: Empty string</returns>
        /// <remarks>Ex. Usage: WSL.WSL_GetIP_BackgroundWorker((ip) => lblWSLIP.Text = $"WSL IP: {ip}");</remarks>
        public static void WSL_GetIP_BackgroundWorker(Action<string> callback) {

            // Another example usage:
            //
            // WSL_GetIPWithBackgroundWorker((ip) => {
            //    Debug.WriteLine(ip);
            // });

            BackgroundWorker worker = new BackgroundWorker();

            worker.DoWork += (sender, e) => {

                string strOutput = string.Empty;
                string strWSLIP = string.Empty;

                strOutput = Misc.RunCommand("bash.exe", string.Format("{0} {1}", "-c", "\"ifconfig eth0 | grep 'inet '\""));
                strWSLIP = IPValidation.IPv4RegEx.Match(strOutput).ToString();

                e.Result = strWSLIP.Length > 0 ? strWSLIP : string.Empty;

            };

            worker.RunWorkerCompleted += (sender, e) => {

                if (e.Error != null) {
                    // Handle errors
                } else {

                    string ip = e.Result as string;
                    callback?.Invoke(ip);

                }

            };

            worker.RunWorkerAsync();
        }

        /// <summary>
        /// Shut down WSL (async)
        /// </summary>
        /// <param name="bShowResult">[optional: default True] Shows a messagebox confirming WSL has shut down</param>
        public static void WSL_ShutDown(bool bShowResult = true) {

            if (WSL_IsRunning()) {

                if (MessageBox.Show("Are sure you want to shut WSL down?", "WSL: Shutdown", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes) {
            
                    Task task = Task.Run(() => {

                        Debug.WriteLine($"WSL_ShutDown: Task started (ID: {Task.CurrentId})");

                        // Run the shutdown command
                        Misc.RunCommand("wsl.exe", "--shutdown");

                        Debug.WriteLine($"WSL_ShutDown: Task complete (ID: {Task.CurrentId})");

                    });

                    // TODO: Add a timer here to timeout if there's a problem shutting down WSL, so this loop doesn't end up running perpetually.
                    while (WSL_IsRunning()) {
                        // Loop until WSL is confirmed to be shut down, then show them confirmation.
                        // TODO: Add some sort of user feedback here to let them know we're doin work?
                    }

                    task.Wait();

                    if (bShowResult) {
                        MessageBox.Show("WSL is shut down.", "WSL: Shutdown", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }

                }
            } else {

                // Ya'll tryna shut down somethin that ain't even runnin!
                MessageBox.Show("WSL isn't running.", "WSL: Shutdown", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        /// <summary>
        /// Start WSL (async)
        /// </summary>
        /// <param name="bShowResult">[optional: default True] Shows a messagebox confirming WSL has started</param>
        public static void WSL_Start(bool bShowResult = true) {

            if (!WSL_IsRunning()) {

                Task task = Task.Run(() =>
                {
                    Debug.WriteLine($"WSL_Start: Task started (ID: {Task.CurrentId})");

                    // Run the startup command
                    Misc.RunCommand("wsl.exe", string.Empty, false);

                    Debug.WriteLine($"WSL_Start: Task complete (ID: {Task.CurrentId})");
                });

                // TODO: Add a timer here to timeout if there's a problem starting up WSL, so this loop doesn't end up running perpetually.
                while (!WSL_IsRunning()) {
                    // Loop until WSL is confirmed to be running, then show them confirmation.
                    // TODO: Add some sort of user feedback here to let them know we're doin work?
                }

                task.Wait();

                if (bShowResult) {
                    MessageBox.Show("WSL is now running.", "WSL: Startup", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

            } else {

                // Ya'll tryna start up somethin that's already runnin!
                MessageBox.Show("WSL is already running.", "WSL: Startup", MessageBoxButtons.OK, MessageBoxIcon.Information);

            }

        }

        /// <summary>
        /// Restart WSL
        /// </summary>
        /// <param name="bShowResult">[Optional: default True] Shows a messagebox confirming WSL has restarted</param>
        public static void WSL_Restart(bool bShowResult = true) {

            // TODO: add Task code

            // Run the shutdown command TODO: need to add confirmation when run from the mini menu.
            Misc.RunCommand("wsl.exe", "--shutdown");

            Debug.WriteLine("WSL Restart: Shutdown command sent");

            while (WSL_IsRunning()) {
                // Loop until WSL is confirmed to be shut down, then start WSL again.
                // TODO: Add some sort of user feedback here to let them know we're doin work?
            }

            Debug.WriteLine("WSL Restart: WSL has shutdown");

            // Run the startup command
            Misc.RunCommand("wsl.exe", string.Empty, false);

            Debug.WriteLine("WSL Restart: Startup command sent");

            while (!WSL_IsRunning()) {
                // Loop until WSL is confirmed to be running, then show them confirmation.
                // TODO: Add some sort of user feedback here to let them know we're doin work?
            }

            Debug.WriteLine("WSL Restart: WSL has restarted");

            if (bShowResult) {
                MessageBox.Show("WSL has restarted.", "WSL: Restart", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

    }
}
