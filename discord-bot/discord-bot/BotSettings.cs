using System;
using System.Collections.Generic;
using System.IO;

namespace bot
{
    internal class BotSettings
    {
        public bool TryGetString(string name, out string val)
        {
            return _data.TryGetValue(name, out val);
        }

        public bool TryGetInt(string name, out int val)
        {
            if (_data.TryGetValue(name, out var strVal) && int.TryParse(strVal, out val))
            {
                return true;
            }
            else
            {
                val = 0;
                return false;
            }
        }

        public static BotSettings Load(string path)
        {
            BotSettings settings = new();

            try
            {
                using (var file = new StreamReader(path))
                {
                    var content = file.ReadToEnd();
                    var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);

                    int i = 0;
                    foreach (var line in lines)
                    {
                        if (line.Trim().StartsWith('#'))
                        {
                            continue;
                        }

                        var keyVal = line.Split('=', StringSplitOptions.RemoveEmptyEntries);

                        if (keyVal.Length > 2)
                        {
                            Console.WriteLine(@"Line {i} has more than one '=' on it. Only the first two entries will be loaded.");
                        }
                        else if (keyVal.Length < 2)
                        {
                            Console.WriteLine(@"Ignoring incorrect line {i} (Check missing '=').");
                            continue;
                        }

                        settings._data[keyVal[0].Trim()] = keyVal[1].Trim();

                        ++i;
                    }
                }
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("Settings have not been found!");
            }

            return settings;
        }

        public static void Save(string path, BotSettings settings)
        {
            using (var file = new StreamWriter(path))
            {
                file.WriteLine("# This file was written automatically ");
                foreach (var keyVal in settings._data)
                {
                    file.WriteLine(keyVal.Key + "=" + keyVal.Value);
                }
            }
        }

        Dictionary<string, string> _data = new();
    }
}
