#region + -- NAMESPACE IMPORTS -- +

using Microsoft.Win32;
using NStandard;
using PortProxyGooey.Data;
using PortProxyGooey.UI;
using PortProxyGooey.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using static System.Windows.Forms.ListViewItem;

#endregion

namespace PortProxyGooey
{
    public partial class PortProxyGooey : Form
    {

        #region Variable Declarations

        private readonly ListViewColumnSorter lvwColumnSorter = new();
        public SetProxy SetProxyForm;
        public About AboutForm;
        private AppConfig AppConfig;

        #endregion

        public PortProxyGooey()
        {
            InitializeComponent();
            Font = InterfaceUtil.UiFont;

            this.Text = "PortProxyGooey  v" + Application.ProductVersion;

            listViewProxies.ListViewItemSorter = lvwColumnSorter;
        }

        private void PortProxyGUI_Load(object sender, EventArgs e)
        {
            AppConfig = Program.Database.GetAppConfig();

            Size size = AppConfig.MainWindowSize;
            Left -= (size.Width - Width) / 2;
            Top -= (size.Height - Height) / 2;
            ResetWindowSize();
        }

        private void PortProxyGUI_Shown(object sender, EventArgs e)
        {
            RefreshProxyList();

            // Sort columns based on saved config
            lvwColumnSorter.SortColumn = AppConfig.SortColumn;

            switch (AppConfig.SortOrder)
            {
                case 2:
                    lvwColumnSorter.Order = SortOrder.Descending;
                    break;
                case 1:
                    lvwColumnSorter.Order = SortOrder.Ascending;
                    break;
                case 0:
                    lvwColumnSorter.Order = SortOrder.None;
                    break;
            }

            // Perform the sort with these new sort options
            listViewProxies.Sort();
        }

        private void ResetWindowSize()
        {
            Size = AppConfig.MainWindowSize;

            if (AppConfig.PortProxyColumnWidths.Length != listViewProxies.Columns.Count)
            {
                Any.ReDim(ref AppConfig.PortProxyColumnWidths, listViewProxies.Columns.Count);
            }

            foreach ((ColumnHeader column, int configWidth) in Any.Zip(listViewProxies.Columns.OfType<ColumnHeader>(), AppConfig.PortProxyColumnWidths))
            {
                column.Width = configWidth;
            }
        }

        private Rule ParseRule(ListViewItem item)
        {
            ListViewSubItem[] subItems = item.SubItems.OfType<ListViewSubItem>().ToArray();
            int listenPort, connectPort;

            listenPort = Rule.ParsePort(subItems[3].Text);
            connectPort = Rule.ParsePort(subItems[5].Text);

            Rule rule = new()
            {
                Type = subItems[1].Text.Trim(),
                ListenOn = subItems[2].Text.Trim(),
                ListenPort = listenPort,
                ConnectTo = subItems[4].Text.Trim(),
                ConnectPort = connectPort,
                Comment = subItems[6].Text.Trim(),
                Group = item.Group?.Header.Trim(),
            };
            return rule;
        }

        private void EnableSelectedProxies()
        {
            IEnumerable<ListViewItem> items = listViewProxies.SelectedItems.OfType<ListViewItem>();
            foreach (ListViewItem item in items)
            {
                item.ImageIndex = 1;

                try
                {
                    Rule rule = ParseRule(item);
                    PortProxyUtil.AddOrUpdateProxy(rule);
                }
                catch (NotSupportedException ex)
                {
                    MessageBox.Show(ex.Message, "Exclamation", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }
            }
            PortProxyUtil.ParamChange();
        }

        private void DisableSelectedProxies()
        {
            IEnumerable<ListViewItem> items = listViewProxies.SelectedItems.OfType<ListViewItem>();
            foreach (ListViewItem item in items)
            {
                item.ImageIndex = 0;

                try
                {
                    Rule rule = ParseRule(item);
                    PortProxyUtil.DeleteProxy(rule);
                }
                catch (NotSupportedException ex)
                {
                    MessageBox.Show(ex.Message, "Exclamation", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }
            }
            PortProxyUtil.ParamChange();
        }

        /// <summary>
        /// Delete Prox(ies)
        /// </summary>
        private void DeleteSelectedProxies()
        {
            IEnumerable<ListViewItem> items = listViewProxies.SelectedItems.OfType<ListViewItem>();

            int intCount = listViewProxies.SelectedItems.Count;

            // Pluralize if necessary ;)
            string strMsg = string.Format("Delete {0} {1}?", intCount, intCount == 1 ? "proxy" : "proxies");

            if (MessageBox.Show(strMsg, "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
            {
                DisableSelectedProxies();
                Program.Database.RemoveRange(items.Select(x => new Rule { Id = x.Tag.ToString() }));
                foreach (ListViewItem item in items) listViewProxies.Items.Remove(item);
            }
        }

        private void SetProxyForUpdate(SetProxy form)
        {
            ListViewItem item = listViewProxies.SelectedItems.OfType<ListViewItem>().FirstOrDefault();

            try
            {
                Rule rule = ParseRule(item);
                form.UseUpdateMode(item, rule);
            }
            catch (NotSupportedException ex)
            {
                MessageBox.Show(ex.Message, "Exclamation", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
        }

        /// <summary>
        /// Add Groups to ListView
        /// </summary>
        /// <param name="rules"></param>
        private void InitProxyGroups(Rule[] rules)
        {
            listViewProxies.Groups.Clear();

            ListViewGroup[] groups = (
                from g in rules.GroupBy(x => x.Group)
                let name = g.Key
                where !name.IsNullOrWhiteSpace()
                orderby name
                select new ListViewGroup(name)
            ).ToArray();

            listViewProxies.Groups.AddRange(groups);
        }

        private void InitProxyItems(Rule[] rules, Rule[] proxies)
        {
            listViewProxies.Items.Clear();

            foreach (Rule rule in rules)
            {
                int imageIndex = proxies.Any(p => p.EqualsWithKeys(rule)) ? 1 : 0;
                ListViewGroup group = listViewProxies.Groups.OfType<ListViewGroup>().FirstOrDefault(x => x.Header == rule.Group);

                ListViewItem item = new();
                UpdateListViewItem(item, rule, imageIndex);
                listViewProxies.Items.Add(item);
            }
        }

        public void UpdateListViewItem(ListViewItem item, Rule rule, int imageIndex)
        {
            item.ImageIndex = imageIndex;
            item.Tag = rule.Id;
            item.SubItems.Clear();
            item.SubItems.AddRange(new[]
            {
                new ListViewSubItem(item, rule.Type),
                new ListViewSubItem(item, rule.ListenOn),
                new ListViewSubItem(item, rule.ListenPort.ToString()) { Tag = "Number" },
                new ListViewSubItem(item, rule.ConnectTo),
                new ListViewSubItem(item, rule.ConnectPort.ToString ()) { Tag = "Number" },
                new ListViewSubItem(item, rule.Comment ?? string.Empty),
            });

            if (rule.Group.IsNullOrWhiteSpace()) item.Group = null;
            else
            {
                ListViewGroup group = listViewProxies.Groups.OfType<ListViewGroup>().FirstOrDefault(x => x.Header == rule.Group);
                if (group == null)
                {
                    group = new ListViewGroup(rule.Group);
                    listViewProxies.Groups.Add(group);
                }
                item.Group = group;
            }
        }

        public void RefreshProxyList()
        {
            listViewProxies.Cursor = Cursors.WaitCursor;

            Rule[] proxies = PortProxyUtil.GetProxies();
            Rule[] rules = Program.Database.Rules.ToArray();
            foreach (Rule proxy in proxies)
            {
                Rule matchedRule = rules.FirstOrDefault(r => r.EqualsWithKeys(proxy));
                proxy.Id = matchedRule?.Id;
            }

            IEnumerable<Rule> pendingAdds = proxies.Where(x => x.Valid && x.Id == null);
            IEnumerable<Rule> pendingUpdates =
                from proxy in proxies
                let exsist = rules.FirstOrDefault(r => r.Id == proxy.Id)
                where exsist is not null
                where proxy.Valid && proxy.Id is not null
                select proxy;

            Program.Database.AddRange(pendingAdds);
            Program.Database.UpdateRange(pendingUpdates);

            rules = Program.Database.Rules.ToArray();
            InitProxyGroups(rules);
            InitProxyItems(rules, proxies);

            listViewProxies.Cursor = Cursors.Default;
        }

        /// <summary>
        /// Context-Menu Actions
        /// </summary>
        private void contextMenuStrip_RightClick_MouseClick(object sender, MouseEventArgs e)
        {
            if (sender is ContextMenuStrip strip)
            {
                ToolStripMenuItem selected = strip.Items.OfType<ToolStripMenuItem>().Where(x => x.Selected).FirstOrDefault();
                if (selected is null || !selected.Enabled) return;

                switch (selected)
                {
                    case ToolStripMenuItem item when item == toolStripMenuItem_Enable: EnableSelectedProxies(); break;
                    case ToolStripMenuItem item when item == toolStripMenuItem_Disable: DisableSelectedProxies(); break;

                    // New Item
                    case ToolStripMenuItem item when item == toolStripMenuItem_New:
                        NewItem();
                        break;

                    // Clone Item
                    case ToolStripMenuItem item when item == toolStripMenuItem_Clone:
                        MessageBox.Show("TODO");
                        break;

                    // Modify/Edit Item
                    case ToolStripMenuItem item when item == toolStripMenuItem_Modify:
                        if (SetProxyForm == null) SetProxyForm = new SetProxy(this);
                        SetProxyForUpdate(SetProxyForm);
                        SetProxyForm.ShowDialog();
                        break;

                    // Refresh List
                    case ToolStripMenuItem item when item == toolStripMenuItem_Refresh:
                        RefreshProxyList();
                        break;

                    // Clear List (Delete All)
                    case ToolStripMenuItem item when item == clearToolStripMenuItem:
                        MessageBox.Show("TODO");
                        break;

                    // Delete Item(s)
                    case ToolStripMenuItem item when item == toolStripMenuItem_Delete: DeleteSelectedProxies(); break;

                    // About
                    case ToolStripMenuItem item when item == toolStripMenuItem_About:
                        if (AboutForm == null)
                        {
                            AboutForm = new About(this);
                            AboutForm.Show();
                        }
                        else AboutForm.Show();
                        break;
                }
            }
        }

        /// <summary>
        /// Add a new item to the proxy list
        /// </summary>
        private void NewItem()
        {
            if (SetProxyForm == null) SetProxyForm = new SetProxy(this);
            SetProxyForm.UseNormalMode();
            SetProxyForm.ShowDialog();
        }

        private void listViewProxies_MouseUp(object sender, MouseEventArgs e)
        {
            if (sender is ListView listView)
            {
                ListViewHitTestInfo hit = listViewProxies.HitTest(e.Location);

                // If a ListView item was clicked ...
                if (hit.Item != null)
                {
                    // Fetch which colum the user clicked in
                    int columnindex = hit.Item.SubItems.IndexOf(hit.SubItem);

                    // If it's the first column (checkbox icon column) then toggle it's state
                    if (columnindex == 0)
                    {
                        if (hit.Item.ImageIndex == 1)
                        {
                            DisableSelectedProxies();
                        }
                        else
                        {
                            EnableSelectedProxies();
                        }
                    }
                }
                else
                {
                    if (e.Button == MouseButtons.Left)
                    {
                        // Left-licking an empty space opens the New Item dialog
                        NewItem();
                    }
                }

                int intCount = listViewProxies.SelectedItems.Count;

                // Set enabled state of toolstrip items based on the selected item's image index
                toolStripMenuItem_Enable.Enabled = e.Button == MouseButtons.Right && listView.SelectedItems.OfType<ListViewItem>().Any(x => x.ImageIndex == 0);
                toolStripMenuItem_Disable.Enabled = e.Button == MouseButtons.Right && listView.SelectedItems.OfType<ListViewItem>().Any(x => x.ImageIndex == 1);

                // Add count to label if more than 1
                if (toolStripMenuItem_Enable.Enabled && intCount > 1) { toolStripMenuItem_Enable.Text = string.Format("Enable ({0})", intCount); }
                if (toolStripMenuItem_Disable.Enabled && intCount > 1) { toolStripMenuItem_Disable.Text = string.Format("Disable ({0})", intCount); }

                // Add count to let users know how many they're about to nuke
                if (intCount > 1) toolStripMenuItem_Delete.Text = string.Format("Delete ({0})", intCount);

                toolStripMenuItem_Delete.Enabled = e.Button == MouseButtons.Right && listView.SelectedItems.OfType<ListViewItem>().Any();
                toolStripMenuItem_Modify.Enabled = e.Button == MouseButtons.Right && listView.SelectedItems.OfType<ListViewItem>().Count() == 1;
            }
        }

        /// <summary>
        /// Double-clicking an item in the list of proxies opens up it's Edit/Modify form
        /// </summary>
        private void listViewProxies_DoubleClick(object sender, EventArgs e)
        {
            if (sender is ListView listView)
            {
                bool selectAny = listView.SelectedItems.OfType<ListViewItem>().Any();
                if (selectAny)
                {
                    if (SetProxyForm == null) SetProxyForm = new SetProxy(this);
                    SetProxyForUpdate(SetProxyForm);
                    SetProxyForm.ShowDialog();
                }
            }
        }

        private void listViewProxies_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            // Determine if clicked column is already the column that is being sorted.
            if (e.Column == lvwColumnSorter.SortColumn)
            {
                // Reverse the current sort direction for this column.
                lvwColumnSorter.Order = lvwColumnSorter.Order == SortOrder.Ascending ? SortOrder.Descending : SortOrder.Ascending;
            }
            else
            {
                // Set the column number that is to be sorted; default to ascending.
                lvwColumnSorter.SortColumn = e.Column;
                lvwColumnSorter.Order = SortOrder.Ascending;
            }
            // LEFT OFF: Making sure the sorting saves and restores, as well as the handlers here are all ok
            if (AppConfig is not null && sender is ListView listView)
            {
                AppConfig.SortColumn = lvwColumnSorter.SortColumn;
                AppConfig.SortOrder = (int)lvwColumnSorter.Order;
            }

            // Perform the sort with these new sort options.
            listViewProxies.Sort();
        }

        /// <summary>
        /// HotKeys / Shortcuts
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="keyData"></param>
        /// <returns></returns>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                // Enable Item
                case Keys.Control | Keys.E:
                    if (listViewProxies.SelectedItems.Count > 0) { EnableSelectedProxies(); }
                    return true;

                // Disable Item
                case Keys.Control | Keys.D:
                    if (listViewProxies.SelectedItems.Count > 0) { DisableSelectedProxies(); }
                    return true;

                // Delete Item(s)
                case Keys.Delete:
                    if (listViewProxies.SelectedItems.Count > 0) { DeleteSelectedProxies(); }
                    return true;

                // Add New Item
                case Keys.Insert:
                    NewItem();
                    return true;

                // FlushDNS
                case Keys.Control | Keys.F:
                    DnsUtil.FlushCache();
                    return true;

                // Clear List
                case Keys.Control | Keys.C:
                    // TODO
                    return true;

                // Edit Proxy
                case Keys.F2:
                    if (listViewProxies.SelectedItems.Count > 0)
                    {
                        if (SetProxyForm == null) SetProxyForm = new SetProxy(this);
                        SetProxyForUpdate(SetProxyForm);
                        SetProxyForm.ShowDialog();
                    }
                    return true;

                // Refresh List
                case Keys.F5:
                    RefreshProxyList();
                    return true;

                default:
                    return base.ProcessCmdKey(ref msg, keyData);
            }
        }

        private void listViewProxies_ColumnWidthChanged(object sender, ColumnWidthChangedEventArgs e)
        {
            if (AppConfig is not null && sender is ListView listView)
            {
                AppConfig.PortProxyColumnWidths[e.ColumnIndex] = listView.Columns[e.ColumnIndex].Width;
            }
        }

        private void PortProxyGUI_FormClosing(object sender, FormClosingEventArgs e)
        {
            Program.Database.SaveAppConfig(AppConfig);
        }

        private void PortProxyGUI_Resize(object sender, EventArgs e)
        {
            if (AppConfig is not null && sender is Form form)
            {
                AppConfig.MainWindowSize = form.Size;
            }
        }

        /// <summary>
        /// Central function to open an URL
        /// </summary>
        /// <param name="strURL">URL to open</param>
        private void LaunchURL(string strURL)
        {
            ProcessStartInfo psi = new()
            {
                FileName = strURL,
                UseShellExecute = true
            };

            Process.Start(psi);
        }

        #region Import / Export

        /// <summary>
        /// Exports current list of proxies to .db
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripMenuItem_Export_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new();

            // Autogenerate a name they can use if they want
            string t2 = DateTime.Now.ToString("MM-dd-yyyy-hh-mm-ss");
            string strGen = "PortProxyGooey-" + t2;

            saveFileDialog.Title = "Export Current Proxy List ...";
            saveFileDialog.Filter = "db files (*.db)|*.db";
            saveFileDialog.FileName = strGen;

            bool intErrCheck = false;

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    File.Copy(ApplicationDbScope.AppDbFile, saveFileDialog.FileName, true);
                }
                catch (Exception ex)
                {
                    intErrCheck = true;
                    Debug.WriteLine("Save File issue: " + ex.Message);
                }

                // Give some user feedback
                if (intErrCheck == true)
                {
                    MessageBox.Show("Couldnt Export Proxy List", "Oops", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    MessageBox.Show("Proxy List Exported", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        /// <summary>
        /// Imports saved proxy list from .db
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripMenuItem_Import_Click(object sender, EventArgs e)
        {

            using OpenFileDialog openFileDialog = new();
            openFileDialog.Title = "Import Proxy List ...";
            openFileDialog.Filter = "db files (*.db)|*.db";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {

                if (MessageBox.Show("Overwrite current list with the selected list? " + openFileDialog.FileName, "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
                {
                    using (ApplicationDbScope scope = ApplicationDbScope.FromFile(openFileDialog.FileName))
                    {
                        foreach (Data.Rule rule in scope.Rules)
                        {
                            Data.Rule exsist = Program.Database.GetRule(rule.Type, rule.ListenOn, rule.ListenPort);
                            if (exsist is null)
                            {
                                rule.Id = Guid.NewGuid().ToString();
                                Program.Database.Add(rule);
                            }
                        }
                    }
                    // TODO: Could be nice to add success/failure mbox here.
                    RefreshProxyList();
                }
            }
        }

        #endregion

        private void toolStripMenuItem_ResetWindowSize_Click(object sender, EventArgs e)
        {
            AppConfig = new AppConfig();
            ResetWindowSize();
        }

        #region External Apps

        /// <summary>
        /// External App: Network Adapters
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void adaptersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("explorer", "/e,::{26EE0668-A00A-44D7-9371-BEB064C98683}\\0\\::{7007ACC7-3202-11D1-AAD2-00805FC1270E}");
        }

        /// <summary>
        /// External App: Windows Firewall Control (Rules Panel)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void rulesPanelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WFC("-rp");
        }

        /// <summary>
        /// External App: Windows Firewall Control (Connections Log)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void connectionsPanelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WFC("-cl");
        }

        /// <summary>
        /// External App: Windows Firewall Control
        /// </summary>
        /// <param name="strPanel">The Panel to open</param>
        private void WFC(string strPanel)
        {

            try
            {

                RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Windows Firewall Control");

                if (key != null)
                {
                    Process.Start(key.GetValue("InstallationPath").ToString(), strPanel);
                    key.Close();
                }
                else
                {
                    // If path to WFC isnt found in the registry, as a courtesy, launch the website for them to dl it, if they want.
                    LaunchURL("https://www.binisoft.org/wfc");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error in WFC(): " + ex.Message);
            }
        }

        /// <summary>
        /// External App: Windows Firewall (Basic)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void basicToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start("control", "firewall.cpl");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error Launching firewall.cpl: " + ex.Message);
            }
        }

        /// <summary>
        /// External App: Windows Firewall (Advanced)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void advancedToolStripMenuItem_Click(object sender, EventArgs e)
        {

            // Were gonna do it this way so the ugly command window doesnt show before opening the fw app.
            var p = new Process();

            {
                var withBlock = p.StartInfo;
                withBlock.Verb = "runas";
                withBlock.RedirectStandardOutput = true;
                withBlock.RedirectStandardError = true;
                withBlock.FileName = "cmd";
                withBlock.Arguments = "/C wf.msc";
                withBlock.UseShellExecute = false;
                withBlock.CreateNoWindow = true;
                withBlock.WorkingDirectory = Environment.GetEnvironmentVariable("WINDIR") + "\\System32";
            }

            p.Start();
        }

        #endregion

        /// <summary>
        /// FlushDNS
        /// </summary>
        private void toolStripMenuItem_FlushDnsCache_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Flush DNS?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
            {
                DnsUtil.FlushCache();
            }
        }

        /// <summary>
        /// Resets the counts on the right-click context menu when the menu closes
        /// </summary>
        private void contextMenuStrip_RightClick_Closed(object sender, ToolStripDropDownClosedEventArgs e)
        {
            toolStripMenuItem_Enable.Text = "Enable";
            toolStripMenuItem_Disable.Text = "Disable";
            toolStripMenuItem_Delete.Text = "Delete";
        }

        #region Clipboard & View Cline

        /// <summary>
        /// Copies netsh add command to clipboard
        /// </summary>
        private void NetSHAddCopyToClipboard_Click(object sender, EventArgs e)
        {
            AdvNetsh(1, 1);
        }

        /// <summary>
        /// Shows netsh add command in a messagebox
        /// </summary>
        private void NetSHAddViewCline_Click(object sender, EventArgs e)
        {
            AdvNetsh(2, 1);
        }

        /// <summary>
        /// Copy delete cmd to clipboard
        /// </summary>
        private void NetSHDelCopyToClipboard_Click(object sender, EventArgs e)
        {
            AdvNetsh(1, 2);
        }

        /// <summary>
        /// View delete command in msgbox
        /// </summary>
        private void NetSHDelViewCline_Click(object sender, EventArgs e)
        {
            AdvNetsh(2, 2);
        }

        /// <summary>
        /// Either shows or copies to clipboard the actual netsh cline for the selected item
        /// </summary>
        /// <param name="intType">1 = Clipboard; else = View</param>
        /// <param name="intCmd">2 = delete; else = add</param>
        private void AdvNetsh(int intType, int intCmd)
        {
            try
            {
                string strCmd = intCmd == 2 ? "delete" : "add";
                string strMessage = string.Empty;

                // Message to copy/show
                if (intCmd == 2)
                {
                    // Delete
                    strMessage = string.Format(
                                                "netsh interface portproxy {3} {0} listenaddress={1} listenport={2}",
                                                listViewProxies.FocusedItem.SubItems[1].Text,
                                                listViewProxies.FocusedItem.SubItems[2].Text,
                                                listViewProxies.FocusedItem.SubItems[3].Text,
                                                strCmd);
                }
                else
                {
                    // Add
                    strMessage = string.Format(
                                                "netsh interface portproxy {5} {0} listenaddress={1} listenport={2} connectaddress={3} connectport={4}",
                                                listViewProxies.FocusedItem.SubItems[1].Text,
                                                listViewProxies.FocusedItem.SubItems[2].Text,
                                                listViewProxies.FocusedItem.SubItems[3].Text,
                                                listViewProxies.FocusedItem.SubItems[4].Text,
                                                listViewProxies.FocusedItem.SubItems[5].Text,
                                                strCmd);
                }

                // Action to do (copy to clipboard or messagebox)
                if (intType == 1)
                {
                    Clipboard.SetText(strMessage);
                }
                else
                {
                    MessageBox.Show(strMessage, "netsh " + strCmd, MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine(string.Format("netsh {1}: {2} error: {0}", ex.Message, intCmd, intType == 1 ? "Copy" : "View"));
                throw;
            }
        }

        #endregion

    }
}
