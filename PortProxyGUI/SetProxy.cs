#region + -- NAMESPACE IMPORTS -- +

using NStandard;
using PortProxyGooey.Data;
using PortProxyGooey.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
//using System.Web.UI.WebControls;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using Rule = PortProxyGooey.Data.Rule;

#endregion

// TODO: 1) Remove comboBox_Type and all code refs to it.
//       2) I added a range of 45 to 54 and label said 10?
//       3) Add all auto-comment ports to the relevant autocomplete fields as well.
//       4) BUG: cant enter colons now, in the listenon/connectto fields, which means no ip6.
namespace PortProxyGooey
{
    public partial class SetProxy : Form
    {
        #region + -- VAR DECLARATIONS -- +

        public readonly PortProxyGooey ParentWindow;
        private string AutoTypeString { get; }

        private bool _updateMode;
        private ListViewItem _listViewItem;
        private Rule _itemRule;

        // Remembers the last label we added for the user
        private string strLastAutoLabel = string.Empty;

        #endregion

        public SetProxy(PortProxyGooey parent)
        {
            ParentWindow = parent;

            InitializeComponent();
            Font = InterfaceUtil.UiFont;

            //
            AutoTypeString = comboBox_Type.Text = comboBox_Type.Items.OfType<string>().First();

            string[] groupNames = (
                from g in parent.listViewProxies.Groups.OfType<ListViewGroup>()
                let header = g.Header
                where !header.IsNullOrWhiteSpace()
                select header
            ).ToArray();

            comboBox_Group.Items.AddRange(groupNames);
        }

        /// <summary>
        /// Add a New proxy rule, or Clone a proxy rule.
        /// </summary>
        /// <param name="rule">[Optional] Rule object. If null, function acts as a brand new empty proxy; if Rule passed, function acts as a clone.</param>
        public void UseNormalMode(Rule rule = null)
        {
            _updateMode = false;
            _listViewItem = null;
            _itemRule = null;

            // Show "Clone" label?
            lblClone.Visible = rule != null;

            comboBox_Type.Text = rule != null ? rule.Type : AutoTypeString;
            comboBox_Group.Text = rule != null ? rule.Group : String.Empty;

            comboBox_ListenOn.Text = rule != null ? rule.ListenOn : "*";
            textBox_ListenPort.Text = rule != null ? rule.ListenPort.ToString() : String.Empty;
            comboBox_ConnectTo.Text = rule != null ? rule.ConnectTo : String.Empty;
            textBox_ConnectPort.Text = rule != null ? rule.ConnectPort.ToString() : String.Empty;
            textBox_Comment.Text = rule != null ? rule.Comment : String.Empty;
        }

        /// <summary>
        /// Update/Modify an existing proxy
        /// </summary>
        /// <param name="item"></param>
        /// <param name="rule">A populated Rule object</param>
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

        /// <summary>
        /// Determine the Type of proxy to pass
        /// </summary>
        /// <param name="listenOn">IP to Listen on</param>
        /// <param name="connectTo">IP to connect to</param>
        /// <returns>Properly formatted proxy Type</returns>
        private static string GetPassType(string listenOn, string connectTo)
        {
            string from = PortProxyUtil.IsIPv6(listenOn) ? "v6" : "v4";
            string to = PortProxyUtil.IsIPv6(connectTo) ? "v6" : "v4";
            return $"{from}to{to}";
        }

        private void button_Set_Click(object sender, EventArgs e)
        {
            int listenPort, listenPortRange, connectPort;

            // Don't let em funk with anything while we're doin work
            this.Enabled = false;

            // Validate the Ports
            try
            {
                listenPort = Rule.ParsePort(textBox_ListenPort.Text);
                connectPort = Rule.ParsePort(textBox_ConnectPort.Text);
                listenPortRange = Rule.ParsePort(textBox_ListenPortRange.Text);
            }
            catch (NotSupportedException ex)
            {
                Debug.WriteLine(ex.Message);
                MessageBox.Show("You're trying to set either a bad port, or no port. Do better.", "Uh, no ...", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                progBarRange.Visible = false;
                this.Enabled = true;
                return;
            }

            // If adding a range ...
            if (chkBox_ListenPortRange.Checked && listenPortRange < listenPort)
            {
                MessageBox.Show("Ending Port is LOWER than the Starting Port", "You need to fix this ...", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

                // Set focus of the 'offending' error target to better help the user understand the source of the issue.
                textBox_ListenPortRange.Select();
                this.Enabled = true;
                return;
            }

            progBarRange.Visible = true;

            // Do a single trim here rather than multiple trims (save a few cpu cycles)
            string strListen = comboBox_ListenOn.Text.Trim();
            string strConnect = comboBox_ConnectTo.Text.Trim();

            // Validate IPv4 (TODO: works great for IP4, but what if we add IP6 in one or both fields?) IP6 regex updated and now working. Look into if it's validation is needed anywwhere else.
            if (!ValidateIPv4(strListen, 1)) return;
            if (!ValidateIPv4(strConnect, 2)) return;

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

            // Validate the Proxy Type ...
            //if (rule.Type == AutoTypeString) rule.Type = GetPassType(rule.ListenOn, rule.ConnectTo);

            // Add the rule to the list & db
            if (_updateMode)
            {
                // Update/Edit/Modify an Existing Rule

                Rule oldRule = Program.Database.GetRule(_itemRule.Type, _itemRule.ListenOn, _itemRule.ListenPort);
                PortProxyUtil.DeleteProxy(oldRule);
                Program.Database.Remove(oldRule);

                PortProxyUtil.AddOrUpdateProxy(rule);
                Program.Database.Add(rule);

                ParentWindow.UpdateListViewItem(_listViewItem, rule, 1);
            }
            else
            {
                // Add a New Rule

                int intRange = listenPortRange - listenPort + 1;
                int intDupes = 0;

                progBarRange.Maximum = intRange;

                for (int i = 0; i < intRange; i++)
                {
                    rule.ListenPort = listenPort;
                    progBarRange.Value += 1;

                    if (DupeCheck(rule))
                    {
                        // If its a dupe, skip it.
                        intDupes += 1;
                    }
                    else
                    {
                        PortProxyUtil.AddOrUpdateProxy(rule);
                        Program.Database.Add(rule);
                    }
                    listenPort++;
                }

                // Alert the user if any were skipped due to duping
                if (intDupes > 0) MessageBox.Show(string.Format("{0} duplicates skipped.", intDupes), "Didn't add some ...", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            ParentWindow.RefreshProxyList();

            // Set .EnsureVisible and .Selected to the newest item added, as a convenience.
            // (Searches listViewProxies .tags for the rule.id)
            ListViewItem item = ParentWindow.listViewProxies.Items
                        .Cast<ListViewItem>()
                        .FirstOrDefault(i => i.Tag != null && i.Tag.ToString().Contains(rule.Id));

            if (item != null)
            {
                ParentWindow.listViewProxies.EnsureVisible(item.Index);
                ParentWindow.listViewProxies.Items[item.Index].Selected = true;
            }

            PortProxyUtil.ParamChange();

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
            string strWSLIP = JSE_Utils.WSL.WSL_GetIP();

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
                lblRangeCount.Text = String.IsNullOrEmpty(textBox_ListenPortRange.Text) ? "Adding: 0" : "Adding: " + CalcRange().ToString();
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
            // Add the same port to the range box as a starting point
            textBox_ListenPortRange.Text = textBox_ListenPort.Text;

            // Add the same port to the connect port box as a starting point
            textBox_ConnectPort.Text = textBox_ListenPort.Text;

            // If it's a dupe port show the label; else hide it.
            lblDupe.Visible = DupeCheck();

            // Auto-comment common ports
            AutoComment(textBox_ListenPort);
        }

        private void textBox_ListenPortRange_TextChanged(object sender, EventArgs e)
        {
            // TODO: I think I can create a single Call for this sub and line 260 called something like "UpdateRangeLabel"
            int intRangeCount = CalcRange();
            string strBase = "Adding:";

            lblRangeCount.Text = intRangeCount < 0 ? strBase + " 0" : string.Format("{0} {1}", strBase, intRangeCount);

            // Auto-comment common ports
            AutoComment(textBox_ListenPortRange);
        }

        private void textBox_ConnectPort_TextChanged(object sender, EventArgs e)
        {
            // Auto-comment common ports
            AutoComment(textBox_ConnectPort);
        }

        /// <summary>
        /// Calculats how many ports will be added based on the values in the port fields
        /// </summary>
        /// <returns>Number of ports</returns>
        private int CalcRange()
        {
            // Make sure we have something to calc first, or else error.
            if (!string.IsNullOrWhiteSpace(textBox_ListenPortRange.Text) && !string.IsNullOrWhiteSpace(textBox_ListenPort.Text))
            {
                int intLPR = Convert.ToInt32(textBox_ListenPortRange.Text.Trim());
                int intLP = Convert.ToInt32(textBox_ListenPort.Text.Trim());
                return ((intLPR - intLP) + 1);
            }
            return 0;
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

            if (PortProxyUtil.IsIPv4(strIP) == false && strIP != "*")
            {
                MessageBox.Show(string.Format("{0} is not a valid IP", strIP), "What are you up to here?", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

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
                progBarRange.Visible = bResult;
                this.Enabled = true;
            }
            return bResult;
        }

        /// <summary>
        /// Automatically enters info in the Comment field for recognized ports.
        /// </summary>
        private void AutoComment(System.Windows.Forms.TextBox textBox)
        {
            // Ref: https://en.wikipedia.org/wiki/List_of_TCP_and_UDP_port_numbers

            Dictionary<string, string> port = new()
            {
                {"FTP", "21"},
                {"SSH", "22"},
                {"Telnet", "23"},
                {"SMTP", "25"},
                {"HTTP", "80"},
                {"DNS", "53"},
                {"POP3", "110"},
                {"sFTP", "115"},
                {"RPC", "135"},
                {"NetBIOS", "139"},
                {"IMAP", "143"},
                {"IRC", "194"},
                {"HTTPS", "443"},
                {"SMB", "445"},
                {"MSSQL", "1433"},
                {"Docker", "2375"},
                {"Docker (SSL)", "2376"},
                {"MySQL/MariaDB", "3306"},
                {"RDP", "3389"},
                {"PCAnywhere", "5632"},
                {"VNC", "5900"},
                {"Jellyfin", "8920"},
                {"Prometheus", "9090"},
                {"Transmission", "9091"},
                {"Syncthing", "8384,22000"},
                {"Minecraft", "25565"},
                {"TeamSpeak", "10011,10022,30033"},
                {"TetriNET", "31457"},
                {"Plex", "32400"},
                {"Jenkens", "33848"},
                {"Glances", "61208"}
            };

            // TODO: Add more ports, i.e. the *arr and other docker things.
            // TODO: Add saving of below option to db
            if (chkAutoComment.Checked)
            {
                // Auto-Labeling of Common Ports
                string searchValue = textBox.Text.Trim();
                string matchingKey = port.FirstOrDefault(x => x.Value.Split(',').Contains(searchValue)).Key;

                if (matchingKey != null)
                {
                    // If user enters a common port, give it an auto-label as a convenience to them. Non-destructive: this will not delete anything they have manually typed in the comment field.
                    //Debug.WriteLine($"The key for value '{searchValue}' is '{matchingKey}'.");
                    textBox_Comment.Text = string.Format("[{0}] {1}", matchingKey, string.IsNullOrEmpty(strLastAutoLabel) ? textBox_Comment.Text.Trim() : textBox_Comment.Text.Replace(strLastAutoLabel, string.Empty).Trim());
                    strLastAutoLabel = string.Format("[{0}]", matchingKey);
                }
                else
                {
                    // If no matching port found, just leave whatever text they may have entered, removing any previous auto-label if exists.
                    //Debug.WriteLine($"No key found for value '{searchValue}'.");
                    textBox_Comment.Text = string.IsNullOrEmpty(strLastAutoLabel) ? textBox_Comment.Text.Trim() : textBox_Comment.Text.Replace(strLastAutoLabel, string.Empty).Trim();
                }
            }
        }

        /// <summary>
        /// Compares the respective items in the listview w/the entered data, looking for it already existing in the list.
        /// </summary>
        /// <param name="rule">[optional] rule list. If not passed, will read currently entered form fields.</param>
        /// <returns>True if Dupe found; False if no Dupe.</returns>
        private bool DupeCheck([Optional] Rule rule)
        {
            // + ------------------------------------------------------------------------------------------------------------------- +
            // | NOTES:                                                                                                              |
            // |   - I haven't thought through all possible dupe scenarios, so they may or may not still need some tweaking.         |
            // |   - I only check Type & Listen Fields, because once you're already listening on an IP + Port Number + Specific Type |
            // |     it doesn't matter if you change the ConnectTo IP of the same Type, because you can only listen on 1 at a time;  |
            // |     i.e. 0.0.0.0 Port 53 4to4 can only be changed to something like 0.0.0.0 53 ::1 4to6                             |
            // + ------------------------------------------------------------------------------------------------------------------- +

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
        /// Auto-selects the correct Type based on what the user types into the LisatenOn/ConnectTo fields
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
            lblDupe.Visible = DupeCheck();
        }

        private void comboBox_ConnectTo_TextChanged(object sender, EventArgs e)
        {
            TypeCheck();
        }

        private void comboBox_ListenOn_TextChanged(object sender, EventArgs e)
        {
            // Dupecheck
            lblDupe.Visible = DupeCheck();

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
            string strWSLIP = JSE_Utils.WSL.WSL_GetIP();
            lblWSLIP.Text = strWSLIP.Length > 0 ? string.Format("WSL: {0}", strWSLIP) : "WSL: Dunno";
            this.Cursor = Cursors.Default;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        #region TextBox Increase / Decrease

        private void TextBox_ListenPortRange_MouseWheel(object sender, MouseEventArgs e)
        {
            bool bUp = (e.Delta > 0);
            IncreaseDecrease(bUp, textBox_ListenPortRange);
        }

        private void TextBox_ListenPort_MouseWheel(object sender, MouseEventArgs e)
        {
            bool bUp = (e.Delta > 0);
            IncreaseDecrease(bUp, textBox_ListenPort);
        }

        private void TextBox_ConnectPort_MouseWheel(object sender, MouseEventArgs e)
        {
            bool bUp = (e.Delta > 0);
            IncreaseDecrease(bUp, textBox_ConnectPort);
        }

        private void textBox_ConnectPort_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode.Equals(Keys.Up))
            {
                IncreaseDecrease(true, textBox_ConnectPort);
            }
            else if (e.KeyCode.Equals(Keys.Down))
            {
                IncreaseDecrease(false, textBox_ConnectPort);
            }
        }

        private void textBox_ListenPort_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode.Equals(Keys.Up))
            {
                IncreaseDecrease(true, textBox_ListenPort);
            }
            else if (e.KeyCode.Equals(Keys.Down))
            {
                IncreaseDecrease(false, textBox_ListenPort);
            }
        }

        private void textBox_ListenPortRange_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode.Equals(Keys.Up))
            {
                IncreaseDecrease(true, textBox_ListenPortRange);
            }
            else if (e.KeyCode.Equals(Keys.Down))
            {
                IncreaseDecrease(false, textBox_ListenPortRange);
            }
        }

        /// <summary>
        /// Increase / Decrease the number in a Textbox
        /// </summary>
        /// <param name="bUp">[boolean] true = Up; false = Down</param>
        /// <param name="textBox">A Textbox control</param>
        private static void IncreaseDecrease(bool bUp, System.Windows.Forms.TextBox textBox)
        {
            if (string.IsNullOrEmpty(textBox.Text)) textBox.Text = "0";
            int intCurrentVal = Convert.ToInt32(textBox.Text);

            if (bUp)
            {
                intCurrentVal++;
            }
            else
            {
                // Don't allow negative numbers
                if (intCurrentVal != 0) intCurrentVal--;
            }

            textBox.Text = intCurrentVal.ToString();

            // Move the cursor to the end, or else it just looks weird to me.
            textBox.SelectionStart = textBox.TextLength;
            textBox.SelectionLength = 0;
        }

        #endregion

        #region TextBox KeyPress

        private void textBox_ListenPort_KeyPress(object sender, KeyPressEventArgs e)
        {
            DigitsOnly(e);
        }

        private void textBox_ConnectPort_KeyPress(object sender, KeyPressEventArgs e)
        {
            DigitsOnly(e);
        }

        private void textBox_ListenPortRange_KeyPress(object sender, KeyPressEventArgs e)
        {
            DigitsOnly(e);
        }

        /// <summary>
        /// Cancels the event if anything other than a number is entered
        /// </summary>
        private static void DigitsOnly(KeyPressEventArgs e)
        {
            if (!char.IsDigit(e.KeyChar) && e.KeyChar != '\b')
            {
                e.Handled = true;
            }
        }

        #endregion

        #region ComboBox KeyPress

        private void comboBox_ListenOn_KeyPress(object sender, KeyPressEventArgs e)
        {
            OnlyCertainAllowed(e);
        }

        private void comboBox_ConnectTo_KeyPress(object sender, KeyPressEventArgs e)
        {
            OnlyCertainAllowed(e);
        }

        /// <summary>
        /// Only numbers, period, and asterisk allowed.
        /// </summary>
        private static void OnlyCertainAllowed(KeyPressEventArgs e)
        {
            if (!char.IsDigit(e.KeyChar) && e.KeyChar != '.' && e.KeyChar != '*' && e.KeyChar != '\b')
            {
                e.Handled = true;
            }
        }



        #endregion

    }
}
