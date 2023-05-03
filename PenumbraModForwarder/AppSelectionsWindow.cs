using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PenumbraModForwarder {
    public partial class AppSelectionsWindow : Form {
        private AppSelectionType appSelection;
        public AppSelectionsWindow() {
            InitializeComponent();
            AutoScaleDimensions = new SizeF(96, 96);
            TopMost = true;
            BringToFront();
        }

        public AppSelectionType AppSelection { get => appSelection; set => appSelection = value; }

        private void penumbraButton_Click(object sender, EventArgs e) {
            appSelection = AppSelectionType.penumbra;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void textoolsButton_Click(object sender, EventArgs e) {
            appSelection = AppSelectionType.textools;
            DialogResult = DialogResult.OK;
            Close();
        }

        public enum AppSelectionType {
            penumbra,
            textools
        }
    }
}
