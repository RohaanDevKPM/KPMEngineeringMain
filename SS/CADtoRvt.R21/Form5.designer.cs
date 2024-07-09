namespace KPMEngineeringB.R
{
    partial class Form5
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form5));
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.cb_CAD_Name = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.cb_CAD_Layer = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.cb_Line_Type = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.cb_Line_Style = new System.Windows.Forms.ComboBox();
            this.button1 = new System.Windows.Forms.Button();
            this.btn_OK = new System.Windows.Forms.Button();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.bt_Finish = new System.Windows.Forms.Button();
            this.flowLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.label1);
            this.flowLayoutPanel1.Controls.Add(this.cb_CAD_Name);
            this.flowLayoutPanel1.Controls.Add(this.label2);
            this.flowLayoutPanel1.Controls.Add(this.cb_CAD_Layer);
            this.flowLayoutPanel1.Controls.Add(this.label3);
            this.flowLayoutPanel1.Controls.Add(this.cb_Line_Type);
            this.flowLayoutPanel1.Controls.Add(this.label4);
            this.flowLayoutPanel1.Controls.Add(this.cb_Line_Style);
            this.flowLayoutPanel1.Location = new System.Drawing.Point(8, 12);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(466, 230);
            this.flowLayoutPanel1.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(178, 20);
            this.label1.TabIndex = 0;
            this.label1.Text = "Select CAD File Name";
            // 
            // cb_CAD_Name
            // 
            this.cb_CAD_Name.BackColor = System.Drawing.SystemColors.Window;
            this.cb_CAD_Name.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cb_CAD_Name.FormattingEnabled = true;
            this.cb_CAD_Name.Location = new System.Drawing.Point(3, 23);
            this.cb_CAD_Name.Name = "cb_CAD_Name";
            this.cb_CAD_Name.Size = new System.Drawing.Size(460, 26);
            this.cb_CAD_Name.TabIndex = 1;
            this.cb_CAD_Name.SelectedIndexChanged += new System.EventHandler(this.cb_CAD_Name_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 52);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(193, 20);
            this.label2.TabIndex = 2;
            this.label2.Text = "Select CAD Layer Name";
            // 
            // cb_CAD_Layer
            // 
            this.cb_CAD_Layer.BackColor = System.Drawing.SystemColors.Window;
            this.cb_CAD_Layer.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cb_CAD_Layer.FormattingEnabled = true;
            this.cb_CAD_Layer.Location = new System.Drawing.Point(3, 75);
            this.cb_CAD_Layer.Name = "cb_CAD_Layer";
            this.cb_CAD_Layer.Size = new System.Drawing.Size(460, 26);
            this.cb_CAD_Layer.TabIndex = 3;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(3, 104);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(134, 20);
            this.label3.TabIndex = 4;
            this.label3.Text = "Select Line Type";
            // 
            // cb_Line_Type
            // 
            this.cb_Line_Type.BackColor = System.Drawing.SystemColors.Window;
            this.cb_Line_Type.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cb_Line_Type.FormattingEnabled = true;
            this.cb_Line_Type.Location = new System.Drawing.Point(3, 127);
            this.cb_Line_Type.Name = "cb_Line_Type";
            this.cb_Line_Type.Size = new System.Drawing.Size(460, 26);
            this.cb_Line_Type.TabIndex = 5;
            this.cb_Line_Type.SelectedIndexChanged += new System.EventHandler(this.cb_Line_Type_SelectedIndexChanged);
            // 
            // label6
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(3, 156);
            this.label4.Name = "label6";
            this.label4.Size = new System.Drawing.Size(135, 20);
            this.label4.TabIndex = 10;
            this.label4.Text = "Select Line Style";
            // 
            // cb_Line_Style
            // 
            this.cb_Line_Style.FormattingEnabled = true;
            this.cb_Line_Style.Location = new System.Drawing.Point(3, 179);
            this.cb_Line_Style.Name = "cb_Line_Style";
            this.cb_Line_Style.Size = new System.Drawing.Size(460, 28);
            this.cb_Line_Style.TabIndex = 13;
            // 
            // button1
            // 
            this.button1.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button1.Location = new System.Drawing.Point(318, 311);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(115, 40);
            this.button1.TabIndex = 1;
            this.button1.Text = "Cancel";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // btn_OK
            // 
            this.btn_OK.Location = new System.Drawing.Point(160, 260);
            this.btn_OK.Name = "btn_OK";
            this.btn_OK.Size = new System.Drawing.Size(150, 40);
            this.btn_OK.TabIndex = 0;
            this.btn_OK.Text = "Create Lines";
            this.btn_OK.UseVisualStyleBackColor = true;
            this.btn_OK.Click += new System.EventHandler(this.btn_OK_Click);
            // 
            // linkLabel1
            // 
            this.linkLabel1.AutoSize = true;
            this.linkLabel1.Location = new System.Drawing.Point(4, 378);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(139, 20);
            this.linkLabel1.TabIndex = 2;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = "KPM-Engineering";
            this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
            // 
            // bt_Finish
            // 
            this.bt_Finish.Location = new System.Drawing.Point(39, 311);
            this.bt_Finish.Name = "bt_Finish";
            this.bt_Finish.Size = new System.Drawing.Size(115, 40);
            this.bt_Finish.TabIndex = 12;
            this.bt_Finish.Text = "Finish";
            this.bt_Finish.UseVisualStyleBackColor = true;
            this.bt_Finish.Click += new System.EventHandler(this.bt_Finish_Click);
            // 
            // Form5
            // 
            this.AcceptButton = this.btn_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
            this.CancelButton = this.button1;
            this.ClientSize = new System.Drawing.Size(478, 409);
            this.Controls.Add(this.bt_Finish);
            this.Controls.Add(this.linkLabel1);
            this.Controls.Add(this.btn_OK);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.flowLayoutPanel1);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "Form5";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "CAD to Revit Lines";
            this.Load += new System.EventHandler(this.Form5_Load);
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cb_CAD_Name;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cb_CAD_Layer;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button btn_OK;
        private System.Windows.Forms.LinkLabel linkLabel1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox cb_Line_Type;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button bt_Finish;
        private System.Windows.Forms.ComboBox cb_Line_Style;
    }
}