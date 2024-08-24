using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using PenumbraModForwarder.UI.ViewModels;
using ReactiveUI;

namespace PenumbraModForwarder.UI.Views
{
    public partial class ArchivePasswordWindow : Form, IViewFor<ArchivePasswordViewModel>
    {
        public ArchivePasswordViewModel ViewModel { get; set; }

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (ArchivePasswordViewModel)value;
        }

        public ArchivePasswordWindow(ArchivePasswordViewModel viewModel)
        {
            InitializeComponent();
            ViewModel = viewModel;

            this.WhenActivated(disposables =>
            {
                this.BindCommand(ViewModel, vm => vm.ConfirmInputCommand, v => v.confim_Button)
                    .DisposeWith(disposables);

                this.Bind(ViewModel, vm => vm.FileName, v => v.file_Label.Text)
                    .DisposeWith(disposables);

                this.Bind(ViewModel, vm => vm.Password, v => v.password_TextBox.Text)
                    .DisposeWith(disposables);
            });
        }
    }
}
