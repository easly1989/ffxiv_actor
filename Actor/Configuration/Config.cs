using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace Actor.Configuration
{
    public static class Config
    {
        private static readonly string ConfigurationPath;
        private static readonly List<Component> Components;

        static Config()
        {
            Components = new List<Component>();
            ConfigurationPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");
            if(!File.Exists(ConfigurationPath))
            {
                try
                {
                    File.Create(ConfigurationPath);
                }
                catch (Exception)
                {
                    throw new Exception("Unable to create configuration file.");
                }
            }

            Load();
        }

        public static bool HasAnyComponent()
        {
            return Components.Any();
        }

        public static Component Get(string name)
        {
            return Components.FirstOrDefault(c => c.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
        }

        public static void Load()
        {
            try
            {
                var json = File.ReadAllText(ConfigurationPath);
                Components.AddRange(JsonConvert.DeserializeObject<Component[]>(json));
            }
            catch (Exception)
            {
                throw new Exception("Unable to read configuration file.");
            }
        }

        public static void Save()
        {
            try
            {
                var json = JsonConvert.SerializeObject(Components.ToArray(), Formatting.Indented);
                File.WriteAllText(ConfigurationPath, json);
            }
            catch (Exception)
            {
                throw new Exception("Unable to save the configuration to file.");
            }
        }
    }
}
