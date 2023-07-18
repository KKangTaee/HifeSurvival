
namespace TestClient
{
    partial class Form1
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
            this.startgameBtn = new System.Windows.Forms.Button();
            this.testBtn = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.connectBtn = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.PlayerSpecBox = new System.Windows.Forms.GroupBox();
            this.ItemListTextBox = new System.Windows.Forms.RichTextBox();
            this.ItemTextBox = new System.Windows.Forms.Label();
            this.CurrencyTextBox = new System.Windows.Forms.RichTextBox();
            this.CurrencyLabel = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.StatTextBox1 = new System.Windows.Forms.RichTextBox();
            this.WorldMap = new System.Windows.Forms.GroupBox();
            this.DropListLabel = new System.Windows.Forms.Label();
            this.dropItemListBox = new System.Windows.Forms.RichTextBox();
            this.LogBox = new System.Windows.Forms.GroupBox();
            this.LogTextBox = new System.Windows.Forms.RichTextBox();
            this.cheatCommandBox = new System.Windows.Forms.TextBox();
            this.cheatCommandBtn = new System.Windows.Forms.Button();
            this.panel1.SuspendLayout();
            this.PlayerSpecBox.SuspendLayout();
            this.WorldMap.SuspendLayout();
            this.LogBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // startgameBtn
            // 
            this.startgameBtn.Enabled = false;
            this.startgameBtn.Location = new System.Drawing.Point(93, 87);
            this.startgameBtn.Name = "startgameBtn";
            this.startgameBtn.Size = new System.Drawing.Size(75, 23);
            this.startgameBtn.TabIndex = 0;
            this.startgameBtn.Text = "StartGameProcess";
            this.startgameBtn.UseVisualStyleBackColor = true;
            this.startgameBtn.Click += new System.EventHandler(this.StartGameClick);
            // 
            // testBtn
            // 
            this.testBtn.Enabled = false;
            this.testBtn.Location = new System.Drawing.Point(174, 87);
            this.testBtn.Name = "testBtn";
            this.testBtn.Size = new System.Drawing.Size(75, 23);
            this.testBtn.TabIndex = 1;
            this.testBtn.Text = "TestPacket";
            this.testBtn.UseVisualStyleBackColor = true;
            this.testBtn.Click += new System.EventHandler(this.TestClick);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 10);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(95, 15);
            this.label1.TabIndex = 2;
            this.label1.Text = "Current Status : ";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(105, 10);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(80, 15);
            this.label2.TabIndex = 3;
            this.label2.Text = "Disconnected";
            // 
            // connectBtn
            // 
            this.connectBtn.Location = new System.Drawing.Point(12, 87);
            this.connectBtn.Name = "connectBtn";
            this.connectBtn.Size = new System.Drawing.Size(75, 23);
            this.connectBtn.TabIndex = 4;
            this.connectBtn.Text = "Connect";
            this.connectBtn.UseVisualStyleBackColor = true;
            this.connectBtn.Click += new System.EventHandler(this.ConnectBtnClick);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(13, 35);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(66, 15);
            this.label3.TabIndex = 5;
            this.label3.Text = "Empty Info";
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.SystemColors.ControlLight;
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.label3);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Location = new System.Drawing.Point(12, 12);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(380, 69);
            this.panel1.TabIndex = 6;
            // 
            // PlayerSpecBox
            // 
            this.PlayerSpecBox.Controls.Add(this.ItemListTextBox);
            this.PlayerSpecBox.Controls.Add(this.ItemTextBox);
            this.PlayerSpecBox.Controls.Add(this.CurrencyTextBox);
            this.PlayerSpecBox.Controls.Add(this.CurrencyLabel);
            this.PlayerSpecBox.Controls.Add(this.label4);
            this.PlayerSpecBox.Controls.Add(this.StatTextBox1);
            this.PlayerSpecBox.Location = new System.Drawing.Point(12, 116);
            this.PlayerSpecBox.Name = "PlayerSpecBox";
            this.PlayerSpecBox.Size = new System.Drawing.Size(193, 425);
            this.PlayerSpecBox.TabIndex = 7;
            this.PlayerSpecBox.TabStop = false;
            this.PlayerSpecBox.Text = "PlayerSpec";
            // 
            // ItemListTextBox
            // 
            this.ItemListTextBox.Location = new System.Drawing.Point(6, 268);
            this.ItemListTextBox.Name = "ItemListTextBox";
            this.ItemListTextBox.ReadOnly = true;
            this.ItemListTextBox.Size = new System.Drawing.Size(179, 149);
            this.ItemListTextBox.TabIndex = 5;
            this.ItemListTextBox.Text = "";
            // 
            // ItemTextBox
            // 
            this.ItemTextBox.AutoSize = true;
            this.ItemTextBox.Location = new System.Drawing.Point(6, 250);
            this.ItemTextBox.Name = "ItemTextBox";
            this.ItemTextBox.Size = new System.Drawing.Size(31, 15);
            this.ItemTextBox.TabIndex = 4;
            this.ItemTextBox.Text = "Item";
            // 
            // CurrencyTextBox
            // 
            this.CurrencyTextBox.Location = new System.Drawing.Point(6, 155);
            this.CurrencyTextBox.Name = "CurrencyTextBox";
            this.CurrencyTextBox.ReadOnly = true;
            this.CurrencyTextBox.Size = new System.Drawing.Size(179, 92);
            this.CurrencyTextBox.TabIndex = 3;
            this.CurrencyTextBox.Text = "";
            // 
            // CurrencyLabel
            // 
            this.CurrencyLabel.AutoSize = true;
            this.CurrencyLabel.Location = new System.Drawing.Point(6, 137);
            this.CurrencyLabel.Name = "CurrencyLabel";
            this.CurrencyLabel.Size = new System.Drawing.Size(55, 15);
            this.CurrencyLabel.TabIndex = 2;
            this.CurrencyLabel.Text = "Currency";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 24);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(28, 15);
            this.label4.TabIndex = 1;
            this.label4.Text = "Stat";
            // 
            // StatTextBox1
            // 
            this.StatTextBox1.CausesValidation = false;
            this.StatTextBox1.Location = new System.Drawing.Point(6, 42);
            this.StatTextBox1.Name = "StatTextBox1";
            this.StatTextBox1.ReadOnly = true;
            this.StatTextBox1.Size = new System.Drawing.Size(179, 92);
            this.StatTextBox1.TabIndex = 0;
            this.StatTextBox1.Text = "";
            // 
            // WorldMap
            // 
            this.WorldMap.Controls.Add(this.DropListLabel);
            this.WorldMap.Controls.Add(this.dropItemListBox);
            this.WorldMap.Location = new System.Drawing.Point(211, 116);
            this.WorldMap.Name = "WorldMap";
            this.WorldMap.Size = new System.Drawing.Size(181, 425);
            this.WorldMap.TabIndex = 8;
            this.WorldMap.TabStop = false;
            this.WorldMap.Text = "WorldMap";
            // 
            // DropListLabel
            // 
            this.DropListLabel.AutoSize = true;
            this.DropListLabel.Location = new System.Drawing.Point(6, 24);
            this.DropListLabel.Name = "DropListLabel";
            this.DropListLabel.Size = new System.Drawing.Size(52, 15);
            this.DropListLabel.TabIndex = 6;
            this.DropListLabel.Text = "DropList";
            // 
            // dropItemListBox
            // 
            this.dropItemListBox.Location = new System.Drawing.Point(6, 42);
            this.dropItemListBox.Name = "dropItemListBox";
            this.dropItemListBox.ReadOnly = true;
            this.dropItemListBox.Size = new System.Drawing.Size(165, 145);
            this.dropItemListBox.TabIndex = 6;
            this.dropItemListBox.Text = "";
            // 
            // LogBox
            // 
            this.LogBox.Controls.Add(this.LogTextBox);
            this.LogBox.Location = new System.Drawing.Point(398, 12);
            this.LogBox.Name = "LogBox";
            this.LogBox.Size = new System.Drawing.Size(273, 527);
            this.LogBox.TabIndex = 9;
            this.LogBox.TabStop = false;
            this.LogBox.Text = "LogBox";
            // 
            // LogTextBox
            // 
            this.LogTextBox.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.LogTextBox.ForeColor = System.Drawing.SystemColors.HotTrack;
            this.LogTextBox.Location = new System.Drawing.Point(6, 22);
            this.LogTextBox.Name = "LogTextBox";
            this.LogTextBox.ReadOnly = true;
            this.LogTextBox.Size = new System.Drawing.Size(261, 499);
            this.LogTextBox.TabIndex = 0;
            this.LogTextBox.Text = "";
            // 
            // cheatCommandBox
            // 
            this.cheatCommandBox.Location = new System.Drawing.Point(256, 87);
            this.cheatCommandBox.Name = "cheatCommandBox";
            this.cheatCommandBox.Size = new System.Drawing.Size(104, 23);
            this.cheatCommandBox.TabIndex = 10;
            // 
            // cheatCommandBtn
            // 
            this.cheatCommandBtn.BackColor = System.Drawing.Color.Red;
            this.cheatCommandBtn.ForeColor = System.Drawing.SystemColors.ControlText;
            this.cheatCommandBtn.Location = new System.Drawing.Point(363, 86);
            this.cheatCommandBtn.Name = "cheatCommandBtn";
            this.cheatCommandBtn.Size = new System.Drawing.Size(26, 24);
            this.cheatCommandBtn.TabIndex = 11;
            this.cheatCommandBtn.UseVisualStyleBackColor = false;
            this.cheatCommandBtn.Click += new System.EventHandler(this.CheatBtnClick);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(683, 551);
            this.Controls.Add(this.cheatCommandBtn);
            this.Controls.Add(this.cheatCommandBox);
            this.Controls.Add(this.LogBox);
            this.Controls.Add(this.WorldMap);
            this.Controls.Add(this.PlayerSpecBox);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.connectBtn);
            this.Controls.Add(this.testBtn);
            this.Controls.Add(this.startgameBtn);
            this.Name = "Form1";
            this.Text = "Form1";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.PlayerSpecBox.ResumeLayout(false);
            this.PlayerSpecBox.PerformLayout();
            this.WorldMap.ResumeLayout(false);
            this.WorldMap.PerformLayout();
            this.LogBox.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button startgameBtn;
        private System.Windows.Forms.Button testBtn;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button connectBtn;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.GroupBox PlayerSpecBox;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.RichTextBox StatTextBox1;
        private System.Windows.Forms.Label CurrencyLabel;
        private System.Windows.Forms.RichTextBox CurrencyTextBox;
        private System.Windows.Forms.RichTextBox ItemListTextBox;
        private System.Windows.Forms.Label ItemTextBox;
        private System.Windows.Forms.GroupBox WorldMap;
        private System.Windows.Forms.Label DropListLabel;
        private System.Windows.Forms.RichTextBox dropItemListBox;
        private System.Windows.Forms.GroupBox LogBox;
        private System.Windows.Forms.RichTextBox LogTextBox;
        private System.Windows.Forms.TextBox cheatCommandBox;
        private System.Windows.Forms.Button cheatCommandBtn;
    }
}

