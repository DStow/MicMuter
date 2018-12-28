using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MicMuter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool _running = true;
        private bool _keysDown = false;
        public MainWindow()
        {
            InitializeComponent();

            SetupAndStartKeyboardListener();

            UpdateStatusLabel();
        }

        private void SetupAndStartKeyboardListener()
        {
            // Create thread to listen for keyboard
            Thread thread = new Thread(() =>
            {
                while (_running)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        if (Keyboard.IsKeyDown(Key.M) && Keyboard.IsKeyDown(Key.LeftShift) && _keysDown == false)
                        {
                            // Toggle Mute
                            Console.WriteLine("Keys Down!");
                            AudioManager.ToggleMasterVolumeMute();
                            UpdateStatusLabel();
                            _keysDown = true;
                        }
                        else if ((Keyboard.IsKeyUp(Key.M) || Keyboard.IsKeyUp(Key.LeftShift)) && _keysDown == true)
                        {
                            Console.WriteLine("Keys Up!");
                            _keysDown = false;
                        }
                    });

                    Thread.Sleep(50);
                }
            });

            thread.Start();
        }

        private void UpdateStatusLabel()
        {
            bool micStatus = AudioManager.GetMasterVolumeMute();
            lblStatus.Content = "Status: " + (!micStatus ? "Enabled" : "Muted");
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            _running = false;
            base.OnClosing(e);
        }
    }
}
