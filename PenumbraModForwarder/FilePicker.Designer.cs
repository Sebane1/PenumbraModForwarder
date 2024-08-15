using System.ComponentModel;

namespace FFXIVVoicePackCreator {
    partial class FilePicker {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            filePath = new TextBox();
            openButton = new Button();
            labelName = new Label();
            tableLayoutPanel1 = new TableLayoutPanel();
            tableLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // filePath
            // 
            filePath.Dock = DockStyle.Fill;
            filePath.Location = new Point(92, 3);
            filePath.Margin = new Padding(4, 3, 4, 3);
            filePath.Name = "filePath";
            filePath.Size = new Size(464, 23);
            filePath.TabIndex = 0;
            filePath.TextChanged += filePath_TextChanged;
            filePath.DragDrop += filePath_DragDrop;
            filePath.DragEnter += filePath_DragEnter;
            filePath.KeyPress += filePath_KeyPress;
            filePath.Leave += filePath_Leave;
            filePath.MouseDown += filePicker_MouseDown;
            filePath.MouseMove += filePicker_MouseMove;
            // 
            // openButton
            // 
            openButton.Dock = DockStyle.Fill;
            openButton.Location = new Point(564, 3);
            openButton.Margin = new Padding(4, 3, 4, 3);
            openButton.Name = "openButton";
            openButton.Size = new Size(80, 22);
            openButton.TabIndex = 1;
            openButton.Text = "Select";
            openButton.UseVisualStyleBackColor = true;
            openButton.Click += openButton_Click;
            // 
            // labelName
            // 
            labelName.AutoSize = true;
            labelName.Dock = DockStyle.Fill;
            labelName.Location = new Point(4, 0);
            labelName.Margin = new Padding(4, 0, 4, 0);
            labelName.Name = "labelName";
            labelName.Size = new Size(80, 28);
            labelName.TabIndex = 2;
            labelName.Text = "surprised";
            labelName.TextAlign = ContentAlignment.MiddleLeft;
            labelName.Click += labelName_Click;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 3;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 88F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 88F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20F));
            tableLayoutPanel1.Controls.Add(openButton, 2, 0);
            tableLayoutPanel1.Controls.Add(filePath, 1, 0);
            tableLayoutPanel1.Controls.Add(labelName, 0, 0);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 1;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Size = new Size(648, 28);
            tableLayoutPanel1.TabIndex = 5;
            // 
            // FilePicker
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            Controls.Add(tableLayoutPanel1);
            Margin = new Padding(4, 3, 4, 3);
            MinimumSize = new Size(300, 28);
            Name = "FilePicker";
            Size = new Size(648, 28);
            Load += filePicker_Load;
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private TextBox filePath;
        private Button openButton;
        private Label labelName;
        private TableLayoutPanel tableLayoutPanel1;

        public Label LabelName { get => labelName; set => labelName = value; }
        public TextBox FilePath { get => filePath; set => filePath = value; }
        [
   Category("Index"),
   Description("Sort Order")
   ]
        public int Index { get => index; set => index = value; }
    }
}
