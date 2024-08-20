﻿using System.Reactive.Disposables;
using System.Reactive.Linq;
using PenumbraModForwarder.Common.Interfaces;
using PenumbraModForwarder.UI.Services;
using PenumbraModForwarder.UI.ViewModels;
using ReactiveUI;

namespace PenumbraModForwarder.UI.Views;

public partial class MainWindow : Form, IViewFor<MainWindowViewModel>
{
    private readonly ISystemTrayManager _systemTrayManager;
    private readonly ToolTip _toolTip;
    public MainWindowViewModel ViewModel { get; set; }
    
    object IViewFor.ViewModel
    {
        get => ViewModel;
        set => ViewModel = (MainWindowViewModel) value;
    }
    
    public MainWindow(MainWindowViewModel viewModel, ISystemTrayManager systemTrayManager)
    {
        InitializeComponent();

        ViewModel = viewModel;
        _systemTrayManager = systemTrayManager;
        
        _toolTip = new ToolTip();


        this.WhenActivated(disposables =>
        {
            this.Bind(ViewModel, vm => vm.VersionNumber, v => v.version_Label.Text)
                .DisposeWith(disposables);
            
            this.Bind(ViewModel, vm => vm.SelectedFolderPath, v => v.directory_text.Text)
                .DisposeWith(disposables);
            
            this.BindCommand(ViewModel, vm => vm.OpenFolderDialog, v => v.select_directory)
                .DisposeWith(disposables);

            this.Bind(ViewModel, vm => vm.FileLinkingEnabled, v => v.associate_Checkbox.Checked)
                .DisposeWith(disposables);
            
            associate_Checkbox.CheckedChanged += (s, e) =>
            {
                ViewModel.EnableFileLinkingCommand.Execute(associate_Checkbox.Checked).Subscribe();
            };
            
            this.Bind(ViewModel, vm => vm.NotificationEnabled, v => v.notification_checkbox.Checked)
                .DisposeWith(disposables);
            
            notification_checkbox.CheckedChanged += (s, e) =>
            {
                ViewModel.UpdateNotificationCommand.Execute(notification_checkbox.Checked).Subscribe();
            };
            
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

            #region Tool Tips

            _toolTip.SetToolTip(select_directory, "Select the folder where the game is installed.");
            _toolTip.SetToolTip(notification_checkbox, "Show a notification when a mod is forwarded.");
            _toolTip.SetToolTip(autodelete_checkbox, "Automatically delete the mod after forwarding to penumbra.");
            _toolTip.SetToolTip(autoforward_checkbox, "Automatically forward the mod to penumbra.");
            _toolTip.SetToolTip(extractall_checkbox, "Extract all files from the mod archive.");
            _toolTip.SetToolTip(xivarchive_Button, "Open the XIV Archive Website.");
            _toolTip.SetToolTip(glamourdressor_Button, "Open the Glamour Dresser Website.");
            _toolTip.SetToolTip(nexusmods_Button, "Open the Nexus Mods Website.");
            _toolTip.SetToolTip(aetherlink_Button, "Open the Aether Link Website.");
            _toolTip.SetToolTip(heliosphere_Button, "Open the Heliosphere Website.");
            _toolTip.SetToolTip(prettykitty_Button, "Open the Pretty Kitty Website.");
            _toolTip.SetToolTip(discord_Button, "Open the Discord Support Server.");
            _toolTip.SetToolTip(donate_Button, "Donate to the Developer.");
            _toolTip.SetToolTip(version_Label, "Current Version of Penumbra Mod Forwarder.");
            _toolTip.SetToolTip(associate_Checkbox, "Let Penumbra Mod Forwarder handle mod files when double clicked.");

            #endregion

            #region Handling Window State

            Observable.FromEventPattern<EventArgs>(this, nameof(Resize))
                .Select(_ => WindowState)
                .Where(state => state == FormWindowState.Minimized)
                .Subscribe(_ =>
                {
                    Hide();
                    _systemTrayManager.ShowNotification("Penumbra Mod Forwarder", "Minimized to tray.");
                })
                .DisposeWith(disposables);

            #endregion

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
            
        });
    }
}