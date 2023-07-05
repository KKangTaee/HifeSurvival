
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
            SuspendLayout();
            // 
            // startgameBtn
            // 
            startgameBtn.Enabled = false;
            startgameBtn.Location = new System.Drawing.Point(93, 85);
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
            testBtn.Location = new System.Drawing.Point(93, 114);
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
            label1.Location = new System.Drawing.Point(12, 9);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(95, 15);
            label1.TabIndex = 2;
            label1.Text = "Current Status : ";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(104, 9);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(80, 15);
            label2.TabIndex = 3;
            label2.Text = "Disconnected";
            // 
            // connectBtn
            // 
            connectBtn.Location = new System.Drawing.Point(12, 85);
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
            label3.Location = new System.Drawing.Point(12, 34);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(66, 15);
            label3.TabIndex = 5;
            label3.Text = "Empty Info";
            // 
            // Form1
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(192, 357);
            Controls.Add(label3);
            Controls.Add(connectBtn);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(testBtn);
            Controls.Add(startgameBtn);
            Name = "Form1";
            Text = "Form1";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Button startgameBtn;
        private System.Windows.Forms.Button testBtn;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button connectBtn;
        private System.Windows.Forms.Label label3;
    }
}

