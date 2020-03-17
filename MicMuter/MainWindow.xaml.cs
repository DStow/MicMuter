using NAudio.Wave;
using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Input;

namespace MicMuter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Used to check if the app is still running so threads can know when to close
        private bool _running = true;

        // Keeps track if the shortcut key is currently pressed down so the mute / unmute toggle is not cosntantly toggled
        private bool _shortcutKeyDown = false;

        // The shortcut key that triggers the mute toggle
        private Key _shortcutKey = Key.Oem8;

        // Config file that stores access to shortcut key access
        private Code.ShortcutConfig _configFile;

        // Icon that displays in the tray area, it changes colours based on the state of the microphone
        private System.Windows.Forms.NotifyIcon _trayIcon;

        public MainWindow()
        {
            InitializeComponent();

            // Setup the config file by getting the directory the executable is running in
            _configFile = new Code.ShortcutConfig(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\config.xml");

            // Load the shortcut key from the config file
            string shortcutKey = _configFile.GetShortcutKeyValue();
            _shortcutKey = GetKeyFromString(shortcutKey);

            // Display a tray icon
            SetupTrayIcon();

            // Kick of the thread for listening for key presses
            SetupAndStartKeyboardListener();

            // Update the status and shortcut label
            UpdateStatusLabel();
            UpdateShortcutLabel();
        }

        /// <summary>
        /// Initialises the notify icon and displays it in the tray
        /// </summary>
        private void SetupTrayIcon()
        {
            // Create notification icon thing
            _trayIcon = new System.Windows.Forms.NotifyIcon();
            _trayIcon.Icon = Properties.Resources.MicrophoneIcon;
            _trayIcon.Visible = true;
            _trayIcon.MouseDoubleClick += NotifyIcon_MouseDoubleClick;

            var trayContext = new System.Windows.Forms.ContextMenu();
            var trayExitButton = new System.Windows.Forms.MenuItem("Exit");
            trayExitButton.Click += (x, y) =>
            {
                this.Close();
            };

            trayContext.MenuItems.Add(trayExitButton);

            var trayToggleButton = new System.Windows.Forms.MenuItem("Toggle");
            trayToggleButton.Click += (x, y) =>
              {
                  this.ToggleStatus();
              };
            trayContext.MenuItems.Add(trayToggleButton);

            _trayIcon.ContextMenu = trayContext;
        }

        /// <summary>
        /// Miximises the application when the user double clicks on the tray icon
        /// </summary>
        private void NotifyIcon_MouseDoubleClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            Application.Current.MainWindow.WindowState = WindowState.Normal;
            Application.Current.MainWindow.BringIntoView();
            BringIntoView();
            Activate();
            Topmost = true;
        }

        /// <summary>
        /// Starts a thread to listen for keyboard inputs from the user.
        /// Once it has detected the shortcut combination it toggles the 
        /// microphone mute status
        /// </summary>
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
                        if (Keyboard.IsKeyDown(_shortcutKey) && Keyboard.IsKeyDown(Key.LeftShift) && _shortcutKeyDown == false)
                        {
                            ToggleStatus();

                            _shortcutKeyDown = true;
                        }
                        else if ((Keyboard.IsKeyUp(_shortcutKey) || Keyboard.IsKeyUp(Key.LeftShift)) && _shortcutKeyDown == true)
                        {
                            _shortcutKeyDown = false;
                        }
                    });

                    Thread.Sleep(50);
                }
            });

            thread.Start();
        }

        private void ToggleStatus()
        {
            AudioManager.ToggleMasterVolumeMute();

            UpdateStatusLabel();

            PlayMicrphoneStatusSound();
        }

        /// <summary>
        /// Update the label showing the user if hte microphone is muted or not
        /// </summary>
        private void UpdateStatusLabel()
        {
            bool micStatus = AudioManager.GetMasterVolumeMute();
            lblStatus.Content = "Status: " + (!micStatus ? "Unmuted" : "Muted");

            if (micStatus == false)
            {
                _trayIcon.Icon = Properties.Resources.MicrophoneIcon;
                _trayIcon.Text = "Enabled";
            }
            else
            {
                _trayIcon.Icon = Properties.Resources.MicrophoneRedIcon;
                _trayIcon.Text = "Disabled";
            }
        }

        /// <summary>
        /// Updates the label showing the user what the combination is to toggle the microphone
        /// </summary>
        private void UpdateShortcutLabel()
        {
            lblShortcut.Content = string.Format("Use Left Shift + {0} to toggle", Code.KeyNamer.GetKeyDisplayName(_shortcutKey));
        }

        #region Window Overrides
        protected override void OnClosing(CancelEventArgs e)
        {
            // As we are closing remove the tray icon and set the flag of running to false
            // So the keyboard listener thread will close itself
            _running = false;
            _trayIcon.Dispose();
            base.OnClosing(e);
        }

        protected override void OnDeactivated(EventArgs e)
        {
            base.OnDeactivated(e);

            // When deactivated we don't want to show the application on the taskbar
            this.ShowInTaskbar = false;
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);

            // When the application is reactivated we want to display in the taskbar
            this.ShowInTaskbar = true;
        }
        #endregion

        /// <summary>
        /// Play the sound that represents the current status of the microphone
        /// </summary>
        private void PlayMicrphoneStatusSound()
        {
            bool isMuted = AudioManager.GetMasterVolumeMute();
            if (isMuted)
            {
                PlaySound(Properties.Resources.muted);
            }
            else
            {
                PlaySound(Properties.Resources.unmuted);
            }
        }

        /// <summary>
        /// Play a sound that is passed in as a unmanaged memory stream
        /// Used with Properties.Resources for stored audio files
        /// </summary>
        /// <param name="sound"></param>
        private void PlaySound(UnmanagedMemoryStream sound)
        {
            WaveOut wave = new WaveOut();

            byte[] bytes = new byte[1024];

            IWaveProvider provider = new RawSourceWaveStream(sound, new WaveFormat());

            wave.Init(provider);
            wave.Play();
        }

        /// <summary>
        /// Handle the user clicking the change shortcut button
        /// </summary>
        private void BtnChangeShortcut_Click(object sender, RoutedEventArgs e)
        {
            // Listen for a key and then set that as teh shortcut?
            Keyboard.AddKeyDownHandler(this, KeyboardShortcut_KeyDownEvent);
            btnChangeShortcut.Content = "Press any key";
            btnChangeShortcut.IsEnabled = false;
        }

        private void btnToggleManual_Click(object sender, RoutedEventArgs e)
        {
            ToggleStatus();
        }

        /// <summary>
        /// Fired when the user is trying to change the keyboard shortcut
        /// </summary>
        private void KeyboardShortcut_KeyDownEvent(object sender, KeyEventArgs args)
        {
            // Check if it's a valid key
            // ToDo: Move invalid key numbers into a config entry?
            if (args.Key == Key.LeftShift)
            {
                return;
            }

            // Remove this event so we don't keep changing the keyboard shortcut
            Keyboard.RemoveKeyDownHandler(this, KeyboardShortcut_KeyDownEvent);
            btnChangeShortcut.Content = "Change Shortcut Key";
            btnChangeShortcut.IsEnabled = true;

            // Change the shortcut key in the config file and local variable
            UpdateKeyboardShortcutKey(args.Key);
        }

        /// <summary>
        /// Update the config file and application to look at a new shortcut key
        /// </summary>
        private void UpdateKeyboardShortcutKey(Key key)
        {
            _configFile.SetShortcutKeyValue(Convert.ToInt32(key).ToString());
            _shortcutKey = key;

            UpdateShortcutLabel();
        }

        /// <summary>
        /// Get a key from a string value (a string value of the key as an integer)
        /// </summary>
        /// <returns>Will return OEM8 if this fails</returns>
        private Key GetKeyFromString(string key)
        {
            try
            {
                Key result = (Key)Convert.ToInt32(key);
                return result;
            }
            catch
            {
                return Key.Oem8;
            }
        }
    }
}
