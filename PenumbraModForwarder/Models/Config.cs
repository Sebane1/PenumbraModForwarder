using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFXIVModExractor.Models
{
    public class Config
    {
        public bool AutoLoad { get; set; }
        public bool AutoDelete { get; set; }
        public string DownloadPath { get; set; } = string.Empty;
        public string TexToolPath { get; set; } = string.Empty;
    }
}
