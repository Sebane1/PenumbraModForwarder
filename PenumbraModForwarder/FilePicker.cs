using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FFXIVVoicePackCreator {
    public partial class FilePicker : UserControl {
        public FilePicker() {
            InitializeComponent();
        }

        int index = 0;
        bool isSaveMode = false;
        bool isSwappable = true;
        bool isPlayable = true;
        public event EventHandler OnFileSelected;

        string filter;
        private Point startPos;
        private bool canDoDragDrop;
        private bool ignoreClear;
        private bool muteState;
        private int maxTime = 4000;
        private Color color;
        private string currentPath;

        [Category("Filter"), Description("Changes what type of selection is made")]
        public string Filter { get => filter; set => filter = value; }

        public string CurrentPath { get => currentPath; set { currentPath = value; filePath.Text = value; } }

        private void filePicker_Load(object sender, EventArgs e) {
            color = BackColor;
            AutoScaleDimensions = new SizeF(96, 96);
            labelName.Text = (index == -1 ? Name : ($"({index})  " + Name));
            filePath.AllowDrop = true;
        }
        private void filePicker_MouseDown(object sender, MouseEventArgs e) {
            startPos = e.Location;
            canDoDragDrop = true;
        }

        private void filePicker_MouseMove(object sender, MouseEventArgs e) {
            if ((e.X != startPos.X || startPos.Y != e.Y) && canDoDragDrop) {
                this.ParentForm.TopMost = true;
                List<string> fileList = new List<string>();
                if (!string.IsNullOrEmpty(filePath.Text)) {
                    fileList.Add(filePath.Text);
                }
                if (fileList.Count > 0) {
                    DataObject fileDragData = new DataObject(DataFormats.FileDrop, fileList.ToArray());
                    DoDragDrop(fileDragData, DragDropEffects.Copy);
                }
                canDoDragDrop = false;
                this.ParentForm.BringToFront();
            }
            this.ParentForm.TopMost = false;
        }
        private void openButton_Click(object sender, EventArgs e) {
            if (!isSaveMode) {
                FolderBrowserDialog openFileDialog = new FolderBrowserDialog();
                if (openFileDialog.ShowDialog() == DialogResult.OK) {
                    filePath.Text = openFileDialog.SelectedPath;
                    currentPath = openFileDialog.SelectedPath;
                }
            }
            if (OnFileSelected != null) {
                OnFileSelected.Invoke(this, EventArgs.Empty);
            }
        }

        private void useGameFileCheckBox_CheckedChanged(object sender, EventArgs e) {

        }

        private void filePath_TextChanged(object sender, EventArgs e) {

        }

        private void filePath_DragEnter(object sender, DragEventArgs e) {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
            else
                e.Effect = DragDropEffects.None;
        }

        private void filePath_DragDrop(object sender, DragEventArgs e) {
            string file = ((string[])e.Data.GetData(DataFormats.FileDrop, false))[0];
            filePath.Text = file;
            currentPath = file;
            if (OnFileSelected != null) {
                OnFileSelected.Invoke(this, EventArgs.Empty);
            }
        }

        private void playButton_Click(object sender, EventArgs e) {

        }

        private void filePath_Enter(object sender, EventArgs e) {

        }

        private void filePath_KeyPress(object sender, KeyPressEventArgs e) {
            if (e.KeyChar == 13) {
                currentPath = FilePath.Text;
                if (OnFileSelected != null) {
                    OnFileSelected.Invoke(this, EventArgs.Empty);
                }
            }
        }

        private void labelName_Click(object sender, EventArgs e) {

        }

        private void filePath_Leave(object sender, EventArgs e) {
            filePath.Text = filePath.Text;
            currentPath = filePath.Text;
            if (OnFileSelected != null) {
                OnFileSelected.Invoke(this, EventArgs.Empty);
            }
        }
    }
}


