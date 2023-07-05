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
//using System.ServiceProcess;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.ListViewItem;
//using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using ListView = System.Windows.Forms.ListView;

#endregion

namespace PortProxyGooey {

    public partial class PortProxyGooey : Form {

        #region + -- VAR DECLARATIONS -- +

        // Easy place to change info app-wide as needed
        internal static readonly string strAppURL = "https://github.com/STaRDoGG/PortProxyGUI";
        internal static readonly string strWFCURL = "https://www.binisoft.org/wfc";

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

            this.Text = $"Port Proxy Gooey v{Application.ProductVersion}";

            listViewProxies.ListViewItemSorter = lvwColumnSorter;
        }

        private void PortProxyGUI_Load(object sender, EventArgs e) {

            // TEST AREA



            // END

            // Get the resource manager for your application.
            ResourceManager rm = Properties.Resources.ResourceManager;

            // Retrieve the image resource by name.
            // TODO: https://stackoverflow.com/questions/4416934/c-how-to-make-a-picture-background-transparent
            Image img = (Image)rm.GetObject("decoration");

            //e.Graphics.DrawImage(img, 50, 50, 100, 100);

            //Debug.WriteLine(PortProxyUtil.CheckPortOpen("::1", 3568)); // bad
            //Debug.WriteLine(PortProxyUtil.CheckPortOpen("::1", 443));  // good
            //Debug.WriteLine(PortProxyUtil.CheckPortOpen("24.14.15.201", 3568)); // bad
            //Debug.WriteLine(PortProxyUtil.CheckPortOpen("24.14.15.201", 443));  // good
            //Debug.WriteLine(PortProxyUtil.CheckPortOpen("0.0.0.0", 443)); // good (returns false though. is it because 0.0.0.0 isnt a real ip?)
            //Debug.WriteLine(PortProxyUtil.CheckPortOpen("127.0.0.1", 443)); // good

            //JSE_Utils.WSL.WSL_GetVersion();

            Debug.WriteLine("Network Available?: " + Network.IsNetworkAvailable());

            //Debug.WriteLine(Services.GetInfo(PortProxyUtil.ServiceName));

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

            // Update labels
            UpdateProxyCount();
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
        /// <param name="bEnable">[optional: default True] True: Enable selected; False: Disable selected.</param>
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

            UpdateProxyCount();
            Services.ParamChangeWinAPI(PortProxyUtil.ServiceName);
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
                string strMsg = $"Delete {intCount} {(intCount == 1 ? "proxy" : "proxies")}?";

                if (MessageBox.Show(strMsg, "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.No)
                    return;

            }

            ToggleSelectedProxies(false);
            Program.Database.RemoveRange(items.Select(x => new Rule { Id = x.Tag.ToString() }));
            foreach (ListViewItem item in items)
                listViewProxies.Items.Remove(item);

            RefreshProxyList();
            UpdateProxyCount();
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


                //int itemCount = Listview.CountItemsInGroup(listViewProxies, group);
                //Debug.WriteLine($"Number of items in group ({group}): {itemCount}");


            }


            // Left off: getting proxy count for group headers; need to figure out how to fetch by group name? Or just use it's index?. Is this the best place for it?



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

        }

        /// <summary>
        /// Adds all group labels to the Move menu
        /// </summary>
        /// <param name="lvGroups">The list of group names</param>
        private void CreateGroupsMenuItem(ListViewGroup[] lvGroups) {

            // First, clear out any previous ones in case we're renaming a group, refreshing this list, or something, so we don't get dupes.
            foreach (ToolStripMenuItem item in toolStripMenuItem_MoveTo.DropDownItems.OfType<ToolStripMenuItem>().ToList()) {
                toolStripMenuItem_MoveTo.DropDownItems.Remove(item);
            }

            // Add a "New" item
            // Default group doesn't get added to the ListViewGroup[]; manually add it.
            ToolStripMenuItem toolStripMenuItem_Move = new ToolStripMenuItem("New ...");
            toolStripMenuItem_Move.Click += ToolStripMenuItem_Move_Click;
            toolStripMenuItem_MoveTo.DropDownItems.Add(toolStripMenuItem_Move);
            toolStripMenuItem_Move.Image = Properties.Resources._new;
            toolStripMenuItem_Move.ToolTipText = "Move proxy(s) to a new group";

            // Add a seperator
            ToolStripSeparator toolStripSeparatorNewGroup = new ToolStripSeparator();
            toolStripMenuItem_MoveTo.DropDownItems.Add(toolStripSeparatorNewGroup);

            // Default group doesn't get added to the ListViewGroup[]; manually add it.
            toolStripMenuItem_Move = new ToolStripMenuItem("Default");
            toolStripMenuItem_Move.Click += ToolStripMenuItem_Move_Click;
            toolStripMenuItem_MoveTo.DropDownItems.Add(toolStripMenuItem_Move);
            toolStripMenuItem_Move.Image = Properties.Resources.add;
            toolStripMenuItem_Move.ToolTipText = "Move selected proxy(s) to the Default group";

            // Add all the other groups
            foreach (ListViewGroup header in lvGroups) {

                toolStripMenuItem_Move = new ToolStripMenuItem(header.Header);
                toolStripMenuItem_Move.Click += ToolStripMenuItem_Move_Click;
                toolStripMenuItem_MoveTo.DropDownItems.Add(toolStripMenuItem_Move);

                // Give it an icon for some of the well known things
                if (header.ToString().ToLower().Contains("docker")) {
                    toolStripMenuItem_Move.Image = Properties.Resources.docker_1;
                } else if (header.ToString().ToLower().Contains("wsl")) {
                    toolStripMenuItem_Move.Image = Properties.Resources.wsl;
                }

                // Set any other properties of the menu item
                //toolStripMenuItem_Move.ToolTipText = string.Format("Move proxy(s) to the {0} group", header.Header);

            }
        }

        /// <summary>
        /// Updates the proxy count label and tooltip
        /// </summary>
        public void UpdateProxyCount() {

            lblProxyCount.Text = listViewProxies.Items.Count.ToString();
            tTipPPG.SetToolTip(lblProxyCount, string.Format("Total Proxies: {0}{3}Enabled: {1}{3}Disabled: {2}", listViewProxies.Items.Count, intEnabled, intDisabled, Environment.NewLine));

        }

        public void RefreshProxyList() {

            listViewProxies.Cursor = Cursors.WaitCursor;

            // Fetch all proxies stored in the registry
            Rule[] proxies = PortProxyUtil.GetProxies();

            // Fetch all proxies from db, store in an array.
            Rule[] rules = Program.Database.Rules.ToArray();

            // Compare list of registry proxies with list of proxies stored in our db (?)
            foreach (Rule proxy in proxies) {

                Rule matchedRule = rules.FirstOrDefault(r => r.EqualsWithKeys(proxy));
                proxy.Id = matchedRule?.Id;

            }

            //
            IEnumerable<Rule> pendingAdds = proxies.Where(x => x.Valid && x.Id == null);
            IEnumerable<Rule> pendingUpdates =
                from proxy in proxies
                let exist = rules.FirstOrDefault(r => r.Id == proxy.Id)
                where exist is not null
                where proxy.Valid && proxy.Id is not null
                select proxy;

            Program.Database.AddRange(pendingAdds);
            Program.Database.UpdateRange(pendingUpdates);

            // Fetch all proxies from db (again)
            rules = Program.Database.Rules.ToArray();

            InitProxyGroups(rules);
            InitProxyItems(rules, proxies);

            // LEFT OFF: Now that I can get the count, need to add it to the actual header as well as handle other areas to remove the count as necessary
            foreach (ToolStripMenuItem item in toolStripMenuItem_MoveTo.DropDownItems.OfType<ToolStripMenuItem>().ToList()) {
                int itemCount = Listview.CountItemsInGroup(listViewProxies, item.Text);
                Debug.WriteLine($"Number of items in group ({item.Text}): {itemCount}");
            }

            listViewProxies.Cursor = Cursors.Default;

        }

        /// <summary>
        /// Context-Menu Actions
        /// </summary>
        private void contextMenuStrip_RightClick_MouseClick(object sender, MouseEventArgs e) {

            if (sender is ContextMenuStrip strip) {

                ToolStripMenuItem selected = strip.Items.OfType<ToolStripMenuItem>().FirstOrDefault(x => x.Selected);

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

                        AboutForm ??= new About(this);

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

            if (listViewProxies.Items.Count > 0) {

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

            } else {
                MessageBox.Show("Nothing to clear, dumdum.", "But hey ... you tried. *shrug*", MessageBoxButtons.OK, MessageBoxIcon.Information);
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

                GroupRename();

                ListViewHitTestInfo hit = listViewProxies.HitTest(e.Location);

                // If a ListView item was clicked ...
                if (hit.Item != null) {

                    // Fetch which column the user clicked in
                    int columnindex = hit.Item.SubItems.IndexOf(hit.SubItem);

                    // If it's the first column (checkbox icon column) then toggle it's state
                    if (columnindex == 0) {
                        ToggleSelectedProxies(hit.Item.ImageIndex != 1);
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

                // Enable/Disable (state): Set enabled state of toolstrip items based on the selected item's image index
                toolStripMenuItem_Enable.Enabled = e.Button == MouseButtons.Right && listView.SelectedItems.OfType<ListViewItem>().Any(x => x.ImageIndex == 0);
                toolStripMenuItem_Disable.Enabled = e.Button == MouseButtons.Right && listView.SelectedItems.OfType<ListViewItem>().Any(x => x.ImageIndex == 1);

                // Enable/Disable (count): Add count to label if more than 1
                if (toolStripMenuItem_Enable.Enabled && intCount > 1) { toolStripMenuItem_Enable.Text = $"Enable ({intCount})"; }
                if (toolStripMenuItem_Disable.Enabled && intCount > 1) { toolStripMenuItem_Disable.Text = $"Disable ({intCount})"; }

                // Delete/Move (count): Add count to let users know how many they're about to nuke (or move)
                if (intCount > 1) {

                    toolStripMenuItem_Delete.Text = $"Delete ({intCount})";
                    toolStripMenuItem_MoveTo.Text = $"Group: Move ({intCount}) to ...";

                }

                toolStripMenuItem_Delete.Enabled = e.Button == MouseButtons.Right && listView.SelectedItems.OfType<ListViewItem>().Any();
                toolStripMenuItem_Modify.Enabled = e.Button == MouseButtons.Right && listView.SelectedItems.OfType<ListViewItem>().Count() == 1;
                toolStripMenuItem_Clone.Enabled = e.Button == MouseButtons.Right && listView.SelectedItems.OfType<ListViewItem>().Count() == 1;

                // Clear List: Only visible if Items exist in the list
                clearToolStripMenuItem.Visible = e.Button == MouseButtons.Right && listView.SelectedItems.OfType<ListViewItem>().Count() == 1;

                // Move: Only visible if Items exist in the list (along with the seperator so we don't get ugly double-seperators)
                toolStripMenuItem_MoveTo.Visible = e.Button == MouseButtons.Right && listView.SelectedItems.OfType<ListViewItem>().Count() == 1;
                toolStripSeparator1.Visible = e.Button == MouseButtons.Right && listView.SelectedItems.OfType<ListViewItem>().Count() == 1;

                // NETSH Menu
                NetSHToolStripMenuItem.Enabled = e.Button == MouseButtons.Right && listView.SelectedItems.OfType<ListViewItem>().Count() == 1;

                // Registry Key
                registryKeyToolStripMenuItem.Enabled = e.Button == MouseButtons.Right && listView.SelectedItems.OfType<ListViewItem>().Count() == 1;

                // WSL
                if (WSL.IsRunning()) {

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
        /// Renames a group by editing all items in that group in the db to match the newly entered name.
        /// </summary>
        /// <param name="bRename"></param>
        private void GroupRename(bool bRename = false) {
            // TODO: need to finish when items are in the Default group....
            ListView.SelectedListViewItemCollection selectedItems = listViewProxies.SelectedItems;

            if (selectedItems.Count > 0) {

                ListViewItem selectedItem = selectedItems[0];
                ListViewGroup group = selectedItem.Group;

                if (group != null) {

                    string groupHeader = group.Header;

                    toolStripMenuItem_RenameGroup.Visible = true;

                    if (bRename) {

                        string input = groupHeader;

                        // Can't be the same as current, or empty.
                        if (Dialogs.InputDialog(ref input, "New group name:") == DialogResult.OK && input != groupHeader && !string.IsNullOrEmpty(input)) {

                            // Rename it in the db
                            Rule rule = ParseRule(selectedItem);
                            Program.Database.RenameGroup(rule, input.Trim());

                            RefreshProxyList();

                        }

                    }

                } else {

                    toolStripMenuItem_RenameGroup.Visible = false;
                    Debug.WriteLine("Item in Default Group");

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

                    Network.DNS.FlushCache();
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

            Debug.WriteLine($"Resized to: [w: {Width} h: {Height}]");
        }

        #region + -- IMPORT / EXPORT -- +

        /// <summary>
        /// Exports current list of proxies to .db
        /// </summary>
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

                    File.Copy(ApplicationDbScope.AppDB, saveFileDialog.FileName, true);

                } catch (Exception ex) {

                    intErrCheck = true;
                    Debug.WriteLine($"Save File issue: {ex.Message}");

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

                if (MessageBox.Show($"Overwrite current list with the selected list? {openFileDialog.FileName}", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes) {

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
                    MessageBox.Show($"{intAdded} rules imported.", "Import Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        #endregion

        private void toolStripMenuItem_ResetWindowSize_Click(object sender, EventArgs e) {

            if (MessageBox.Show("Are sure you want to reset the window?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes) {

                AppConfig = new AppConfig();
                ResetWindowSize();

            }

        }

        #region + -- EXTERNAL APPS -- +

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
                    PortProxyUtil.Launch(strWFCURL);

                }

            } catch (Exception ex) {
                Debug.WriteLine($"Error in WFC(): {ex.Message}");
            }
        }

        /// <summary>
        /// External App: Windows Firewall (Basic)
        /// </summary>
        private void basicToolStripMenuItem_Click(object sender, EventArgs e) {
            try {
                PortProxyUtil.Launch("control", "firewall.cpl");
            } catch (Exception ex) {
                Debug.WriteLine($"Error Launching firewall.cpl: {ex.Message}");
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

            if (Network.DNS.FlushCache(true, false)) {

                Audio.PlaySound("D:\\Coding\\Repos\\PortProxyGUI\\PortProxyGUI\\Resources\\audio\\Flush.wav");
                //TODO: Audio.PlaySound(Properties.Resources.Flush);

                // Note: I could use the built-in confirmation dialog in FlushCache(), but it does a "ding" system sound,
                //       which conflicts with the .wav, so I'm playing the .wav first, THEN let it ding. Slightly nicer.
                MessageBox.Show("DNS Flushed!", "Whoosh", MessageBoxButtons.OK, MessageBoxIcon.Information);

            }

        }

        /// <summary>
        /// Resets the counts on the right-click context menu when the menu closes
        /// </summary>
        private void contextMenuStrip_RightClick_Closed(object sender, ToolStripDropDownClosedEventArgs e) {

            toolStripMenuItem_Enable.Text = "Enable";
            toolStripMenuItem_Disable.Text = "Disable";
            toolStripMenuItem_Delete.Text = "Delete";
            toolStripMenuItem_MoveTo.Text = "Group: Move to ...";

        }

        #region + -- CLIPBOARD / VIEW CLINE -- +

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
                    MessageBox.Show(strMessage, $"netsh {strCmd}", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

            } catch (Exception ex) {
                Debug.WriteLine($"netsh {intCmd}: {(intType == 1 ? "Copy" : "View")} error: {ex.Message}");
                throw;
            }
        }

        #endregion

        /// <summary>
        /// Open Regedit to the specified proxy 'Type'
        /// </summary>
        private void registryKeyToolStripMenuItem_Click(object sender, EventArgs e) {

            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Applets\Regedit", true);

            if (key != null) {
                key.SetValue("LastKey", $"HKEY_LOCAL_MACHINE\\{PortProxyUtil.GetKeyName(listViewProxies.SelectedItems[0].SubItems[1].Text)}", RegistryValueKind.String);
                key.Close();
            }

            Process.Start("regedit.exe");
        }

        private void portForwardingTesterToolStripMenuItem_Click(object sender, EventArgs e) {
            Network.Link_OpenPortTester();
        }

        #region + -- DOUBLE-CLICKING ALL OF THESE OPENS NEW ITEM DIALOG  -- +

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
            WSL.IsRunning(true);
        }

        private void WSLShutDownToolStripMenuItem_Click(object sender, EventArgs e) {
            WSL.ShutDown();
        }

        /// <summary>
        /// WSL Mini Menu: Shutdown
        /// </summary>
        private void toolStripMenuItemWSLShutDown_Click(object sender, EventArgs e) {
            WSL.ShutDown();
        }

        private void WSLStartToolStripMenuItem_Click(object sender, EventArgs e) {
            WSL.Start();
        }

        private void WSLRestartToolStripMenuItem_Click(object sender, EventArgs e) {
            WSL.Restart();
        }

        /// <summary>
        /// WSL Mini Menu: Restart
        /// </summary>
        private void toolStripMenuItemWSLRestart_Click(object sender, EventArgs e) {
            WSL.Restart();
        }

        #endregion

        #region + -- DOCKER -- +

        private void picDocker_Click(object sender, EventArgs e) {
            contextMenuStrip_Docker.Show(Cursor.Position);
        }

        #endregion

        private void tmrCheck_Tick(object sender, EventArgs e) {

            //_ = UpdateAsyncTasks();

            // Keep Current Local Machine & WSL IPs shown to help alert of any potential changes

            // Fetch all Local IPs
            List<string> lstIPs = Network.GetLocalIPAddress();

            // Get another list of those IPs, excluding the 1st one
            List<string> lstAltIPs = lstIPs.GetRange(1, lstIPs.Count - 1);

            // Join the 'extra' local IPs
            string strAltIPs = string.Join(Environment.NewLine, lstAltIPs);

            lblCurrentLocalIP.Text = $"LOCAL IP: {(lstIPs.Count > 0 ? lstIPs[0] : "N/A")}";

            // If any "alternate/other" IP's let's add them to the tooltip
            if (lstAltIPs.Count > 0) {
                tTipPPG.SetToolTip(lblCurrentLocalIP, $"Other Reported IPs:{Environment.NewLine}{strAltIPs}");
            }

            // + ----- WSL

            WSL.GetIP_BackgroundWorker((ip) => lblWSLIP.Text = $"WSL IP: {ip}");
            tTipPPG.SetToolTip(lblWSLIP, "Double-click to copy IP to clipboard");

            // -----

            // Keep WSL status updated
            WSL.IsRunning_BackgroundWorker((result) => {

                if (result) {
                    picWSLStatus.Image = Properties.Resources.green;
                    picWSLStatus.Tag = "1";
                    tTipPPG.SetToolTip(picWSLStatus, "WSL: RUNNING");
                    picWSL.Visible = true;
                } else {
                    picWSLStatus.Image = Properties.Resources.red;
                    picWSLStatus.Tag = "0";
                    tTipPPG.SetToolTip(picWSLStatus, "WSL: N/A");
                    picWSL.Visible = false;
                }

            }, false);
            
            
            // + ----- DOCKER

            // Keep Docker status updated
            if (Docker.IsRunning()) {
                picDockerStatus.Image = Properties.Resources.green;
                Docker.GetVersion_BackgroundWorker((DockerVersion) => tTipPPG.SetToolTip(picDockerStatus, $"DOCKER{(!string.IsNullOrEmpty(DockerVersion) ? " (v" + (DockerVersion) + ")" : string.Empty)}: RUNNING"));
                picDocker.Visible = true;
            } else {
                picDockerStatus.Image = Properties.Resources.red;
                tTipPPG.SetToolTip(picDockerStatus, "DOCKER: N/A");
                picDocker.Visible = false;
            }

            // + ----- SERVICE

            // Keep IpHlpSvc status updated
            Services.IsRunning_BackgroundWorker((result) => {

                if (result) {
                    picIpHlpSvcStatus.Image = Properties.Resources.green;
                    tTipPPG.SetToolTip(picIpHlpSvcStatus, $"{PortProxyUtil.ServiceFriendlyName.ToUpper()} SERVICE: RUNNING");
                } else {
                    picIpHlpSvcStatus.Image = Properties.Resources.red;
                    tTipPPG.SetToolTip(picIpHlpSvcStatus, $"{PortProxyUtil.ServiceFriendlyName.ToUpper()} SERVICE: N/A{Environment.NewLine}Click icon to Start it");
                }

            }, PortProxyUtil.ServiceName, false);

        }

        private async Task UpdateAsyncTasks() {

            //string ip = await WSL.WSL_GetIPAsync();
            lblWSLIP.Text = $"WSL IP: {await WSL.GetIP_Task_Async()}";

        }


        private void ToolStripMenuItem_Move_Click(object sender, EventArgs e) {
            // LEFT OFF: Need to add the actual MOVE code now ...
            // Pluralize if necessary ;)
            //string strMsg = string.Format("Delete {0} {1}?", intCount, intCount == 1 ? "proxy" : "proxies");

            // If a New group is chosen to be created
            if (sender.ToString() == "New ...") {
                GroupRename(true);
            }

            Debug.WriteLine("Move To: " + sender.ToString());

            IEnumerable<ListViewItem> items = listViewProxies.SelectedItems.OfType<ListViewItem>();

            foreach (ListViewItem item in items) {

                Debug.WriteLine("Current: " + item.Group);

                try {

                    Rule rule = ParseRule(item);



                } catch (NotSupportedException ex) {

                    MessageBox.Show(ex.Message, "Scramble the jets!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;

                }
            }
            Services.ParamChange(PortProxyUtil.ServiceName);
        }

        private void toolStripMenuItem_Exit_Click(object sender, EventArgs e) {
            Application.Exit();
        }

        private void toolStripMenuItem_RenameGroup_Click(object sender, EventArgs e) {
            GroupRename(true);
        }

        /// <summary>
        /// "Restart" IpHlpSvc. Technically not a real service restart, but more of a param reload.
        /// </summary>
        private void toolStripMenuItem_ReloadIpHlpSvc_Click(object sender, EventArgs e) {

            // Source: https://github.com/swagfin/PortProxyGUI/commit/af6cfdcafe66883def4a9305149c6a48afbfb8f9

            try {
                //Services.ParamChange(PortProxyUtil.ServiceName);
                Services.ParamChangeWinAPI(PortProxyUtil.ServiceName);
            } catch (Exception ex) {
                MessageBox.Show(ex.Message, $"{PortProxyUtil.ServiceFriendlyName} Restart Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            MessageBox.Show($"{PortProxyUtil.ServiceFriendlyName} Restarted", $"{PortProxyUtil.ServiceFriendlyName} Restart", MessageBoxButtons.OK, MessageBoxIcon.Information);

        }

        private void picIpHlpSvcStatus_Click(object sender, EventArgs e) {

            // First, check if the service is already running; we only want to do something if it's not.
            Services.IsRunning_BackgroundWorker((result) => {

                if (!result) {

                    // Start the service
                    //Debug.WriteLine($"{PortProxyUtil.ServiceName}: Starting");  

                    Services.Start_BackgroundWorker((result) => {
                        //Debug.WriteLine(result);
                    }, PortProxyUtil.ServiceName);

                }

            }, PortProxyUtil.ServiceName, false);

        }

        /// <summary>
        /// Copies the Local Machine's IP(s) to the clipboard (ignoring if no IP available)
        /// </summary>
        private void lblCurrentLocalIP_DoubleClick(object sender, EventArgs e) {

            // Copy the IP (only) to the clipboard
            if (!lblCurrentLocalIP.Text.Contains("N/A")) {
                Clipboard.SetText(lblCurrentLocalIP.Text.Replace("LOCAL IP: ", ""));
            }

        }

        /// <summary>
        /// Copies the WSL IP to the clipboard (ignoring if no IP available)
        /// </summary>
        private void lblWSLIP_DoubleClick(object sender, EventArgs e) {

            // Copy the IP (only) to the clipboard
            if (!lblWSLIP.Text.Contains("N/A")) {
                Clipboard.SetText(lblWSLIP.Text.Replace("WSL IP: ", ""));
            }

        }

        private void picWSLStatus_Click(object sender, EventArgs e) {

            if (picWSLStatus.Tag.ToString() == "1") {
                // TODO: do this w/ the ones in the advanced menu as well
                toolStripMenuItemWSLStart.Visible = false;
                toolStripMenuItemWSLRestart.Visible = true;
                toolStripMenuItemWSLShutDown.Visible = true;

            } else {

                toolStripMenuItemWSLStart.Visible = true;
                toolStripMenuItemWSLRestart.Visible = false;
                toolStripMenuItemWSLShutDown.Visible = false;
            }

            contextMenuStrip_WSL.Show(Cursor.Position);

        }
    }
}
