using System.Diagnostics;
using System.Net;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Lanchat.Core.Encryption;

namespace Lanchat.Avalonia
{
    public class MainWindow : Window
    {
        private TextBox input = null!;
        private StackPanel chat = null!;
        private ScrollViewer chatScroll = null!;

        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            input = this.FindControl<TextBox>("Input");
            chat = this.FindControl<StackPanel>("Chat");
            chatScroll = this.FindControl<ScrollViewer>("ChatScroll");
            input.AddHandler(KeyDownEvent, OnInput!, RoutingStrategies.Tunnel);

            Lanchat.Start(x =>
            {
                x.Instance.Messaging.MessageReceived += (_, s) => { AddMessage(x.Instance.User.Nickname, s); };
            });
            AddMessage("Fingerprint: ", RsaFingerprint.GetMd5(Lanchat.Network.LocalRsa.Rsa.ExportRSAPrivateKey()));
        }

        private void OnInput(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Return)
            {
                return;
            }

            var message = input.Text;
            Lanchat.Network.Broadcast.SendMessage(message);
            AddMessage(Lanchat.Config.Nickname, message);
            input.Text = string.Empty;
            chatScroll.ScrollToEnd();
        }

        private void AddMessage(string nickname, string message)
        {
            Dispatcher.UIThread.InvokeAsync(() =>
                chat.Children.Add(new TextBlock
                {
                    Text = $"{nickname}: {message}"
                }));
        }
    }
}