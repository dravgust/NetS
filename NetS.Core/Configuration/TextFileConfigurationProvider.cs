using System;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace NetS.Core.Configuration
{
    public class TextFileConfigurationSource : FileConfigurationSource
    {
        public override IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            this.EnsureDefaults(builder);
            return new TextFileConfigurationProvider(this);
        }

        public class TextFileConfigurationProvider : FileConfigurationProvider
        {
            public TextFileConfigurationProvider(FileConfigurationSource source) : base(source)
            {

            }

            public override void Load(Stream stream)
            {
                using (var sr = new StreamReader(stream))
                {
                    Load(sr.ReadToEnd());
                }
            }

            public void Load(string data)
            {
                this.Data.Clear();

                int lineNumber = 0;
                // Process all lines, even if empty.
                foreach (string l in data.Split(new[] {"\r\n", "\n"}, StringSplitOptions.None))
                {
                    // Track line numbers, also for empty lines.
                    lineNumber++;
                    string line = l.Trim();

                    // From here onwards don't process empty or commented lines.
                    if (string.IsNullOrEmpty(line) || line.StartsWith("#"))
                        continue;

                    // Split on the FIRST "=".
                    // This will allow mime-encoded - data strings end in one or more "=" to be parsed.
                    string[] split = line.Split('=');
                    if (split.Length == 1)
                        throw new FormatException("Line " + lineNumber + $": \"{l}\" : No value is set");

                    // Add to dictionary. Trim spaces around keys and values.
                    string key = split[0].Trim();
                    if (!key.StartsWith("-"))
                        key = "-" + key;

                    this.Add(key, string.Join("=", split.Skip(1)).Trim());
                }
            }

            private void Add(string key, string value)
            {
                key = key.ToLowerInvariant();

                if (!this.Data.ContainsKey(key))
                {
                    this.Data.Add(key, value);
                }
                else
                {
                    this.Data[key] = value;
                }
            }
        }
    }
}
