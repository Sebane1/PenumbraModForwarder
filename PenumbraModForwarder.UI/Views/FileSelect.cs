using System.Collections.ObjectModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using PenumbraModForwarder.UI.Models;
using ReactiveUI;

namespace PenumbraModForwarder.UI.Views
{
    public partial class FileSelect : Form, IViewFor<FileSelectViewModel>
    {
        public FileSelectViewModel ViewModel { get; set; }

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (FileSelectViewModel)value;
        }

        public FileSelect(FileSelectViewModel viewModel)
        {
            InitializeComponent();
            ViewModel = viewModel;
            ViewModel.CloseAction = () =>
            {
                DialogResult = DialogResult.OK;
                Close();
            };

            this.WhenActivated(disposables =>
            {
                BindListBox(ViewModel, vm => vm.Files, fileCheckedListBox);

                this.BindCommand(ViewModel, vm => vm.ConfirmSelectionCommand, v => v.confirmButton)
                    .DisposeWith(disposables);
                
                this.BindCommand(ViewModel, vm => vm.CancelSelectionCommand, v => v.cancel_Button)
                    .DisposeWith(disposables);
                
                this.BindCommand(ViewModel, vm => vm.SelectAllCommand, v => v.selectall_Button)
                    .DisposeWith(disposables);
                
                this.Bind(ViewModel, vm => vm.ShowAllSelectedEnabled, v => v.selectall_Button.Enabled)
                    .DisposeWith(disposables);
                
                this.Bind(ViewModel, vm => vm.ShowAllSelectedVisible, v => v.selectall_Button.Visible)
                    .DisposeWith(disposables);
                
                this.Bind(ViewModel, vm => vm.ArchiveFileName, v => v.archivefile_Label.Text)
                    .DisposeWith(disposables);
                
                this.Bind(ViewModel, vm => vm.ShowDtTextVisible, v => v.predt_Label.Visible)
                    .DisposeWith(disposables);

                fileCheckedListBox.ItemCheck += FileCheckedListBox_ItemCheck;

                // Observe changes to the SelectedFiles array and update the UI
                ViewModel.WhenAnyValue(vm => vm.SelectedFiles)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(selectedFiles =>
                    {
                        BeginInvoke(() =>
                        {
                            for (var i = 0; i < fileCheckedListBox.Items.Count; i++)
                            {
                                var fileItem = (FileItem)fileCheckedListBox.Items[i];
                                fileCheckedListBox.SetItemChecked(i, selectedFiles.Contains(fileItem.FullPath));
                            }
                        });
                    })
                    .DisposeWith(disposables);

                // Dispose event handler
                Disposable.Create(() => fileCheckedListBox.ItemCheck -= FileCheckedListBox_ItemCheck)
                    .DisposeWith(disposables);
            });
        }

        private void FileCheckedListBox_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            BeginInvoke(() =>
            {
                var checkedItems = fileCheckedListBox.Items.Cast<FileItem>()
                    .Where((item, index) => 
                        (index == e.Index ? e.NewValue == CheckState.Checked : fileCheckedListBox.GetItemChecked(index)))
                    .Select(item => item.FullPath)
                    .ToArray();

                ViewModel.SelectedFiles = checkedItems;
            });
        }

        private void BindListBox(FileSelectViewModel viewModel,
            System.Linq.Expressions.Expression<Func<FileSelectViewModel, ObservableCollection<FileItem>>> vmProperty,
            CheckedListBox listBox)
        {
            viewModel.WhenAnyValue(vmProperty)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(files =>
                {
                    BeginInvoke(() =>
                    {
                        listBox.Items.Clear();
                        foreach (var file in files)
                        {
                            listBox.Items.Add(file, false);
                        }
                    });
                });
        }
    }
}
