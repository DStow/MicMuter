using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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
        private bool _microphoneEnabled = false;

        System.Windows.Forms.NotifyIcon notifyIcon;

        public MainWindow()
        {
            InitializeComponent();

            // Create notification icon thing
            notifyIcon = new System.Windows.Forms.NotifyIcon();
            notifyIcon.Icon = Properties.Resources.MicrophoneIcon;
            notifyIcon.Visible = true;
            notifyIcon.MouseDoubleClick += NotifyIcon_MouseDoubleClick;

            SetupAndStartKeyboardListener();

            UpdateStatusLabel();

            this.ShowInTaskbar = false;
        }

        private void NotifyIcon_MouseDoubleClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            Application.Current.MainWindow.WindowState = WindowState.Normal;
            Application.Current.MainWindow.BringIntoView();
            BringIntoView();Activate();
            Topmost = true;
        }

        private void SetupAndStartKeyboardListener()
        {
           // Keyboard.AddKeyDownHandler(this, Handled);
            // Create thread to listen for keyboard
            Thread thread = new Thread(() =>
            {
                while (_running)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {

                        if (Keyboard.IsKeyDown(Key.Oem8) && Keyboard.IsKeyDown(Key.LeftShift) && _keysDown == false)
                        {
                            // Toggle Mute
                            Console.WriteLine("Keys Down!");
                            AudioManager.ToggleMasterVolumeMute();
                            UpdateStatusLabel();
                            PlaySound();
                            _keysDown = true;
                        }
                        else if ((Keyboard.IsKeyUp(Key.Oem8) || Keyboard.IsKeyUp(Key.LeftShift)) && _keysDown == true)
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

        private void Handled(object sender, KeyEventArgs e)
        {
            lblStatus.Content = e.Key.ToString();
        }

        private void UpdateStatusLabel()
        {
            bool micStatus = AudioManager.GetMasterVolumeMute();
            lblStatus.Content = "Status: " + (!micStatus ? "Enabled" : "Muted");

            if(micStatus == false)
            {
                notifyIcon.Icon = Properties.Resources.MicrophoneIcon;
                notifyIcon.Text = "Enabled";
            }
            else
            {
                notifyIcon.Icon = Properties.Resources.MicrophoneRedIcon;
                notifyIcon.Text = "Disabled";
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            _running = false;
            base.OnClosing(e);
        }

        private void PlaySound()
        {
            bool micStatus = AudioManager.GetMasterVolumeMute();
            if (micStatus)
            {
                PlayMutedSound();
            }
            else
            {
                PlayUnmutedSound();
            }
        }

        private void PlayMutedSound()
        {
            NAudio.Wave.WaveOut wave = new NAudio.Wave.WaveOut();
            //var x = new AudioFileReader(Properties.Resources.muted);

            byte[] bytes = new byte[1024];

            IWaveProvider provider = new RawSourceWaveStream(
                                     Properties.Resources.muted, new WaveFormat());

            wave.Init(provider);
            wave.Play();
        }

        private void PlayUnmutedSound()
        {
            NAudio.Wave.WaveOut wave = new NAudio.Wave.WaveOut();
            //var x = new AudioFileReader(Properties.Resources.muted);

            byte[] bytes = new byte[1024];

            IWaveProvider provider = new RawSourceWaveStream(
                                     Properties.Resources.unmuted, new WaveFormat());

            wave.Init(provider);
            wave.Play();
        }
    }
}
