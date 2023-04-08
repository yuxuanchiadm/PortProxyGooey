namespace PortProxyGUI
{
    partial class PortProxyGUI
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PortProxyGUI));
            listViewProxies = new System.Windows.Forms.ListView();
            columnHeader1 = new System.Windows.Forms.ColumnHeader();
            columnHeader2 = new System.Windows.Forms.ColumnHeader();
            columnHeader3 = new System.Windows.Forms.ColumnHeader();
            columnHeader4 = new System.Windows.Forms.ColumnHeader();
            columnHeader5 = new System.Windows.Forms.ColumnHeader();
            columnHeader6 = new System.Windows.Forms.ColumnHeader();
            columnHeader7 = new System.Windows.Forms.ColumnHeader();
            contextMenuStrip_RightClick = new System.Windows.Forms.ContextMenuStrip(components);
            toolStripMenuItem_Enable = new System.Windows.Forms.ToolStripMenuItem();
            toolStripMenuItem_Disable = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            toolStripMenuItem_New = new System.Windows.Forms.ToolStripMenuItem();
            toolStripMenuItem_Modify = new System.Windows.Forms.ToolStripMenuItem();
            toolStripMenuItem_Delete = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            clearToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripMenuItem_Refresh = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            toolStripMenuItem_More = new System.Windows.Forms.ToolStripMenuItem();
            toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            toolStripMenuItem_Export = new System.Windows.Forms.ToolStripMenuItem();
            toolStripMenuItem_Import = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            externalAppsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            dockerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator8 = new System.Windows.Forms.ToolStripSeparator();
            windowsFirewallToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            windowsFirewallToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            basicToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            advancedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            windowsFirewallControlWFCToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            rulesPanelToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            connectionsPanelToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            adaptersToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            toolStripMenuItem_FlushDnsCache = new System.Windows.Forms.ToolStripMenuItem();
            toolStripMenuItem_ResetWindowSize = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            toolStripMenuItem_About = new System.Windows.Forms.ToolStripMenuItem();
            imageListProxies = new System.Windows.Forms.ImageList(components);
            saveFileDialog_Export = new System.Windows.Forms.SaveFileDialog();
            openFileDialog_Import = new System.Windows.Forms.OpenFileDialog();
            contextMenuStrip_RightClick.SuspendLayout();
            SuspendLayout();
            // 
            // listViewProxies
            // 
            listViewProxies.BorderStyle = System.Windows.Forms.BorderStyle.None;
            listViewProxies.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { columnHeader1, columnHeader2, columnHeader3, columnHeader4, columnHeader5, columnHeader6, columnHeader7 });
            listViewProxies.ContextMenuStrip = contextMenuStrip_RightClick;
            resources.ApplyResources(listViewProxies, "listViewProxies");
            listViewProxies.FullRowSelect = true;
            listViewProxies.Name = "listViewProxies";
            listViewProxies.SmallImageList = imageListProxies;
            listViewProxies.UseCompatibleStateImageBehavior = false;
            listViewProxies.View = System.Windows.Forms.View.Details;
            listViewProxies.ColumnClick += listViewProxies_ColumnClick;
            listViewProxies.ColumnWidthChanged += listViewProxies_ColumnWidthChanged;
            listViewProxies.DoubleClick += listViewProxies_DoubleClick;
            listViewProxies.MouseUp += listViewProxies_MouseUp;
            // 
            // columnHeader1
            // 
            resources.ApplyResources(columnHeader1, "columnHeader1");
            // 
            // columnHeader2
            // 
            resources.ApplyResources(columnHeader2, "columnHeader2");
            // 
            // columnHeader3
            // 
            resources.ApplyResources(columnHeader3, "columnHeader3");
            // 
            // columnHeader4
            // 
            columnHeader4.Tag = "";
            resources.ApplyResources(columnHeader4, "columnHeader4");
            // 
            // columnHeader5
            // 
            resources.ApplyResources(columnHeader5, "columnHeader5");
            // 
            // columnHeader6
            // 
            columnHeader6.Tag = "";
            resources.ApplyResources(columnHeader6, "columnHeader6");
            // 
            // columnHeader7
            // 
            resources.ApplyResources(columnHeader7, "columnHeader7");
            // 
            // contextMenuStrip_RightClick
            // 
            resources.ApplyResources(contextMenuStrip_RightClick, "contextMenuStrip_RightClick");
            contextMenuStrip_RightClick.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { toolStripMenuItem_Enable, toolStripMenuItem_Disable, toolStripSeparator3, toolStripMenuItem_New, toolStripMenuItem_Modify, toolStripMenuItem_Delete, toolStripSeparator1, clearToolStripMenuItem, toolStripMenuItem_Refresh, toolStripSeparator2, toolStripMenuItem_More, toolStripSeparator4, toolStripMenuItem_About });
            contextMenuStrip_RightClick.Name = "contextMenuStrip1";
            contextMenuStrip_RightClick.Closed += contextMenuStrip_RightClick_Closed;
            contextMenuStrip_RightClick.MouseClick += contextMenuStrip_RightClick_MouseClick;
            // 
            // toolStripMenuItem_Enable
            // 
            toolStripMenuItem_Enable.Image = Properties.Resources.enable;
            toolStripMenuItem_Enable.Name = "toolStripMenuItem_Enable";
            resources.ApplyResources(toolStripMenuItem_Enable, "toolStripMenuItem_Enable");
            // 
            // toolStripMenuItem_Disable
            // 
            toolStripMenuItem_Disable.Image = Properties.Resources.disable;
            toolStripMenuItem_Disable.Name = "toolStripMenuItem_Disable";
            resources.ApplyResources(toolStripMenuItem_Disable, "toolStripMenuItem_Disable");
            // 
            // toolStripSeparator3
            // 
            toolStripSeparator3.Name = "toolStripSeparator3";
            resources.ApplyResources(toolStripSeparator3, "toolStripSeparator3");
            // 
            // toolStripMenuItem_New
            // 
            toolStripMenuItem_New.Image = Properties.Resources.add;
            toolStripMenuItem_New.Name = "toolStripMenuItem_New";
            resources.ApplyResources(toolStripMenuItem_New, "toolStripMenuItem_New");
            // 
            // toolStripMenuItem_Modify
            // 
            toolStripMenuItem_Modify.Image = Properties.Resources.edit;
            toolStripMenuItem_Modify.Name = "toolStripMenuItem_Modify";
            resources.ApplyResources(toolStripMenuItem_Modify, "toolStripMenuItem_Modify");
            // 
            // toolStripMenuItem_Delete
            // 
            toolStripMenuItem_Delete.Image = Properties.Resources.delete;
            toolStripMenuItem_Delete.Name = "toolStripMenuItem_Delete";
            resources.ApplyResources(toolStripMenuItem_Delete, "toolStripMenuItem_Delete");
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            resources.ApplyResources(toolStripSeparator1, "toolStripSeparator1");
            // 
            // clearToolStripMenuItem
            // 
            clearToolStripMenuItem.Image = Properties.Resources.clear;
            clearToolStripMenuItem.Name = "clearToolStripMenuItem";
            resources.ApplyResources(clearToolStripMenuItem, "clearToolStripMenuItem");
            // 
            // toolStripMenuItem_Refresh
            // 
            toolStripMenuItem_Refresh.Image = Properties.Resources.refresh;
            toolStripMenuItem_Refresh.Name = "toolStripMenuItem_Refresh";
            resources.ApplyResources(toolStripMenuItem_Refresh, "toolStripMenuItem_Refresh");
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            resources.ApplyResources(toolStripSeparator2, "toolStripSeparator2");
            // 
            // toolStripMenuItem_More
            // 
            toolStripMenuItem_More.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { toolStripMenuItem1, toolStripSeparator5, externalAppsToolStripMenuItem, toolStripSeparator6, toolStripMenuItem_FlushDnsCache, toolStripMenuItem_ResetWindowSize });
            toolStripMenuItem_More.Name = "toolStripMenuItem_More";
            resources.ApplyResources(toolStripMenuItem_More, "toolStripMenuItem_More");
            // 
            // toolStripMenuItem1
            // 
            toolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { toolStripMenuItem_Export, toolStripMenuItem_Import });
            toolStripMenuItem1.Name = "toolStripMenuItem1";
            resources.ApplyResources(toolStripMenuItem1, "toolStripMenuItem1");
            // 
            // toolStripMenuItem_Export
            // 
            toolStripMenuItem_Export.Name = "toolStripMenuItem_Export";
            resources.ApplyResources(toolStripMenuItem_Export, "toolStripMenuItem_Export");
            toolStripMenuItem_Export.Click += toolStripMenuItem_Export_Click;
            // 
            // toolStripMenuItem_Import
            // 
            toolStripMenuItem_Import.Name = "toolStripMenuItem_Import";
            resources.ApplyResources(toolStripMenuItem_Import, "toolStripMenuItem_Import");
            toolStripMenuItem_Import.Click += toolStripMenuItem_Import_Click;
            // 
            // toolStripSeparator5
            // 
            toolStripSeparator5.Name = "toolStripSeparator5";
            resources.ApplyResources(toolStripSeparator5, "toolStripSeparator5");
            // 
            // externalAppsToolStripMenuItem
            // 
            externalAppsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { dockerToolStripMenuItem, toolStripSeparator8, windowsFirewallToolStripMenuItem, adaptersToolStripMenuItem });
            externalAppsToolStripMenuItem.Name = "externalAppsToolStripMenuItem";
            resources.ApplyResources(externalAppsToolStripMenuItem, "externalAppsToolStripMenuItem");
            // 
            // dockerToolStripMenuItem
            // 
            dockerToolStripMenuItem.Image = Properties.Resources.docker;
            dockerToolStripMenuItem.Name = "dockerToolStripMenuItem";
            resources.ApplyResources(dockerToolStripMenuItem, "dockerToolStripMenuItem");
            // 
            // toolStripSeparator8
            // 
            toolStripSeparator8.Name = "toolStripSeparator8";
            resources.ApplyResources(toolStripSeparator8, "toolStripSeparator8");
            // 
            // windowsFirewallToolStripMenuItem
            // 
            windowsFirewallToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { windowsFirewallToolStripMenuItem1, toolStripSeparator7, windowsFirewallControlWFCToolStripMenuItem });
            windowsFirewallToolStripMenuItem.Name = "windowsFirewallToolStripMenuItem";
            resources.ApplyResources(windowsFirewallToolStripMenuItem, "windowsFirewallToolStripMenuItem");
            // 
            // windowsFirewallToolStripMenuItem1
            // 
            windowsFirewallToolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { basicToolStripMenuItem, advancedToolStripMenuItem });
            windowsFirewallToolStripMenuItem1.Name = "windowsFirewallToolStripMenuItem1";
            resources.ApplyResources(windowsFirewallToolStripMenuItem1, "windowsFirewallToolStripMenuItem1");
            // 
            // basicToolStripMenuItem
            // 
            basicToolStripMenuItem.Name = "basicToolStripMenuItem";
            resources.ApplyResources(basicToolStripMenuItem, "basicToolStripMenuItem");
            basicToolStripMenuItem.Click += basicToolStripMenuItem_Click;
            // 
            // advancedToolStripMenuItem
            // 
            advancedToolStripMenuItem.Name = "advancedToolStripMenuItem";
            resources.ApplyResources(advancedToolStripMenuItem, "advancedToolStripMenuItem");
            advancedToolStripMenuItem.Click += advancedToolStripMenuItem_Click;
            // 
            // toolStripSeparator7
            // 
            toolStripSeparator7.Name = "toolStripSeparator7";
            resources.ApplyResources(toolStripSeparator7, "toolStripSeparator7");
            // 
            // windowsFirewallControlWFCToolStripMenuItem
            // 
            windowsFirewallControlWFCToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { rulesPanelToolStripMenuItem, connectionsPanelToolStripMenuItem });
            windowsFirewallControlWFCToolStripMenuItem.Image = Properties.Resources.wfc;
            windowsFirewallControlWFCToolStripMenuItem.Name = "windowsFirewallControlWFCToolStripMenuItem";
            resources.ApplyResources(windowsFirewallControlWFCToolStripMenuItem, "windowsFirewallControlWFCToolStripMenuItem");
            // 
            // rulesPanelToolStripMenuItem
            // 
            rulesPanelToolStripMenuItem.Image = Properties.Resources.wfc2;
            rulesPanelToolStripMenuItem.Name = "rulesPanelToolStripMenuItem";
            resources.ApplyResources(rulesPanelToolStripMenuItem, "rulesPanelToolStripMenuItem");
            rulesPanelToolStripMenuItem.Click += rulesPanelToolStripMenuItem_Click;
            // 
            // connectionsPanelToolStripMenuItem
            // 
            connectionsPanelToolStripMenuItem.Image = Properties.Resources.wfc2;
            connectionsPanelToolStripMenuItem.Name = "connectionsPanelToolStripMenuItem";
            resources.ApplyResources(connectionsPanelToolStripMenuItem, "connectionsPanelToolStripMenuItem");
            connectionsPanelToolStripMenuItem.Click += connectionsPanelToolStripMenuItem_Click;
            // 
            // adaptersToolStripMenuItem
            // 
            adaptersToolStripMenuItem.Name = "adaptersToolStripMenuItem";
            resources.ApplyResources(adaptersToolStripMenuItem, "adaptersToolStripMenuItem");
            adaptersToolStripMenuItem.Click += adaptersToolStripMenuItem_Click;
            // 
            // toolStripSeparator6
            // 
            toolStripSeparator6.Name = "toolStripSeparator6";
            resources.ApplyResources(toolStripSeparator6, "toolStripSeparator6");
            // 
            // toolStripMenuItem_FlushDnsCache
            // 
            toolStripMenuItem_FlushDnsCache.Image = Properties.Resources.flushdns;
            toolStripMenuItem_FlushDnsCache.Name = "toolStripMenuItem_FlushDnsCache";
            resources.ApplyResources(toolStripMenuItem_FlushDnsCache, "toolStripMenuItem_FlushDnsCache");
            toolStripMenuItem_FlushDnsCache.Click += toolStripMenuItem_FlushDnsCache_Click;
            // 
            // toolStripMenuItem_ResetWindowSize
            // 
            toolStripMenuItem_ResetWindowSize.Name = "toolStripMenuItem_ResetWindowSize";
            resources.ApplyResources(toolStripMenuItem_ResetWindowSize, "toolStripMenuItem_ResetWindowSize");
            toolStripMenuItem_ResetWindowSize.Click += toolStripMenuItem_ResetWindowSize_Click;
            // 
            // toolStripSeparator4
            // 
            toolStripSeparator4.Name = "toolStripSeparator4";
            resources.ApplyResources(toolStripSeparator4, "toolStripSeparator4");
            // 
            // toolStripMenuItem_About
            // 
            toolStripMenuItem_About.Name = "toolStripMenuItem_About";
            resources.ApplyResources(toolStripMenuItem_About, "toolStripMenuItem_About");
            // 
            // imageListProxies
            // 
            imageListProxies.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            imageListProxies.ImageStream = (System.Windows.Forms.ImageListStreamer)resources.GetObject("imageListProxies.ImageStream");
            imageListProxies.TransparentColor = System.Drawing.Color.Transparent;
            imageListProxies.Images.SetKeyName(0, "disable.png");
            imageListProxies.Images.SetKeyName(1, "enable.png");
            // 
            // saveFileDialog_Export
            // 
            resources.ApplyResources(saveFileDialog_Export, "saveFileDialog_Export");
            // 
            // openFileDialog_Import
            // 
            openFileDialog_Import.FileName = "openFileDialog1";
            resources.ApplyResources(openFileDialog_Import, "openFileDialog_Import");
            // 
            // PortProxyGUI
            // 
            resources.ApplyResources(this, "$this");
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(listViewProxies);
            Name = "PortProxyGUI";
            FormClosing += PortProxyGUI_FormClosing;
            Load += PortProxyGUI_Load;
            Shown += PortProxyGUI_Shown;
            Resize += PortProxyGUI_Resize;
            contextMenuStrip_RightClick.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip_RightClick;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_New;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_Delete;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_Refresh;
        private System.Windows.Forms.ColumnHeader columnHeader5;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_About;
        private System.Windows.Forms.ColumnHeader columnHeader6;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_Modify;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ImageList imageListProxies;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_Enable;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_Disable;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ColumnHeader columnHeader7;
        internal System.Windows.Forms.ListView listViewProxies;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_More;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.SaveFileDialog saveFileDialog_Export;
        private System.Windows.Forms.OpenFileDialog openFileDialog_Import;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_ResetWindowSize;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_Import;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_Export;
        private System.Windows.Forms.ToolStripMenuItem externalAppsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem adaptersToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
        private System.Windows.Forms.ToolStripMenuItem windowsFirewallToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem windowsFirewallToolStripMenuItem1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator7;
        private System.Windows.Forms.ToolStripMenuItem windowsFirewallControlWFCToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem rulesPanelToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem connectionsPanelToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem basicToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem advancedToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem clearToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_FlushDnsCache;
        private System.Windows.Forms.ToolStripMenuItem dockerToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator8;
    }
}

