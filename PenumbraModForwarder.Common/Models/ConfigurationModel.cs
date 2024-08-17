using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PenumbraModForwarder.Common.Models
{
    public class ConfigurationModel
    {
        public bool AutoLoad { get; set; }
        public bool AutoDelete { get; set; }
        public bool ExtractAll { get; set; }
        public string DownloadPath { get; set; } = string.Empty;
        public string TexToolPath { get; set; } = string.Empty;
    }
}
