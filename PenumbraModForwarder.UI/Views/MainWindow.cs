﻿using System.Reactive.Disposables;
using PenumbraModForwarder.Common.Interfaces;
using PenumbraModForwarder.UI.Services;
using PenumbraModForwarder.UI.ViewModels;
using ReactiveUI;

namespace PenumbraModForwarder.UI.Views;

public partial class MainWindow : Form, IViewFor<MainWindowViewModel>
{
    public MainWindowViewModel ViewModel { get; set; }
    private readonly ITrayNotificationService _notificationService;
    
    object IViewFor.ViewModel
    {
        get => ViewModel;
        set => ViewModel = (MainWindowViewModel) value;
    }
    
    public MainWindow(MainWindowViewModel viewModel, ITrayNotificationService notificationService)
    {
        InitializeComponent();

        ViewModel = viewModel;
        _notificationService = notificationService;


        this.WhenActivated(disposables =>
        {
            this.Bind(ViewModel, vm => vm.SelectedFolderPath, v => v.directory_text.Text)
                .DisposeWith(disposables);
            
            this.BindCommand(ViewModel, vm => vm.OpenFolderDialog, v => v.select_directory)
                .DisposeWith(disposables);
            
            
            this.Bind(ViewModel, vm => vm.AutoDelete, v => v.autodelete_checkbox.Checked)
                .DisposeWith(disposables);
            
            autodelete_checkbox.CheckedChanged += (s, e) =>
            {
                ViewModel.UpdateAutoDeleteCommand.Execute(autodelete_checkbox.Checked).Subscribe();
            };
            
            this.Bind(ViewModel, vm => vm.AutoLoad, v => v.autoforward_checkbox.Checked)
                .DisposeWith(disposables);
            
            autoforward_checkbox.CheckedChanged += (s, e) =>
            {
                ViewModel.UpdateAutoLoadCommand.Execute(autoforward_checkbox.Checked).Subscribe();
            };
            
            this.Bind(ViewModel, vm => vm.ExtractAll, v => v.extractall_checkbox.Checked)
                .DisposeWith(disposables);

            extractall_checkbox.CheckedChanged += (s, e) =>
            {
                ViewModel.UpdateExtractAllCommand.Execute(extractall_checkbox.Checked).Subscribe();
            };
            
            this.Bind(ViewModel, vm => vm.SelectBoxEnabled, v => v.select_directory.Enabled)
                .DisposeWith(disposables);

            #region Link Buttons
            
            this.BindCommand(ViewModel, vm => vm.OpenXivArchiveCommand, v => v.xivarchive_Button)
                .DisposeWith(disposables);
            
            this.BindCommand(ViewModel, vm => vm.OpenGlamourDresserCommand, v => v.glamourdressor_Button)
                .DisposeWith(disposables);
            
            this.BindCommand(ViewModel, vm => vm.OpenNexusModsCommand, v => v.nexusmods_Button)
                .DisposeWith(disposables);
            
            this.BindCommand(ViewModel, vm => vm.OpenAetherLinkCommand, v => v.aetherlink_Button)
                .DisposeWith(disposables);
            
            this.BindCommand(ViewModel, vm => vm.OpenHelioSphereCommand, v => v.heliosphere_Button)
                .DisposeWith(disposables);
            
            this.BindCommand(ViewModel, vm => vm.OpenPrettyKittyCommand, v => v.prettykitty_Button)
                .DisposeWith(disposables);
            
            this.BindCommand(ViewModel, vm => vm.OpenDiscordCommand, v => v.discord_Button)
                .DisposeWith(disposables);
            
            this.BindCommand(ViewModel, vm => vm.OpenDonateCommand, v => v.donate_Button)
                .DisposeWith(disposables);

            #endregion
            
            // Register the notification service for disposal
            disposables.Add(_notificationService);
        });
    }
}