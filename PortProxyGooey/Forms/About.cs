#region +-- IMPORTS -- +

using PortProxyGooey.Utils;
using System;
using System.Diagnostics;
using System.Windows.Forms;

#endregion

namespace PortProxyGooey {

    public partial class About : Form {

        public readonly PortProxyGooey PortProxyGooey;

        public About(PortProxyGooey PortProxyGooey) {

            this.PortProxyGooey = PortProxyGooey;

            InitializeComponent();
            Font = InterfaceUtil.UiFont;

            label_version.Text = label_version.Text + "  v" + Application.ProductVersion;
        }

        private void linkLabel1_Click(object sender, EventArgs e) {

            if (sender is LinkLabel _sender) {
                Process.Start("explorer", _sender.Text);
            }

        }

        private void About_FormClosing(object sender, FormClosingEventArgs e) {
            PortProxyGooey.AboutForm = null;
        }

    }

}
