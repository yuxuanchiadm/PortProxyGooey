#region + -- IMPORTS -- +

using JSE_Utils;
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
using System.Resources;
using System.Windows.Forms;
using static System.Windows.Forms.ListViewItem;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;
using ListView = System.Windows.Forms.ListView;

#endregion

namespace PortProxyGooey {

    public partial class PortProxyGooey : Form {

        #region + -- VAR DECLARATIONS -- +

        // Easy place to change app info app-wide as needed
        public const string strAppURL = "https://github.com/STaRDoGG/PortProxyGUI";

        private readonly ListViewColumnSorter lvwColumnSorter = new();
        public SetProxy SetProxyForm;
        public About AboutForm;
        private AppConfig AppConfig;
        private DateTime ClickStartTime;
        private int ClickCount;

        // Keep track of enable/disabled proxies
        private int intEnabled = 0;
        private int intDisabled = 0;

        #endregion

        public PortProxyGooey() {

            InitializeComponent();
            Font = InterfaceUtil.UiFont;

            this.Text = string.Format("Port Proxy Gooey  v{0}", Application.ProductVersion);

            listViewProxies.ListViewItemSorter = lvwColumnSorter;
        }

        private void PortProxyGUI_Load(object sender, EventArgs e) {

            // Get the resource manager for your application.
            ResourceManager rm = Properties.Resources.ResourceManager;

            // Retrieve the image resource by name.
            Image img = (Image)rm.GetObject("decoration");

            //e.Graphics.DrawImage(img, 50, 50, 100, 100);

            //Debug.WriteLine(PortProxyUtil.CheckPortOpen("::1", 3568)); // bad
            //Debug.WriteLine(PortProxyUtil.CheckPortOpen("::1", 443));  // good
            //Debug.WriteLine(PortProxyUtil.CheckPortOpen("24.14.15.201", 3568)); // bad
            //Debug.WriteLine(PortProxyUtil.CheckPortOpen("24.14.15.201", 443));  // good
            //Debug.WriteLine(PortProxyUtil.CheckPortOpen("0.0.0.0", 443)); // good (returns false though. is it because 0.0.0.0 isnt a real ip?)
            //Debug.WriteLine(PortProxyUtil.CheckPortOpen("127.0.0.1", 443)); // good

            //JSE_Utils.WSL.WSL_GetVersion();

            AppConfig = Program.Database.GetAppConfig();

            // Set Main Window Size from saved settings
            Size size = AppConfig.MainWindowSize;
            this.Left -= (size.Width - Width) / 2;
            this.Top -= (size.Height - Height) / 2;
            ResetWindowSize();

            // Set Main Window Location from saved settings
            if (AppConfig.MainWindowLocationX != 0 && AppConfig.MainWindowLocationY != 0) {

                this.StartPosition = FormStartPosition.Manual;
                this.Left = AppConfig.MainWindowLocationX;
                this.Top = AppConfig.MainWindowLocationY;

            }
        }

        private void PortProxyGUI_Shown(object sender, EventArgs e) {

            RefreshProxyList();

            // Sort columns based on saved config
            lvwColumnSorter.SortColumn = AppConfig.SortColumn;

            switch (AppConfig.SortOrder) {

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

        private void ResetWindowSize() {

            Size = AppConfig.MainWindowSize;

            if (AppConfig.PortProxyColumnWidths.Length != listViewProxies.Columns.Count) {
                Any.ReDim(ref AppConfig.PortProxyColumnWidths, listViewProxies.Columns.Count);
            }

            foreach ((ColumnHeader column, int configWidth) in Any.Zip(listViewProxies.Columns.OfType<ColumnHeader>(), AppConfig.PortProxyColumnWidths)) {
                column.Width = configWidth;
            }

        }

        private static Rule ParseRule(ListViewItem item) {

            ListViewSubItem[] subItems = item.SubItems.OfType<ListViewSubItem>().ToArray();
            int listenPort, connectPort;

            listenPort = Rule.ParsePort(subItems[3].Text);
            connectPort = Rule.ParsePort(subItems[5].Text);

            Rule rule = new() {

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

        /// <summary>
        /// Enable/Disable selected Proxies
        /// </summary>
        /// <param name="bEnable">[Optional: default True]True: Enable selected; False: Disable selected.</param>
        private void ToggleSelectedProxies(bool bEnable = true) {
            // TODO: working on the label tip counter(s)
            IEnumerable<ListViewItem> items = listViewProxies.SelectedItems.OfType<ListViewItem>();

            foreach (ListViewItem item in items) {

                if (bEnable) {

                    item.ImageIndex = 1;

                    intEnabled++;
                    intDisabled--; // might be wrong

                } else {

                    item.ImageIndex = 0;

                    intEnabled--;
                    intDisabled++;
                }

                try {

                    Rule rule = ParseRule(item);

                    if (bEnable) {
                        PortProxyUtil.AddOrUpdateProxy(rule);
                    } else {
                        PortProxyUtil.DeleteProxy(rule);
                    }

                } catch (NotSupportedException ex) {

                    MessageBox.Show(ex.Message, "Scramble the jets!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;

                }
            }
            PortProxyUtil.ParamChange();
        }

        /// <summary>
        /// Delete Prox(ies)
        /// </summary>
        /// <param name="intShowConfirmation">[optional] display a confirmation first: 1 = Yes, 0 = No (default)</param>
        private void DeleteSelectedProxies(int intShowConfirmation = 0) {

            IEnumerable<ListViewItem> items = listViewProxies.SelectedItems.OfType<ListViewItem>();

            if (intShowConfirmation == 1) {

                int intCount = listViewProxies.SelectedItems.Count;

                // Pluralize if necessary ;)
                string strMsg = string.Format("Delete {0} {1}?", intCount, intCount == 1 ? "proxy" : "proxies");

                if (MessageBox.Show(strMsg, "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.No)
                    return;

            }

            ToggleSelectedProxies(false);
            Program.Database.RemoveRange(items.Select(x => new Rule { Id = x.Tag.ToString() }));
            foreach (ListViewItem item in items)
                listViewProxies.Items.Remove(item);
            RefreshProxyList();
        }

        private void SetProxyForUpdateOrClone(SetProxy form, bool bclone = false) {

            ListViewItem item = listViewProxies.SelectedItems.OfType<ListViewItem>().FirstOrDefault();

            try {

                Rule rule = ParseRule(item);

                if (bclone) {

                    // Clone rule
                    form.UseNormalMode(rule);

                } else {

                    // Modify rule
                    form.UseUpdateMode(item, rule);

                }

            } catch (NotSupportedException ex) {
                MessageBox.Show(ex.Message, "Ayy Now!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
        }

        /// <summary>
        /// Add Groups to ListView
        /// </summary>
        /// <param name="rules"></param>
        private void InitProxyGroups(Rule[] rules) {

            listViewProxies.Groups.Clear();

            ListViewGroup[] groups = (
                from g in rules.GroupBy(x => x.Group)
                let name = g.Key
                where !name.IsNullOrWhiteSpace()
                orderby name
                select new ListViewGroup(name)
            ).ToArray();

            listViewProxies.Groups.AddRange(groups);

            // Add group names to menu item
            CreateGroupsMenuItem(groups);
        }

        private void InitProxyItems(Rule[] rules, Rule[] proxies) {

            // Reset when updating the list
            intEnabled = 0;
            intDisabled = 0;

            listViewProxies.Items.Clear();

            foreach (Rule rule in rules) {

                int imageIndex = proxies.Any(p => p.EqualsWithKeys(rule)) ? 1 : 0;

                ListViewGroup group = listViewProxies.Groups.OfType<ListViewGroup>().FirstOrDefault(x => x.Header == rule.Group);

                ListViewItem item = new();
                UpdateListViewItem(item, rule, imageIndex);
                listViewProxies.Items.Add(item);

            }

        }

        public void UpdateListViewItem(ListViewItem item, Rule rule, int imageIndex) {

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

            if (rule.Group.IsNullOrWhiteSpace())
                item.Group = null;
            else {

                ListViewGroup group = listViewProxies.Groups.OfType<ListViewGroup>().FirstOrDefault(x => x.Header == rule.Group);

                if (group == null) {

                    group = new ListViewGroup(rule.Group);
                    listViewProxies.Groups.Add(group);

                }

                item.Group = group;
            }

            // Keep track of how many enabled/disabled
            if (imageIndex == 1) {
                intEnabled++;
            } else {
                intDisabled++;
            }

            UpdateProxyCountToolTip();

        }

        /// <summary>
        /// Adds all group labels to the Move menu
        /// </summary>
        /// <param name="lvGroups">The list of group names</param>
        private void CreateGroupsMenuItem(ListViewGroup[] lvGroups) {

            // Default group doesn't get added to the ListViewGroup[]; manually add it.
            ToolStripMenuItem toolStripMenuItem_Move = new ToolStripMenuItem("Default");
            toolStripMenuItem_Move.Click += ToolStripMenuItem_Move_Click;
            moveToToolStripMenuItem.DropDownItems.Add(toolStripMenuItem_Move);

            //toolStripMenuItem_Move.ToolTipText = "Move selected proxy(s) to the Default group";

            // Add all the other groups
            foreach (ListViewGroup header in lvGroups) {

                toolStripMenuItem_Move = new ToolStripMenuItem(header.Header);
                toolStripMenuItem_Move.Click += ToolStripMenuItem_Move_Click;
                moveToToolStripMenuItem.DropDownItems.Add(toolStripMenuItem_Move);

                // Set any other properties of the menu item
                //toolStripMenuItem_Move.ToolTipText = string.Format("Move proxy(s) to the {0} group", header.Header);
            }
        }

        private void UpdateProxyCountToolTip() {

            lblProxyCount.Text = listViewProxies.Items.Count.ToString();

            // LEFT OFF: setting the ttip. Everything seems to be working here EXCEPT when I toggle en/disabled on a proxy; then the tip isnt being updated cuz this func isnt called.
            // Total count is off.
            tTipPPG.SetToolTip(lblProxyCount, string.Format("Total Proxies: {0}{3}Enabled: {1}{3}Disabled: {2}", listViewProxies.Items.Count, intEnabled, intDisabled, Environment.NewLine));

        }

        public void RefreshProxyList() {

            listViewProxies.Cursor = Cursors.WaitCursor;

            Rule[] proxies = PortProxyUtil.GetProxies();
            Rule[] rules = Program.Database.Rules.ToArray();

            foreach (Rule proxy in proxies) {

                Rule matchedRule = rules.FirstOrDefault(r => r.EqualsWithKeys(proxy));
                proxy.Id = matchedRule?.Id;

            }

            IEnumerable<Rule> pendingAdds = proxies.Where(x => x.Valid && x.Id == null);
            IEnumerable<Rule> pendingUpdates =
                from proxy in proxies
                let exist = rules.FirstOrDefault(r => r.Id == proxy.Id)
                where exist is not null
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
        private void contextMenuStrip_RightClick_MouseClick(object sender, MouseEventArgs e) {

            if (sender is ContextMenuStrip strip) {

                ToolStripMenuItem selected = strip.Items.OfType<ToolStripMenuItem>().Where(x => x.Selected).FirstOrDefault();

                if (selected is null || !selected.Enabled)
                    return;

                contextMenuStrip_RightClick.Visible = false;

                switch (selected) {

                    case ToolStripMenuItem item when item == toolStripMenuItem_Enable:

                        ToggleSelectedProxies(true);
                        break;

                    case ToolStripMenuItem item when item == toolStripMenuItem_Disable:

                        ToggleSelectedProxies(false);
                        break;

                    // New Item
                    case ToolStripMenuItem item when item == toolStripMenuItem_New:

                        NewItem();
                        break;

                    // Clone Item
                    case ToolStripMenuItem item when item == toolStripMenuItem_Clone:

                        ModOrClone(true);
                        break;

                    // Modify/Edit Item
                    case ToolStripMenuItem item when item == toolStripMenuItem_Modify:

                        ModOrClone();
                        break;

                    // Refresh List
                    case ToolStripMenuItem item when item == toolStripMenuItem_Refresh:

                        RefreshProxyList();
                        break;

                    // Clear All Proxies
                    case ToolStripMenuItem item when item == clearToolStripMenuItem:

                        ClearProxies();
                        break;

                    // Delete Item(s)
                    case ToolStripMenuItem item when item == toolStripMenuItem_Delete:

                        DeleteSelectedProxies(1);
                        break;

                    // About
                    case ToolStripMenuItem item when item == toolStripMenuItem_About:

                        if (AboutForm == null) {
                            AboutForm = new About(this);
                        }

                        AboutForm.Show();
                        break;
                }
            }
        }

        /// <summary>
        /// Are we modifying a proxy or cloning it?
        /// </summary>
        /// <param name="bClone">[Optional] True = Clone; else = Modify</param>
        private void ModOrClone(bool bClone = false) {

            SetProxyForm ??= new SetProxy(this);
            SetProxyForUpdateOrClone(SetProxyForm, bClone);
            SetProxyForm.ShowDialog();

        }

        /// <summary>
        /// Deletes ALL proxies in the list
        /// </summary>
        private void ClearProxies() {

            if (MessageBox.Show(
                        string.Format("This will literally delete:{0}{0}- EVERY PROXY{0}- All Groups{0}- All Comments{0}{0}that you have in this list and on your machine and start from scratch.{0}{0}Proceed?", Environment.NewLine),
                        "* FOCUS! *",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question,
                        MessageBoxDefaultButton.Button2
                        ) == DialogResult.Yes) {

                // Select all in the list and delete
                listViewProxies.Items.Cast<ListViewItem>().ToList().ForEach(item => item.Selected = true);
                DeleteSelectedProxies();
            }
        }

        /// <summary>
        /// Add a new item to the proxy list
        /// </summary>
        private void NewItem() {

            SetProxyForm ??= new SetProxy(this);
            SetProxyForm.UseNormalMode();
            SetProxyForm.ShowDialog();

        }

        private void listViewProxies_MouseUp(object sender, MouseEventArgs e) {

            if (sender is ListView listView) {

                ListViewHitTestInfo hit = listViewProxies.HitTest(e.Location);

                // If a ListView item was clicked ...
                if (hit.Item != null) {

                    // Fetch which column the user clicked in
                    int columnindex = hit.Item.SubItems.IndexOf(hit.SubItem);

                    // If it's the first column (checkbox icon column) then toggle it's state
                    if (columnindex == 0) {

                        if (hit.Item.ImageIndex == 1) {
                            ToggleSelectedProxies(false);
                        } else {
                            ToggleSelectedProxies(true);
                        }

                    }

                } else {

                    if (e.Button == MouseButtons.Left) {

                        ClickCount++;

                        if (ClickCount == 1) {

                            ClickStartTime = DateTime.Now;

                        } else if (ClickCount == 2) {

                            ClickCount = 0;

                            DateTime endTime = DateTime.Now;
                            TimeSpan elapsed = endTime - ClickStartTime;
                            double elapsedMilliseconds = elapsed.TotalMilliseconds;

                            if (elapsedMilliseconds < SystemInformation.DoubleClickTime) {

                                // Double-clicking an empty space opens the New Item dialog
                                NewItem();

                            }
                        }
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
                if (intCount > 1)
                    toolStripMenuItem_Delete.Text = string.Format("Delete ({0})", intCount);

                toolStripMenuItem_Delete.Enabled = e.Button == MouseButtons.Right && listView.SelectedItems.OfType<ListViewItem>().Any();
                toolStripMenuItem_Modify.Enabled = e.Button == MouseButtons.Right && listView.SelectedItems.OfType<ListViewItem>().Count() == 1;
                toolStripMenuItem_Clone.Enabled = e.Button == MouseButtons.Right && listView.SelectedItems.OfType<ListViewItem>().Count() == 1;

                // NETSH Menu
                NetSHToolStripMenuItem.Enabled = e.Button == MouseButtons.Right && listView.SelectedItems.OfType<ListViewItem>().Count() == 1;

                // Registry Key
                registryKeyToolStripMenuItem.Enabled = e.Button == MouseButtons.Right && listView.SelectedItems.OfType<ListViewItem>().Count() == 1;

                // WSL
                if (WSL.WSL_IsRunning()) {

                    WSLShutDownToolStripMenuItem.Enabled = true;
                    WSLRestartToolStripMenuItem.Enabled = true;
                    WSLStartToolStripMenuItem.Enabled = false;

                } else {

                    WSLStartToolStripMenuItem.Enabled = true;
                    WSLShutDownToolStripMenuItem.Enabled = false;
                    WSLRestartToolStripMenuItem.Enabled = false;
                }
            }
        }

        /// <summary>
        /// Double-clicking an item in the list of proxies opens up it's Edit/Modify form
        /// </summary>
        private void listViewProxies_DoubleClick(object sender, EventArgs e) {

            if (sender is ListView listView) {

                bool selectAny = listView.SelectedItems.OfType<ListViewItem>().Any();

                if (selectAny) {

                    SetProxyForm ??= new SetProxy(this);
                    SetProxyForUpdateOrClone(SetProxyForm);
                    SetProxyForm.ShowDialog();

                }

            }
        }

        private void listViewProxies_ColumnClick(object sender, ColumnClickEventArgs e) {

            // Determine if clicked column is already the column that is being sorted.
            if (e.Column == lvwColumnSorter.SortColumn) {

                // Reverse the current sort direction for this column.
                lvwColumnSorter.Order = lvwColumnSorter.Order == SortOrder.Ascending ? SortOrder.Descending : SortOrder.Ascending;

            } else {

                // Set the column number that is to be sorted; default to ascending.
                lvwColumnSorter.SortColumn = e.Column;
                lvwColumnSorter.Order = SortOrder.Ascending;

            }

            // Store them
            if (AppConfig is not null && sender is ListView) {

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
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData) {

            switch (keyData) {

                // Enable Item
                case Keys.Control | Keys.E:

                    if (listViewProxies.SelectedItems.Count > 0) { ToggleSelectedProxies(true); }
                    return true;

                // Disable Item
                case Keys.Control | Keys.D:

                    if (listViewProxies.SelectedItems.Count > 0) { ToggleSelectedProxies(false); }
                    return true;

                // Delete Item(s)
                case Keys.Delete:

                    if (listViewProxies.SelectedItems.Count > 0) { DeleteSelectedProxies(1); }
                    return true;

                // Add New Item
                case Keys.Insert:

                    NewItem();
                    return true;

                // FlushDNS
                case Keys.Control | Keys.F:

                    PortProxyUtil.FlushCache();
                    return true;

                // Clear All Proxies
                case Keys.Control | Keys.C:

                    ClearProxies();
                    return true;

                // Edit Proxy
                case Keys.F2:

                    if (listViewProxies.SelectedItems.Count > 0) { ModOrClone(); }
                    return true;

                // Clone Proxy
                case Keys.F3:

                    if (listViewProxies.SelectedItems.Count > 0) { ModOrClone(true); }
                    return true;

                // Refresh List
                case Keys.F5:

                    RefreshProxyList();
                    return true;

                default:
                    return base.ProcessCmdKey(ref msg, keyData);
            }
        }

        private void listViewProxies_ColumnWidthChanged(object sender, ColumnWidthChangedEventArgs e) {
            if (AppConfig is not null && sender is ListView listView) {
                AppConfig.PortProxyColumnWidths[e.ColumnIndex] = listView.Columns[e.ColumnIndex].Width;
            }
        }

        /// <summary>
        /// Saves all current configs
        /// </summary>
        private void PortProxyGUI_FormClosing(object sender, FormClosingEventArgs e) {

            // Save Main Window Location
            AppConfig.MainWindowLocationX = this.Location.X;
            AppConfig.MainWindowLocationY = this.Location.Y;

            // Save everything else
            Program.Database.SaveAppConfig(AppConfig);
        }

        private void PortProxyGUI_Resize(object sender, EventArgs e) {

            if (AppConfig is not null && sender is Form form)
                AppConfig.MainWindowSize = form.Size;

            Debug.WriteLine(string.Format("w: {0} h: {1}", this.Width, this.Height));
        }

        #region + -- IMPORT / EXPORT -- +

        /// <summary>
        /// Exports current list of proxies to .db
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripMenuItem_Export_Click(object sender, EventArgs e) {

            SaveFileDialog saveFileDialog = new();

            // Autogenerate a name they can use if they want
            string t2 = DateTime.Now.ToString("MM-dd-yyyy-hh-mm-ss");
            string strGen = "PortProxyGooey-" + t2;

            saveFileDialog.Title = "Export Current Proxy List ...";
            saveFileDialog.Filter = "db files (*.db)|*.db";
            saveFileDialog.FileName = strGen;

            bool intErrCheck = false;

            if (saveFileDialog.ShowDialog() == DialogResult.OK) {

                try {

                    File.Copy(ApplicationDbScope.AppDbFile, saveFileDialog.FileName, true);

                } catch (Exception ex) {

                    intErrCheck = true;
                    Debug.WriteLine(string.Format("Save File issue: {0}", ex.Message));

                }

                // Give some user feedback
                if (intErrCheck == true) {
                    MessageBox.Show("Couldn't Export Proxy List", "Oops", MessageBoxButtons.OK, MessageBoxIcon.Error);
                } else {
                    MessageBox.Show("Proxy List Exported", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        /// <summary>
        /// Imports saved proxy list from .db
        /// </summary>
        private void toolStripMenuItem_Import_Click(object sender, EventArgs e) {

            using OpenFileDialog openFileDialog = new();
            openFileDialog.Title = "Import Proxy List ...";
            openFileDialog.Filter = "db files (*.db)|*.db";
            int intAdded = 0;

            if (openFileDialog.ShowDialog() == DialogResult.OK) {

                if (MessageBox.Show(string.Format("Overwrite current list with the selected list? {0}", openFileDialog.FileName), "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes) {

                    using (ApplicationDbScope scope = ApplicationDbScope.FromFile(openFileDialog.FileName)) {

                        foreach (Rule rule in scope.Rules) {

                            Rule exist = Program.Database.GetRule(rule.Type, rule.ListenOn, rule.ListenPort);

                            if (exist is null) {

                                rule.Id = Guid.NewGuid().ToString();
                                Program.Database.Add(rule);
                                intAdded++;

                            }
                        }
                    }

                    RefreshProxyList();
                    MessageBox.Show(string.Format("{0} rules imported.", intAdded), "Import Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        #endregion

        private void toolStripMenuItem_ResetWindowSize_Click(object sender, EventArgs e) {

            AppConfig = new AppConfig();
            ResetWindowSize();

        }

        #region External Apps

        /// <summary>
        /// External App: Network Adapters
        /// </summary>
        private void adaptersToolStripMenuItem_Click(object sender, EventArgs e) {
            Process.Start("explorer", "/e,::{26EE0668-A00A-44D7-9371-BEB064C98683}\\0\\::{7007ACC7-3202-11D1-AAD2-00805FC1270E}");
        }

        /// <summary>
        /// External App: Windows Firewall Control (Rules Panel)
        /// </summary>
        private void rulesPanelToolStripMenuItem_Click(object sender, EventArgs e) {
            WFC("-rp");
        }

        /// <summary>
        /// External App: Windows Firewall Control (Connections Log)
        /// </summary>
        private void connectionsPanelToolStripMenuItem_Click(object sender, EventArgs e) {
            WFC("-cl");
        }

        /// <summary>
        /// External App: Windows Firewall Control
        /// </summary>
        /// <param name="strPanel">The Panel to open</param>
        private static void WFC(string strPanel) {

            try {

                RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Windows Firewall Control");

                if (key != null) {

                    Process.Start(key.GetValue("InstallationPath").ToString(), strPanel);
                    key.Close();

                } else {

                    // If path to WFC isnt found in the registry, as a courtesy, launch the website for them to dl it, if they want.
                    PortProxyUtil.Launch("https://www.binisoft.org/wfc");

                }

            } catch (Exception ex) {
                Debug.WriteLine(string.Format("Error in WFC(): {0}", ex.Message));
            }
        }

        /// <summary>
        /// External App: Windows Firewall (Basic)
        /// </summary>
        private void basicToolStripMenuItem_Click(object sender, EventArgs e) {
            try {
                PortProxyUtil.Launch("control", "firewall.cpl");
            } catch (Exception ex) {
                Debug.WriteLine(string.Format("Error Launching firewall.cpl: {0}", ex.Message));
            }
        }

        /// <summary>
        /// External App: Windows Firewall (Advanced)
        /// </summary>
        private void advancedToolStripMenuItem_Click(object sender, EventArgs e) {
            // TODO: Merge this w/PortProxyUtil.Launch() to remove some redundancy?
            // We're gonna do it this way so the ugly command window doesn't show before opening the fw app.
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
                //withBlock.WorkingDirectory = Path.Combine(Environment.GetEnvironmentVariable("WINDIR"), "\\System32"); // this ignores the first path
                withBlock.WorkingDirectory = Environment.GetEnvironmentVariable("WINDIR") + "\\System32";
            }

            p.Start();
        }

        #endregion

        /// <summary>
        /// FlushDNS
        /// </summary>
        private void toolStripMenuItem_FlushDnsCache_Click(object sender, EventArgs e) {

            if (MessageBox.Show("Flush DNS?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes) {
                PortProxyUtil.FlushCache();
            }

        }

        /// <summary>
        /// Resets the counts on the right-click context menu when the menu closes
        /// </summary>
        private void contextMenuStrip_RightClick_Closed(object sender, ToolStripDropDownClosedEventArgs e) {

            toolStripMenuItem_Enable.Text = "Enable";
            toolStripMenuItem_Disable.Text = "Disable";
            toolStripMenuItem_Delete.Text = "Delete";

        }

        #region Clipboard & View Cline

        /// <summary>
        /// Copies netsh add command to clipboard
        /// </summary>
        private void NetSHAddCopyToClipboard_Click(object sender, EventArgs e) {
            AdvNetsh(1, 1);
        }

        /// <summary>
        /// Shows netsh add command in a messagebox
        /// </summary>
        private void NetSHAddViewCline_Click(object sender, EventArgs e) {
            AdvNetsh(2, 1);
        }

        /// <summary>
        /// Copy delete cmd to clipboard
        /// </summary>
        private void NetSHDelCopyToClipboard_Click(object sender, EventArgs e) {
            AdvNetsh(1, 2);
        }

        /// <summary>
        /// View delete command in msgbox
        /// </summary>
        private void NetSHDelViewCline_Click(object sender, EventArgs e) {
            AdvNetsh(2, 2);
        }

        /// <summary>
        /// Either shows or copies to clipboard the actual netsh cline for the selected item
        /// </summary>
        /// <param name="intType">1 = Clipboard; else = View</param>
        /// <param name="intCmd">2 = delete; else = add</param>
        private void AdvNetsh(int intType, int intCmd) {
            try {
                string strCmd = intCmd == 2 ? "delete" : "add";
                string strMessage = string.Empty;

                // Message to copy/show
                if (intCmd == 2) {
                    // Delete
                    strMessage = string.Format(
                                                "netsh interface portproxy {3} {0} listenaddress={1} listenport={2}",
                                                listViewProxies.FocusedItem.SubItems[1].Text,
                                                listViewProxies.FocusedItem.SubItems[2].Text,
                                                listViewProxies.FocusedItem.SubItems[3].Text,
                                                strCmd);
                } else {
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
                if (intType == 1) {
                    Clipboard.SetText(strMessage);
                } else {
                    MessageBox.Show(strMessage, "netsh " + strCmd, MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

            } catch (Exception ex) {
                Debug.WriteLine(string.Format("netsh {1}: {2} error: {0}", ex.Message, intCmd, intType == 1 ? "Copy" : "View"));
                throw;
            }
        }

        #endregion

        /// <summary>
        /// Open Regedit to the specified Type
        /// </summary>
        private void registryKeyToolStripMenuItem_Click(object sender, EventArgs e) {

            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Applets\Regedit", true);

            if (key != null) {
                key.SetValue("LastKey", string.Format("HKEY_LOCAL_MACHINE\\{0}", PortProxyUtil.GetKeyName(listViewProxies.SelectedItems[0].SubItems[1].Text)), RegistryValueKind.String);
                key.Close();
            }

            Process.Start("regedit.exe");
        }

        private void portForwardingTesterToolStripMenuItem_Click(object sender, EventArgs e) {
            PortProxyUtil.Launch("https://www.yougetsignal.com/tools/open-ports");
        }

        #region + -- Double-Clicking All of These Opens New Item Dialog  -- +

        private void lblGooey_DoubleClick(object sender, EventArgs e) {
            NewItem();
        }

        private void lblPP_DoubleClick(object sender, EventArgs e) {
            NewItem();
        }

        private void lblJSE_DoubleClick(object sender, EventArgs e) {
            NewItem();
        }

        private void lblProxyCount_DoubleClick(object sender, EventArgs e) {
            NewItem();
        }

        private void PortProxyGooey_DoubleClick(object sender, EventArgs e) {
            NewItem();
        }

        private void pnlBar_DoubleClick(object sender, EventArgs e) {
            NewItem();
        }

        #endregion

        private void winsockResetToolStripMenuItem_Click(object sender, EventArgs e) {
            Winsock.Reset();
        }

        #region + -- WSL -- +

        private void WSLRunningToolStripMenuItem_Click(object sender, EventArgs e) {
            WSL.WSL_IsRunning(true);
        }

        private void WSLShutDownToolStripMenuItem_Click(object sender, EventArgs e) {
            WSL.WSL_ShutDown();
        }

        /// <summary>
        /// WSL Mini Menu: Shutdown
        /// </summary>
        private void toolStripMenuItemWSLShutDown_Click(object sender, EventArgs e) {
            WSL.WSL_ShutDown();
        }

        private void WSLStartToolStripMenuItem_Click(object sender, EventArgs e) {
            WSL.WSL_Start();
        }

        private void WSLRestartToolStripMenuItem_Click(object sender, EventArgs e) {
            WSL.WSL_Restart();
        }

        /// <summary>
        /// WSL Mini Menu: Restart
        /// </summary>
        private void toolStripMenuItemWSLRestart_Click(object sender, EventArgs e) {
            WSL.WSL_Restart();
        }

        private void picWSL_Click(object sender, EventArgs e) {
            contextMenuStrip_WSL.Show(Cursor.Position);
        }

        #endregion

        private void tmrCheck_Tick(object sender, EventArgs e) {

            // Always keep the WSL status icon updated
            picWSL.Visible = WSL.WSL_IsRunning();

        }


        private void ToolStripMenuItem_Move_Click(object sender, EventArgs e) {
            // LEFT OFF: Need to add the actual MOVE code now ...
            // Pluralize if necessary ;)
            //string strMsg = string.Format("Delete {0} {1}?", intCount, intCount == 1 ? "proxy" : "proxies");

            Debug.WriteLine(sender.ToString());
        }

    }
}
