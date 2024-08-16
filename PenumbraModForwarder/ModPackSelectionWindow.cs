namespace PenumbraModForwarder {
    public partial class ModPackSelectionWindow : Form {
        private string[] _modPackItems;

        public ModPackSelectionWindow() {
            InitializeComponent();
        }
        public string[] ModPackItems {
            get {
                return _modPackItems;
            }
            set {
                modPackOptions.Items.Clear();
                _modPackItems = value;
                foreach (var item in _modPackItems) {
                    modPackOptions.Items.Add(Path.GetFileName(item));
                }
            }
        }

        public int[] SelectedIndexes {
            get {
                List<int> results = new List<int>();
                for (int i = 0; i < modPackOptions.Items.Count; i++) {
                    if (modPackOptions.GetItemChecked(i)) {
                        results.Add(i);
                    }
                }
                return results.ToArray();
            }
        }
        private void confirmButtom_Click(object sender, EventArgs e) {
            DialogResult = DialogResult.OK;
            Hide();
        }
    }
}
