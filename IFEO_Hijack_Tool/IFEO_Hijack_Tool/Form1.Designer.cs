namespace IFEO_Hijack_Tool
{
    partial class IFEO
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(IFEO));
            txtTargetApp = new TextBox();
            txtDebugger = new TextBox();
            lblTarget = new Label();
            lblDebugger = new Label();
            btnHijack = new Button();
            btnRestore = new Button();
            SuspendLayout();
            // 
            // txtTargetApp
            // 
            txtTargetApp.Location = new Point(113, 24);
            txtTargetApp.Name = "txtTargetApp";
            txtTargetApp.Size = new Size(100, 23);
            txtTargetApp.TabIndex = 0;
            // 
            // txtDebugger
            // 
            txtDebugger.Location = new Point(113, 64);
            txtDebugger.Name = "txtDebugger";
            txtDebugger.Size = new Size(100, 23);
            txtDebugger.TabIndex = 1;
            // 
            // lblTarget
            // 
            lblTarget.AutoSize = true;
            lblTarget.Location = new Point(39, 27);
            lblTarget.Name = "lblTarget";
            lblTarget.Size = new Size(68, 17);
            lblTarget.TabIndex = 2;
            lblTarget.Text = "目标程序名";
            // 
            // lblDebugger
            // 
            lblDebugger.AutoSize = true;
            lblDebugger.Location = new Point(51, 67);
            lblDebugger.Name = "lblDebugger";
            lblDebugger.Size = new Size(56, 17);
            lblDebugger.TabIndex = 3;
            lblDebugger.Text = "劫持路径";
            // 
            // btnHijack
            // 
            btnHijack.Location = new Point(39, 113);
            btnHijack.Name = "btnHijack";
            btnHijack.Size = new Size(75, 23);
            btnHijack.TabIndex = 4;
            btnHijack.Text = "劫持";
            btnHijack.UseVisualStyleBackColor = true;
            btnHijack.Click += btnHijack_Click;
            // 
            // btnRestore
            // 
            btnRestore.Location = new Point(138, 113);
            btnRestore.Name = "btnRestore";
            btnRestore.Size = new Size(75, 23);
            btnRestore.TabIndex = 5;
            btnRestore.Text = "恢复";
            btnRestore.UseVisualStyleBackColor = true;
            btnRestore.Click += btnRestore_Click;
            // 
            // IFEO
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(269, 155);
            Controls.Add(btnRestore);
            Controls.Add(btnHijack);
            Controls.Add(lblDebugger);
            Controls.Add(lblTarget);
            Controls.Add(txtDebugger);
            Controls.Add(txtTargetApp);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "IFEO";
            Text = "劫持并转移调用";
            Load += IFEO_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox txtTargetApp;
        private TextBox txtDebugger;
        private Label lblTarget;
        private Label lblDebugger;
        private Button btnHijack;
        private Button btnRestore;
    }
}
