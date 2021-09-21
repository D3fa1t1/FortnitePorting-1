using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CUE4Parse.Encryption.Aes;
using CUE4Parse.FileProvider;
using CUE4Parse.MappingsProvider;
using CUE4Parse.UE4.Objects.Core.Misc;
using CUE4Parse.UE4.Vfs;
using FortnitePorting.Cosmetics;
using FortnitePorting.Utils;
using Newtonsoft.Json;

using static FortnitePorting.Utils.SimpleLogger;

namespace FortnitePorting
{
    static class Program
    {
        public static readonly string ConfigPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");
        public static readonly string MappingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Mappings");
        public static readonly SimpleLogger Logger = new ("FortnitePorting");
        public static DefaultFileProvider Provider;
        
        private static Config _config;
        
        public static void Main(string[] args)
        {
            if (args.Length == 0) Logger.Log("No program arguments found!", ELogLevel.Critical);
            
            if (args[0] == "-fill")
            {
                UpdateKeysMappings.UpdateKeys();
                UpdateKeysMappings.UpdateMappings();
                PromptExit(0);
            }
            
            LoadConfig();
            LoadProvider();

            switch (args[0])
            {
                case "-c":
                    Character.ProcessCharacter(args[1]);
                    break;
            }
        }

        static void LoadConfig()
        {
            if (!File.Exists(ConfigPath)) Logger.Log("Config file does not exist", ELogLevel.Critical);
            
            Logger.Log($"Reading Config File {ConfigPath}");
            _config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(ConfigPath));

            if (!Directory.Exists(_config.PaksDirectory)) Logger.Log($"Paks directory {_config.PaksDirectory} does not exist", ELogLevel.Critical);
        }
        static void LoadProvider()
        {
            Provider = new DefaultFileProvider(_config.PaksDirectory, SearchOption.TopDirectoryOnly, true)
            {
                MappingsContainer = new FileUsmapTypeMappingsProvider(UpdateKeysMappings.GetNewestUsmap(MappingsPath)),
            };
            Provider.Initialize();
            Provider.UnloadAllVfs();
            Provider.SubmitKey(new FGuid(), new FAesKey(_config.MainKey));
            
            foreach (var entry in _config.DynamicKeys)
            {
                IAesVfsReader vfs = Provider.UnloadedVfs.FirstOrDefault(pak => pak.Name.Equals(entry.FileName));
                if (vfs != null) Provider.SubmitKey(vfs.EncryptionKeyGuid, new FAesKey(entry.Key));
            }
        } 
        private static void PromptExit(int code)
        {
            Console.WriteLine("Press any button to exit...");
            Console.ReadLine();
            Environment.Exit(code);
        }
        public class Config
        {
            public string PaksDirectory { get; set; }
            public string MainKey { get; set; }
            public List<KeyEntry> DynamicKeys { get; set; }

            public class KeyEntry
            {
                public string FileName { get; set; }
                public string Key { get; set; }
            }
        }
    }
} 