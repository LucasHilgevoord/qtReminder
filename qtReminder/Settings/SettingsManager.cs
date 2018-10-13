using System;
using qtReminder.Settings.Models;
using Newtonsoft.Json;
using System.IO;

namespace qtReminder.Settings
{
    public class SettingsHelper
    {
        private string fileName;
        public SettingsModel SettingsModel { get; private set; }
        
        public SettingsHelper(string fileName)
        {
            this.fileName = fileName;
            
            // Check if the file exists,
            // and if it doesn't, create the file and ask for required info.
            if (!File.Exists(fileName))
            {
                SettingsModel = new SettingsModel();

                Console.ForegroundColor = ConsoleColor.Red;
                Console.BackgroundColor = ConsoleColor.White;
                Console.WriteLine("MISSING REQUIRED VALUES.");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("Please enter the bot token: ");
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.White;
                string tokenString = Console.ReadLine();
                
                SettingsModel.ClientToken = tokenString;
                
                Console.WriteLine("\nToken accepted. If you want to change the settings, edit the settings.config file.");
                
                SaveSettings();
            }
            else LoadSettings();
        }

        private void LoadSettings()
        {
            using (var textreader = File.OpenText(fileName))
            {
                string json = textreader.ReadToEnd();
                SettingsModel = JsonConvert.DeserializeObject<SettingsModel>(json);
            }
        }

        public void SaveSettings()
        {
            string json = JsonConvert.SerializeObject(SettingsModel);
            using (var textwriter = File.CreateText(fileName))
            {
                textwriter.Write(json);
            }
        }
    }
}