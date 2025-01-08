using System;
using Avalonia.Controls;
using NLog;

namespace PenumbraModForwarder.UI.Views
{
    public partial class ErrorWindow : Window
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public ErrorWindow()
        {
            InitializeComponent();

            var titleBar = this.FindControl<Grid>("TitleBar");
            titleBar.PointerPressed += (s, e) =>
            {
                if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
                {
                    BeginMoveDrag(e);
                }
            };

            this.Get<Button>("CloseButton").Click += (s, e) =>
            {
                _logger.Info("Close button clicked");
                Close();
            };
        }

        protected override void OnClosing(WindowClosingEventArgs e)
        {
            base.OnClosing(e);
            _logger.Info("Window closing");
            Environment.Exit(0);
        }
    }
}