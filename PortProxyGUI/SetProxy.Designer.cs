namespace PortProxyGUI
{
    partial class SetProxy
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SetProxy));
            label_ListenOn = new System.Windows.Forms.Label();
            label_ConnectTo = new System.Windows.Forms.Label();
            textBox_ConnectTo = new System.Windows.Forms.TextBox();
            textBox_ConnectPort = new System.Windows.Forms.TextBox();
            label_ConnectPort = new System.Windows.Forms.Label();
            button_Set = new System.Windows.Forms.Button();
            label_Type = new System.Windows.Forms.Label();
            label_ListenPort = new System.Windows.Forms.Label();
            textBox_ListenPort = new System.Windows.Forms.TextBox();
            comboBox_Type = new System.Windows.Forms.ComboBox();
            label_Comment = new System.Windows.Forms.Label();
            textBox_Comment = new System.Windows.Forms.TextBox();
            label_Group = new System.Windows.Forms.Label();
            comboBox_Group = new System.Windows.Forms.ComboBox();
            comboBox_ListenOn = new System.Windows.Forms.ComboBox();
            SuspendLayout();
            // 
            // label_ListenOn
            // 
            resources.ApplyResources(label_ListenOn, "label_ListenOn");
            label_ListenOn.Name = "label_ListenOn";
            // 
            // label_ConnectTo
            // 
            resources.ApplyResources(label_ConnectTo, "label_ConnectTo");
            label_ConnectTo.Name = "label_ConnectTo";
            // 
            // textBox_ConnectTo
            // 
            resources.ApplyResources(textBox_ConnectTo, "textBox_ConnectTo");
            textBox_ConnectTo.Name = "textBox_ConnectTo";
            // 
            // textBox_ConnectPort
            // 
            textBox_ConnectPort.AutoCompleteCustomSource.AddRange(new string[] { resources.GetString("textBox_ConnectPort.AutoCompleteCustomSource"), resources.GetString("textBox_ConnectPort.AutoCompleteCustomSource1"), resources.GetString("textBox_ConnectPort.AutoCompleteCustomSource2"), resources.GetString("textBox_ConnectPort.AutoCompleteCustomSource3"), resources.GetString("textBox_ConnectPort.AutoCompleteCustomSource4"), resources.GetString("textBox_ConnectPort.AutoCompleteCustomSource5"), resources.GetString("textBox_ConnectPort.AutoCompleteCustomSource6"), resources.GetString("textBox_ConnectPort.AutoCompleteCustomSource7"), resources.GetString("textBox_ConnectPort.AutoCompleteCustomSource8"), resources.GetString("textBox_ConnectPort.AutoCompleteCustomSource9"), resources.GetString("textBox_ConnectPort.AutoCompleteCustomSource10"), resources.GetString("textBox_ConnectPort.AutoCompleteCustomSource11"), resources.GetString("textBox_ConnectPort.AutoCompleteCustomSource12"), resources.GetString("textBox_ConnectPort.AutoCompleteCustomSource13") });
            textBox_ConnectPort.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            textBox_ConnectPort.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            resources.ApplyResources(textBox_ConnectPort, "textBox_ConnectPort");
            textBox_ConnectPort.Name = "textBox_ConnectPort";
            // 
            // label_ConnectPort
            // 
            resources.ApplyResources(label_ConnectPort, "label_ConnectPort");
            label_ConnectPort.Name = "label_ConnectPort";
            // 
            // button_Set
            // 
            resources.ApplyResources(button_Set, "button_Set");
            button_Set.Name = "button_Set";
            button_Set.UseVisualStyleBackColor = true;
            button_Set.Click += button_Set_Click;
            // 
            // label_Type
            // 
            resources.ApplyResources(label_Type, "label_Type");
            label_Type.Name = "label_Type";
            // 
            // label_ListenPort
            // 
            resources.ApplyResources(label_ListenPort, "label_ListenPort");
            label_ListenPort.Name = "label_ListenPort";
            // 
            // textBox_ListenPort
            // 
            textBox_ListenPort.AutoCompleteCustomSource.AddRange(new string[] { resources.GetString("textBox_ListenPort.AutoCompleteCustomSource"), resources.GetString("textBox_ListenPort.AutoCompleteCustomSource1"), resources.GetString("textBox_ListenPort.AutoCompleteCustomSource2"), resources.GetString("textBox_ListenPort.AutoCompleteCustomSource3"), resources.GetString("textBox_ListenPort.AutoCompleteCustomSource4"), resources.GetString("textBox_ListenPort.AutoCompleteCustomSource5"), resources.GetString("textBox_ListenPort.AutoCompleteCustomSource6"), resources.GetString("textBox_ListenPort.AutoCompleteCustomSource7"), resources.GetString("textBox_ListenPort.AutoCompleteCustomSource8"), resources.GetString("textBox_ListenPort.AutoCompleteCustomSource9"), resources.GetString("textBox_ListenPort.AutoCompleteCustomSource10"), resources.GetString("textBox_ListenPort.AutoCompleteCustomSource11"), resources.GetString("textBox_ListenPort.AutoCompleteCustomSource12"), resources.GetString("textBox_ListenPort.AutoCompleteCustomSource13") });
            textBox_ListenPort.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            textBox_ListenPort.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            resources.ApplyResources(textBox_ListenPort, "textBox_ListenPort");
            textBox_ListenPort.Name = "textBox_ListenPort";
            // 
            // comboBox_Type
            // 
            comboBox_Type.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            resources.ApplyResources(comboBox_Type, "comboBox_Type");
            comboBox_Type.FormattingEnabled = true;
            comboBox_Type.Items.AddRange(new object[] { resources.GetString("comboBox_Type.Items"), resources.GetString("comboBox_Type.Items1"), resources.GetString("comboBox_Type.Items2"), resources.GetString("comboBox_Type.Items3"), resources.GetString("comboBox_Type.Items4") });
            comboBox_Type.Name = "comboBox_Type";
            // 
            // label_Comment
            // 
            resources.ApplyResources(label_Comment, "label_Comment");
            label_Comment.Name = "label_Comment";
            // 
            // textBox_Comment
            // 
            resources.ApplyResources(textBox_Comment, "textBox_Comment");
            textBox_Comment.Name = "textBox_Comment";
            // 
            // label_Group
            // 
            resources.ApplyResources(label_Group, "label_Group");
            label_Group.Name = "label_Group";
            // 
            // comboBox_Group
            // 
            comboBox_Group.AutoCompleteCustomSource.AddRange(new string[] { resources.GetString("comboBox_Group.AutoCompleteCustomSource"), resources.GetString("comboBox_Group.AutoCompleteCustomSource1"), resources.GetString("comboBox_Group.AutoCompleteCustomSource2"), resources.GetString("comboBox_Group.AutoCompleteCustomSource3"), resources.GetString("comboBox_Group.AutoCompleteCustomSource4"), resources.GetString("comboBox_Group.AutoCompleteCustomSource5"), resources.GetString("comboBox_Group.AutoCompleteCustomSource6") });
            comboBox_Group.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            comboBox_Group.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            resources.ApplyResources(comboBox_Group, "comboBox_Group");
            comboBox_Group.FormattingEnabled = true;
            comboBox_Group.Name = "comboBox_Group";
            // 
            // comboBox_ListenOn
            // 
            comboBox_ListenOn.AutoCompleteCustomSource.AddRange(new string[] { resources.GetString("comboBox_ListenOn.AutoCompleteCustomSource"), resources.GetString("comboBox_ListenOn.AutoCompleteCustomSource1") });
            comboBox_ListenOn.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            comboBox_ListenOn.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            comboBox_ListenOn.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
            resources.ApplyResources(comboBox_ListenOn, "comboBox_ListenOn");
            comboBox_ListenOn.FormattingEnabled = true;
            comboBox_ListenOn.Items.AddRange(new object[] { resources.GetString("comboBox_ListenOn.Items"), resources.GetString("comboBox_ListenOn.Items1"), resources.GetString("comboBox_ListenOn.Items2") });
            comboBox_ListenOn.Name = "comboBox_ListenOn";
            // 
            // SetProxy
            // 
            AcceptButton = button_Set;
            resources.ApplyResources(this, "$this");
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(comboBox_ListenOn);
            Controls.Add(comboBox_Group);
            Controls.Add(label_Group);
            Controls.Add(textBox_Comment);
            Controls.Add(label_Comment);
            Controls.Add(comboBox_Type);
            Controls.Add(textBox_ListenPort);
            Controls.Add(label_ListenPort);
            Controls.Add(label_Type);
            Controls.Add(button_Set);
            Controls.Add(label_ConnectPort);
            Controls.Add(textBox_ConnectPort);
            Controls.Add(textBox_ConnectTo);
            Controls.Add(label_ConnectTo);
            Controls.Add(label_ListenOn);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "SetProxy";
            TopMost = true;
            FormClosing += SetProxyForm_FormClosing;
            Load += SetProxyForm_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label label_ListenOn;
        private System.Windows.Forms.Label label_ConnectTo;
        private System.Windows.Forms.TextBox textBox_ConnectTo;
        private System.Windows.Forms.TextBox textBox_ConnectPort;
        private System.Windows.Forms.Label label_ConnectPort;
        private System.Windows.Forms.Button button_Set;
        private System.Windows.Forms.Label label_Type;
        private System.Windows.Forms.Label label_ListenPort;
        private System.Windows.Forms.TextBox textBox_ListenPort;
        private System.Windows.Forms.ComboBox comboBox_Type;
        private System.Windows.Forms.Label label_Comment;
        private System.Windows.Forms.TextBox textBox_Comment;
        private System.Windows.Forms.Label label_Group;
        private System.Windows.Forms.ComboBox comboBox_Group;
        private System.Windows.Forms.ComboBox comboBox_ListenOn;
    }
}