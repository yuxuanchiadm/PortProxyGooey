#region + -- NAMESPACE IMPORTS -- +

using NStandard;
using PortProxyGUI.Data;
using PortProxyGUI.Utils;
using System;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
//using System.Web.UI.WebControls;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using Rule = PortProxyGUI.Data.Rule;

#endregion

// NOTE: I think this entire form isn't really "needed"; it could all be done right from the listview itself on the main form.
// TODO: 1) After adding a range of ports, unchecking, then checking, the count in the label gets set to 1 even though the range is larger.
//       2) I added a range of 45 to 54 and label said 10?
//       3) 0.0.0.0 in both fields, w/port 53 is saying dupe, even though that specifically isn't in the list.
namespace PortProxyGUI
{
    public partial class SetProxy : Form
    {
        #region + -- VAR DECLARATIONS -- +

        public readonly PortProxyGUI ParentWindow;
        private string AutoTypeString { get; }

        private bool _updateMode;
        private ListViewItem _listViewItem;
        private Rule _itemRule;

        #endregion

        public SetProxy(PortProxyGUI parent)
        {
            ParentWindow = parent;

            InitializeComponent();
            Font = InterfaceUtil.UiFont;

            AutoTypeString = comboBox_Type.Text = comboBox_Type.Items.OfType<string>().First();

            string[] groupNames = (
                from g in parent.listViewProxies.Groups.OfType<ListViewGroup>()
                let header = g.Header
                where !header.IsNullOrWhiteSpace()
                select header
            ).ToArray();

            comboBox_Group.Items.AddRange(groupNames);
        }

        public void UseNormalMode()
        {
            _updateMode = false;
            _listViewItem = null;
            _itemRule = null;

            comboBox_Type.Text = AutoTypeString;
            comboBox_Group.Text = String.Empty;

            comboBox_ListenOn.Text = "*";
            textBox_ListenPort.Text = String.Empty;
            comboBox_ConnectTo.Text = String.Empty;
            textBox_ConnectPort.Text = String.Empty;
            textBox_Comment.Text = String.Empty;
        }

        public void UseUpdateMode(ListViewItem item, Rule rule)
        {
            _updateMode = true;
            _listViewItem = item;

            _itemRule = rule;

            comboBox_Type.Text = rule.Type;
            comboBox_Group.Text = rule.Group;

            comboBox_ListenOn.Text = rule.ListenOn;
            textBox_ListenPort.Text = rule.ListenPort.ToString();
            comboBox_ConnectTo.Text = rule.ConnectTo;
            textBox_ConnectPort.Text = rule.ConnectPort.ToString();
            textBox_Comment.Text = rule.Comment;
        }

        private bool IsIPv6(string ip)
        {
            // Scott Note: Is this correct? Looks more like a MAC Address regex? Added a better regex below.
            //return ip.IsMatch(new Regex(@"^[\dABCDEF]{2}(?::(?:[\dABCDEF]{2})){5}$"));
            return ip.IsMatch(new Regex(@"^(([0-9a-fA-F]{1,4}:){7}([0-9a-fA-F]{1,4}|:))|(([0-9a-fA-F]{1,4}:){0,6}(:[0-9a-fA-F]{1,4}){1,6})$"));
        }

        private string GetPassType(string listenOn, string connectTo)
        {
            string from = IsIPv6(listenOn) ? "v6" : "v4";
            string to = IsIPv6(connectTo) ? "v6" : "v4";
            return $"{from}to{to}";
        }

        private void button_Set_Click(object sender, EventArgs e)
        {
            int listenPort, listenPortRange, connectPort;

            // Don't let em funk with anything while we're doin work
            this.Enabled = false;
            progBarRange.Visible = true;

            // Validate the Ports
            try
            {
                listenPort = Rule.ParsePort(textBox_ListenPort.Text);
                connectPort = Rule.ParsePort(textBox_ConnectPort.Text);
                listenPortRange = Rule.ParsePort(textBox_ListenPortRange.Text);
            }
            catch (NotSupportedException ex)
            {
                MessageBox.Show(ex.Message, "Invalid port", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                this.Enabled = true;
                return;
            }

            // If adding a range ...
            if (chkBox_ListenPortRange.Checked && listenPortRange < listenPort)
            {
                MessageBox.Show("Ending Port is LOWER than the Starting Port", "You need to fix this:", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

                // Set focus of the 'offending' error target to better help the user understand the source of the issue.
                textBox_ListenPortRange.Select();
                this.Enabled = true;
                return;
            }

            // Do a single trim here rather than multiple trims (save a few cpu cycles)
            string strListen = comboBox_ListenOn.Text.Trim();
            string strConnect = comboBox_ConnectTo.Text.Trim();

            // Validate IPv4 (TODO: works great for IP4, but what if we add IP6 in one or both fields?) IP6 regex updated and now working. Look into if it's validation is needed anywwhere else.
            if (!ValidateIPv4(strListen, 1)) return;
            if (!ValidateIPv4(strConnect, 2)) return;

            // TODO: unfinished(?) We might be done now. just go through and confirm. I've confirmed range duping does work.
            //Debug.Write(DupeCheck());

            // Add to Rule
            Rule rule = new()
            {
                Type = comboBox_Type.Text.Trim(),
                ListenOn = strListen,
                ListenPort = listenPort,
                ConnectTo = strConnect,
                ConnectPort = connectPort,
                Comment = textBox_Comment.Text.Trim(),
                Group = comboBox_Group.Text.Trim(),
            };

            // Validate the Proxy Type ... // TODO: now with proper IP4/6 detection on keypress, this entire combobox may not even be needed. Maybe just add a label showing it?
            if (rule.Type == AutoTypeString) rule.Type = GetPassType(rule.ListenOn, rule.ConnectTo);

            // Add the rule to the list & db
            if (_updateMode)
            {
                // Update/Edit/Modify Existing Rule

                Rule oldRule = Program.Database.GetRule(_itemRule.Type, _itemRule.ListenOn, _itemRule.ListenPort);
                PortPorxyUtil.DeleteProxy(oldRule);
                Program.Database.Remove(oldRule);

                PortPorxyUtil.AddOrUpdateProxy(rule);
                Program.Database.Add(rule);

                ParentWindow.UpdateListViewItem(_listViewItem, rule, 1);
            }
            else
            {
                // Add New Rule

                int intRange = listenPortRange - listenPort + 1;
                int intDupes = 0;

                progBarRange.Maximum = intRange;


                for (int i = 0; i < intRange; i++)
                {

                    rule.ListenPort = listenPort;
                    progBarRange.Value += 1;

                    if (DupeCheck(rule))
                    {
                        // If its a dupe, skip adding it.
                        intDupes += 1;
                    }
                    else
                    {

                        PortPorxyUtil.AddOrUpdateProxy(rule);
                        Program.Database.Add(rule);
                    }

                    listenPort += 1;
                }

                // Alert the user if any were skipped due to duping
                if (intDupes > 0) MessageBox.Show(string.Format("{0} duplicates skipped.", intDupes), "Didn't add some", MessageBoxButtons.OK, MessageBoxIcon.Information);

                ParentWindow.RefreshProxyList();
            }
            PortPorxyUtil.ParamChange();

            progBarRange.Visible = false;
            this.Enabled = true;
            Close();
        }

        private void SetProxyForm_Load(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;

            Top = ParentWindow.Top + (ParentWindow.Height - Height) / 2;
            Left = ParentWindow.Left + (ParentWindow.Width - Width) / 2;

            // Current WSL IP
            lblWSLIP.Text = "Refreshing ...";
            string strWSLIP = PortPorxyUtil.GetWSLIP();

            if (strWSLIP.Length > 0)
            {
                lblWSLIP.Text = string.Format("WSL: {0}", strWSLIP);
                comboBox_ConnectTo.AutoCompleteCustomSource.Add(strWSLIP);
                comboBox_ListenOn.AutoCompleteCustomSource.Add(strWSLIP);
            }
            else
            {
                lblWSLIP.Text = "WSL: Dunno";
            }

            this.Cursor = Cursors.Default;
        }

        private void SetProxyForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            ParentWindow.SetProxyForm = null;
        }

        private void chkBox_ListenPortRange_CheckedChanged(object sender, EventArgs e)
        {
            if (chkBox_ListenPortRange.Checked)
            {
                lblDash.Visible = true;
                textBox_ListenPortRange.Visible = true;
                lblRangeCount.Visible = true;
                lblRangeCount.Text = String.IsNullOrEmpty(textBox_ListenPortRange.Text) ? "Adding: 0" : "Adding: 1";
            }
            else
            {
                lblDash.Visible = false;
                textBox_ListenPortRange.Visible = false;
                lblRangeCount.Visible = false;
            }
        }

        private void textBox_ListenPort_TextChanged(object sender, EventArgs e)
        {
            textBox_ListenPortRange.Text = textBox_ListenPort.Text;
            textBox_ConnectPort.Text = textBox_ListenPort.Text;

            lblDupe.Visible = DupeCheck() ? true : false;
        }

        private void textBox_ListenPortRange_TextChanged(object sender, EventArgs e)
        {

            int intRangeCount = Convert.ToInt32(textBox_ListenPortRange.Text) - Convert.ToInt32(textBox_ListenPort.Text) + 1;
            string strBase = "Total Proxies:";

            lblRangeCount.Text = intRangeCount < 0 ? strBase + " 0" : string.Format("{0} {1}", strBase, intRangeCount);
        }

        /// <summary>
        /// Validates the IPv4 fields (allows exception for an asterisk); sets focus to invalid field(s)
        /// </summary>
        /// <param name="strIP">IPv4 string to check</param>
        /// <param name="intField">Field to focus back on in case of a failure. 2: ConnectTo, any other int:ListenOn.</param>
        /// <returns>true if valid; false if invalid</returns>
        private bool ValidateIPv4(string strIP, int intField)
        {
            bool bResult = true;

            if (PortPorxyUtil.IsIPv4(strIP) == false && strIP != "*")
            {
                MessageBox.Show(string.Format("{0} is not a valid IP", strIP), "What are you up to?", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

                if (intField == 2)
                {
                    // The "ConnectTo" field
                    bResult = false;
                    comboBox_ConnectTo.Select();
                }
                else
                {
                    // The "ListenOn" field
                    bResult = false;
                    comboBox_ListenOn.Select();
                }
                this.Enabled = true;
            }
            return bResult;
        }

        /// <summary>
        /// Compares the respective items in the listview w/the entered data, looking for it already existing in the list.
        /// </summary>
        /// <param name="rule">[optional] rule list. If not passed, will read currently entered form fields.</param>
        /// <returns>True if Dupe found; False if no Dupe.</returns>
        private bool DupeCheck([Optional] Rule rule)
        {
            // NOTE: I haven't thought through all possible dupe scenarios, so they may or may not still need some tweaking.

            string strType = rule != null ? rule.Type : comboBox_Type.Text.Trim();
            string strListen = rule != null ? rule.ListenOn : comboBox_ListenOn.Text.Trim();
            string strListenPort = rule != null ? rule.ListenPort.ToString() : textBox_ListenPort.Text.Trim();

            bool bResult = false;

            foreach (ListViewItem item in ParentWindow.listViewProxies.Items)
            {
                if (item.SubItems[1].Text.Equals(strType) &&
                    item.SubItems[2].Text.Equals(strListen) &&
                    item.SubItems[3].Text.Equals(strListenPort))
                {
                    // If dupe port found, flag it.
                    bResult = true;
                    break;
                }
            }
            return bResult;
        }

        /// <summary>
        /// Autoselects the correct Type based on what the user types into the LisatenOn/ConnectTo fields
        /// </summary>
        private void TypeCheck()
        {
            // TypeCheck (TODO: Still buggy)
            string strListen = comboBox_ListenOn.Text.Trim();
            string strConnect = comboBox_ConnectTo.Text.Trim();
            string strResult = GetPassType(strListen, strConnect);

            comboBox_Type.Text = strResult;
            lblType.Text = strResult;
        }

        private void comboBox_Type_SelectedIndexChanged(object sender, EventArgs e)
        {
            lblDupe.Visible = DupeCheck() ? true : false;
        }

        private void comboBox_ConnectTo_TextChanged(object sender, EventArgs e)
        {
            // typeCheck
            TypeCheck();
        }

        private void comboBox_ListenOn_TextChanged(object sender, EventArgs e)
        {
            // Dupecheck
            lblDupe.Visible = DupeCheck() ? true : false;

            // typeCheck
            TypeCheck();
        }

        /// <summary>
        /// Refreshes the WSL IP on double-click
        /// </summary>
        private void lblWSLIP_DoubleClick(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            lblWSLIP.Text = "Refreshing ...";
            string strWSLIP = PortPorxyUtil.GetWSLIP();
            lblWSLIP.Text = strWSLIP.Length > 0 ? string.Format("WSL: {0}", strWSLIP) : "WSL: Dunno";
            this.Cursor = Cursors.Default;
        }
    }
}
