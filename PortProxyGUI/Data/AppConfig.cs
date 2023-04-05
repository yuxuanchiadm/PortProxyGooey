using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;

namespace PortProxyGUI.Data
{
    public class AppConfig
    {
        public Size MainWindowSize = new(720, 500);
        public int[] PortProxyColumnWidths = new int[] { 24, 64, 140, 100, 140, 100, 100 };

        private readonly Regex _intArrayRegex = new(@"^\[\s*(\d+)(?:\s*,\s*(\d+))*\s*\]$");

        public AppConfig() { }
        public AppConfig(Config[] rows)
        {
            {
                IEnumerable<Config> item = rows.Where(x => x.Item == "MainWindow");
                if (int.TryParse(item.FirstOrDefault(x => x.Key == "Width")?.Value, out int width)
                    && int.TryParse(item.FirstOrDefault(x => x.Key == "Height")?.Value, out int height))
                {
                    MainWindowSize = new Size(width, height);
                }
                else MainWindowSize = new Size(720, 500);
            }

            {
                IEnumerable<Config> item = rows.Where(x => x.Item == "PortProxy");
                string s_ColumnWidths = item.FirstOrDefault(x => x.Key == "ColumnWidths").Value;
                Match match = _intArrayRegex.Match(s_ColumnWidths);

                if (match.Success)
                {
                    PortProxyColumnWidths = match.Groups
                        .OfType<Group>().Skip(1)
                        .SelectMany(x => x.Captures.OfType<Capture>())
                        .Select(x => int.Parse(x.Value))
                        .ToArray();
                }
                else
                {
#if NETCOREAPP3_0_OR_GREATER 
                    PortProxyColumnWidths = Array.Empty<int>();
#else
                    PortProxyColumnWidths = new int[0];
#endif
                }
            }
        }

    }
}
