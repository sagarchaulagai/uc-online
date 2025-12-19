using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace uc_online
{
    public class IniConfig
    {
        private string _iniFilePath;
        private Dictionary<string, Dictionary<string, string>> _configData;

        public IniConfig(string iniFilePath = "config.ini")
        {
            _iniFilePath = iniFilePath;
            _configData = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
            LoadConfig();
        }

        public void LoadConfig()
        {
            _configData.Clear();

            if (!File.Exists(_iniFilePath))
            {
                CreateDefaultConfig();
                return;
            }

            try
            {
                string[] lines = File.ReadAllLines(_iniFilePath);
                string currentSection = "";

                foreach (string line in lines)
                {
                    string trimmedLine = line.Trim();

                    if (string.IsNullOrWhiteSpace(trimmedLine) || trimmedLine.StartsWith(";") || trimmedLine.StartsWith("#"))
                        continue;

                    if (trimmedLine.StartsWith("[") && trimmedLine.EndsWith("]"))
                    {
                        currentSection = trimmedLine.Substring(1, trimmedLine.Length - 2);
                        if (!_configData.ContainsKey(currentSection))
                        {
                            _configData[currentSection] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                        }
                        continue;
                    }

                    if (!string.IsNullOrEmpty(currentSection))
                    {
                        int equalsPos = trimmedLine.IndexOf('=');
                        if (equalsPos > 0)
                        {
                            string key = trimmedLine.Substring(0, equalsPos).Trim();
                            string value = trimmedLine.Substring(equalsPos + 1).Trim();
                            _configData[currentSection][key] = value;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading config: {ex.Message}");
                CreateDefaultConfig();
            }
        }

        private void CreateDefaultConfig()
        {
            var defaultConfig = new StringBuilder();
            defaultConfig.AppendLine("[uc-online]");
            defaultConfig.AppendLine("; Set the appID to be used here, e.g., 730 for Counter-Strike 2)");
            defaultConfig.AppendLine("; (Please note that you will want to set it to a game you can get for free that is multiplayer. Anything else, and it won't work.)");
            defaultConfig.AppendLine("; Default appID is set to 480 (Spacewar), however you can change it to any appID you want.");
            defaultConfig.AppendLine("AppID = 480");
            defaultConfig.AppendLine("");
            defaultConfig.AppendLine("; Executable needs to be set directly. Unlike the dll, there is no 'default' for the exe.");
            defaultConfig.AppendLine("; Using UE5 games as an example, the correct launcher path will look like this:");
            defaultConfig.AppendLine("; .\\game folder\\game folder\\Binaries\\Win64\\game folder-Win64-Shipping.exe");
            defaultConfig.AppendLine("GameExecutable = ");
            defaultConfig.AppendLine("");
            defaultConfig.AppendLine("; Set launch arguments where necessary - e.g., for Source Engine games like Half-Life: Source, set it to '-game hl1 -windowed' to launch it correctly.");
            defaultConfig.AppendLine("GameArguments = ");
            defaultConfig.AppendLine("");
            defaultConfig.AppendLine("; Set the path to the steam_appid.txt file to use. (If one does not exist, it will be generated with the appID set at the top.)");
            defaultConfig.AppendLine("SteamAppIdFile = steam_appid.txt");
            defaultConfig.AppendLine("");
            defaultConfig.AppendLine("; Path to steam_api.dll (leave empty to use default location - in the same folder next to the launcher.)");
            defaultConfig.AppendLine("; Only set the path as the folder containing the dll relative to the launcher.");
            defaultConfig.AppendLine("; Again, using UE5 games as an example:"); 
            defaultConfig.AppendLine("; .\\game folder\\Engine\\Binaries\\ThirdParty\\Steamworks\\Steamv153\\Win64");
            defaultConfig.AppendLine("SteamApiDLLPath = ");
            defaultConfig.AppendLine("");
            defaultConfig.AppendLine("[Logging]");
            defaultConfig.AppendLine("; Turns on logging. Not much gets logged, so it's not exactly useful. It does help with figuring out if you don't have .NET Runtime installed though.");
            defaultConfig.AppendLine("EnableLogging = true");
            defaultConfig.AppendLine("LogFile = uc-online.log");

            try
            {
                File.WriteAllText(_iniFilePath, defaultConfig.ToString());
                LoadConfig();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating default config: {ex.Message}");
            }
        }

        public void SaveConfig()
        {
            try
            {
                var configBuilder = new StringBuilder();

                foreach (var section in _configData)
                {
                    configBuilder.AppendLine($"[{section.Key}]");

                    foreach (var kvp in section.Value)
                    {
                        configBuilder.AppendLine($"{kvp.Key} = {kvp.Value}");
                    }

                    configBuilder.AppendLine();
                }

                File.WriteAllText(_iniFilePath, configBuilder.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving config: {ex.Message}");
            }
        }

        public string GetValue(string section, string key, string defaultValue = "")
        {
            if (_configData.TryGetValue(section, out var sectionData) &&
                sectionData.TryGetValue(key, out var value))
            {
                return value;
            }
            return defaultValue;
        }

        public void SetValue(string section, string key, string value)
        {
            if (!_configData.ContainsKey(section))
            {
                _configData[section] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            }

            _configData[section][key] = value;
        }

        public uint GetAppID()
        {
            string appIdStr = GetValue("uc-online", "AppID", "0");
            if (uint.TryParse(appIdStr, out uint appId) && appId > 0)
            {
                return appId;
            }
            return 0; // Return 0 to indicate no AppID is configured
        }

        public void SetAppID(uint appId)
        {
            SetValue("uc-online", "AppID", appId.ToString());
        }

        public string GetGameExecutable()
        {
            return GetValue("uc-online", "GameExecutable", "");
        }

        public void SetGameExecutable(string gameExePath)
        {
            SetValue("uc-online", "GameExecutable", gameExePath);
        }

        public string GetGameArguments()
        {
            return GetValue("uc-online", "GameArguments", "");
        }

        public void SetGameArguments(string arguments)
        {
            SetValue("uc-online", "GameArguments", arguments);
        }

        public string GetSteamApiDllPath()
        {
            return GetValue("uc-online", "SteamApiDllPath", "");
        }

        public void SetSteamApiDllPath(string dllPath)
        {
            SetValue("uc-online", "SteamApiDllPath", dllPath);
            SaveConfig();
        }
    }
}