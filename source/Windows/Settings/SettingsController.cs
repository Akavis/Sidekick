﻿using Newtonsoft.Json;
using Sidekick.Helpers;
using System;
using System.IO;
using System.Windows.Forms.Integration;
using System.Windows.Input;

namespace Sidekick.Windows.Settings
{
    public static class SettingsController
    {
        private static readonly string SETTINGS_PATH = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.json");
        private static readonly int WINDOW_WIDTH = 480;
        private static readonly int WINDOW_HEIGHT = 320;

        private static SettingsView _settingsView;
        private static Models.Settings _settings;

        public static bool IsDisplayed => _settingsView != null && _settingsView.IsDisplayed;

        public static void Show()
        {
            if (_settingsView == null)
            {
                _settingsView = new SettingsView(WINDOW_WIDTH, WINDOW_HEIGHT);
            }

            _settingsView.Activate();

            // When running a tray app with a call to 'Application.Run()' instead of opening a MainWindow
            // we need this to capture keyboard input for our window
            ElementHost.EnableModelessKeyboardInterop(_settingsView);
            _settingsView.OnWindowClosed += (s, e) => _settingsView = null;
        }

        /// <summary>
        /// Loads the default settings
        /// </summary>
        /// <returns></returns>
        private static Models.Settings LoadDefaultSettings()
        {
            try
            {
                Logger.Log("Loading default settings");
                var settings = new Models.Settings();

                /* KeybindSettings */
                settings.KeybindSettings.Add(Models.KeybindSetting.CloseWindow, new Models.Hotkey(System.Windows.Forms.Keys.Escape, System.Windows.Forms.Keys.None));
                settings.KeybindSettings.Add(Models.KeybindSetting.PriceCheck, new Models.Hotkey(System.Windows.Forms.Keys.D, System.Windows.Forms.Keys.Control));
                settings.KeybindSettings.Add(Models.KeybindSetting.Hideout, new Models.Hotkey(System.Windows.Forms.Keys.F5, System.Windows.Forms.Keys.None));
                settings.KeybindSettings.Add(Models.KeybindSetting.ItemWiki, new Models.Hotkey(System.Windows.Forms.Keys.W, System.Windows.Forms.Keys.Alt));
                settings.KeybindSettings.Add(Models.KeybindSetting.FindItems, new Models.Hotkey(System.Windows.Forms.Keys.F, System.Windows.Forms.Keys.Control));
                settings.KeybindSettings.Add(Models.KeybindSetting.LeaveParty, new Models.Hotkey(System.Windows.Forms.Keys.F4, System.Windows.Forms.Keys.None));

                /* GeneralSettings */
                settings.GeneralSettings.Add(Models.GeneralSetting.CharacterName, string.Empty);

                // #TODO: Add more default settings
                return settings;
            }
            catch (Exception)
            {
                Logger.Log("Could not load default settings", LogState.Error);
                throw;
            }
        }

        public static Models.Settings GetSettingsInstance()
        {
            if(_settings == null)
            {
                return LoadSettings();
            }
            else
            {
                return _settings;
            }
        }

        public static void CaptureKeyEvents(System.Windows.Forms.Keys key, System.Windows.Forms.Keys modifier)
        {
            _settingsView.CaptureKeyEvents(key, modifier);
        }

        /// <summary>
        /// Loads the settings from the settings file. If no settings file exists, loads default settings
        /// </summary>
        /// <returns></returns>
        public static Models.Settings LoadSettings()
        {
            try
            {
                Logger.Log("Loading settings");
                string settingsString = null;
                if (File.Exists(SETTINGS_PATH))
                {
                    settingsString = File.ReadAllText(SETTINGS_PATH);
                }

                //If settings have never been initialized, create new settings, otherwise clear and reload settings
                if (_settings == null) _settings = new Models.Settings();
                else _settings.Clear();

                if (string.IsNullOrEmpty(settingsString))
                {
                    _settings = LoadDefaultSettings();
                }
                else
                {
                    _settings = JsonConvert.DeserializeObject<Models.Settings>(settingsString);
                    // TODO: Add new settings, that aren't in the settings file yet
                }
                return _settings;
            }
            catch (Exception)
            {
                Logger.Log("Could not load settings", LogState.Error);
                throw;
            }
        }

        /// <summary>
        /// Backups the current settings file and saves settings
        /// </summary>
        public static void SaveSettings()
        {
            try
            {
                Logger.Log("Saving settings");
				
                // Backup old settings
                if (File.Exists(SETTINGS_PATH))
                {
                    File.Copy(SETTINGS_PATH, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.json.old"), true);
                    File.Delete(SETTINGS_PATH);
                }

                string settingsString = JsonConvert.SerializeObject(_settings, Formatting.Indented);
                File.WriteAllText(SETTINGS_PATH, settingsString);
            }
            catch (Exception)
            {
                Logger.Log("Could not save settings", LogState.Error);
                throw;
            }
        }
    }
}
