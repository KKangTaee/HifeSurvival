
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
            startgameBtn = new System.Windows.Forms.Button();
            testBtn = new System.Windows.Forms.Button();
            label1 = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            connectBtn = new System.Windows.Forms.Button();
            label3 = new System.Windows.Forms.Label();
            panel1 = new System.Windows.Forms.Panel();
            PlayerSpecBox = new System.Windows.Forms.GroupBox();
            ItemListTextBox = new System.Windows.Forms.RichTextBox();
            ItemTextBox = new System.Windows.Forms.Label();
            CurrencyTextBox = new System.Windows.Forms.RichTextBox();
            CurrencyLabel = new System.Windows.Forms.Label();
            label4 = new System.Windows.Forms.Label();
            StatTextBox1 = new System.Windows.Forms.RichTextBox();
            panel1.SuspendLayout();
            PlayerSpecBox.SuspendLayout();
            SuspendLayout();
            // 
            // startgameBtn
            // 
            startgameBtn.Enabled = false;
            startgameBtn.Location = new System.Drawing.Point(93, 87);
            startgameBtn.Name = "startgameBtn";
            startgameBtn.Size = new System.Drawing.Size(75, 23);
            startgameBtn.TabIndex = 0;
            startgameBtn.Text = "StartGameProcess";
            startgameBtn.UseVisualStyleBackColor = true;
            startgameBtn.Click += StartGameClick;
            // 
            // testBtn
            // 
            testBtn.Enabled = false;
            testBtn.Location = new System.Drawing.Point(174, 87);
            testBtn.Name = "testBtn";
            testBtn.Size = new System.Drawing.Size(75, 23);
            testBtn.TabIndex = 1;
            testBtn.Text = "TestPacket";
            testBtn.UseVisualStyleBackColor = true;
            testBtn.Click += TestClick;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(13, 10);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(95, 15);
            label1.TabIndex = 2;
            label1.Text = "Current Status : ";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(105, 10);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(80, 15);
            label2.TabIndex = 3;
            label2.Text = "Disconnected";
            // 
            // connectBtn
            // 
            connectBtn.Location = new System.Drawing.Point(12, 87);
            connectBtn.Name = "connectBtn";
            connectBtn.Size = new System.Drawing.Size(75, 23);
            connectBtn.TabIndex = 4;
            connectBtn.Text = "Connect";
            connectBtn.UseVisualStyleBackColor = true;
            connectBtn.Click += ConnectBtnClick;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(13, 35);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(66, 15);
            label3.TabIndex = 5;
            label3.Text = "Empty Info";
            // 
            // panel1
            // 
            panel1.BackColor = System.Drawing.SystemColors.ControlLight;
            panel1.Controls.Add(label1);
            panel1.Controls.Add(label3);
            panel1.Controls.Add(label2);
            panel1.Location = new System.Drawing.Point(12, 12);
            panel1.Name = "panel1";
            panel1.Size = new System.Drawing.Size(347, 69);
            panel1.TabIndex = 6;
            // 
            // PlayerSpecBox
            // 
            PlayerSpecBox.Controls.Add(ItemListTextBox);
            PlayerSpecBox.Controls.Add(ItemTextBox);
            PlayerSpecBox.Controls.Add(CurrencyTextBox);
            PlayerSpecBox.Controls.Add(CurrencyLabel);
            PlayerSpecBox.Controls.Add(label4);
            PlayerSpecBox.Controls.Add(StatTextBox1);
            PlayerSpecBox.Location = new System.Drawing.Point(12, 116);
            PlayerSpecBox.Name = "PlayerSpecBox";
            PlayerSpecBox.Size = new System.Drawing.Size(347, 425);
            PlayerSpecBox.TabIndex = 7;
            PlayerSpecBox.TabStop = false;
            PlayerSpecBox.Text = "PlayerSpec";
            // 
            // ItemListTextBox
            // 
            ItemListTextBox.Location = new System.Drawing.Point(6, 164);
            ItemListTextBox.Name = "ItemListTextBox";
            ItemListTextBox.ReadOnly = true;
            ItemListTextBox.Size = new System.Drawing.Size(179, 111);
            ItemListTextBox.TabIndex = 5;
            ItemListTextBox.Text = "";
            // 
            // ItemTextBox
            // 
            ItemTextBox.AutoSize = true;
            ItemTextBox.Location = new System.Drawing.Point(6, 146);
            ItemTextBox.Name = "ItemTextBox";
            ItemTextBox.Size = new System.Drawing.Size(31, 15);
            ItemTextBox.TabIndex = 4;
            ItemTextBox.Text = "Item";
            // 
            // CurrencyTextBox
            // 
            CurrencyTextBox.Location = new System.Drawing.Point(94, 42);
            CurrencyTextBox.Name = "CurrencyTextBox";
            CurrencyTextBox.ReadOnly = true;
            CurrencyTextBox.Size = new System.Drawing.Size(91, 92);
            CurrencyTextBox.TabIndex = 3;
            CurrencyTextBox.Text = "";
            // 
            // CurrencyLabel
            // 
            CurrencyLabel.AutoSize = true;
            CurrencyLabel.Location = new System.Drawing.Point(94, 24);
            CurrencyLabel.Name = "CurrencyLabel";
            CurrencyLabel.Size = new System.Drawing.Size(55, 15);
            CurrencyLabel.TabIndex = 2;
            CurrencyLabel.Text = "Currency";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new System.Drawing.Point(6, 24);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(28, 15);
            label4.TabIndex = 1;
            label4.Text = "Stat";
            // 
            // StatTextBox1
            // 
            StatTextBox1.CausesValidation = false;
            StatTextBox1.Location = new System.Drawing.Point(6, 42);
            StatTextBox1.Name = "StatTextBox1";
            StatTextBox1.ReadOnly = true;
            StatTextBox1.Size = new System.Drawing.Size(82, 92);
            StatTextBox1.TabIndex = 0;
            StatTextBox1.Text = "";
            // 
            // Form1
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(371, 553);
            Controls.Add(PlayerSpecBox);
            Controls.Add(panel1);
            Controls.Add(connectBtn);
            Controls.Add(testBtn);
            Controls.Add(startgameBtn);
            Name = "Form1";
            Text = "Form1";
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            PlayerSpecBox.ResumeLayout(false);
            PlayerSpecBox.PerformLayout();
            ResumeLayout(false);
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
    }
}

