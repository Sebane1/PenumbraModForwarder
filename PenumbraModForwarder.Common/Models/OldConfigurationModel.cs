﻿namespace PenumbraModForwarder.Common.Models;

public class OldConfigurationModel
{
    public bool AutoLoad { get; set; }
    public bool AutoDelete { get; set; }
    public bool AllowChoicesBeforeExtractingArchive { get; set; }
    public string DownloadPath { get; set; } = string.Empty;
    public string TexToolPath { get; set; } = string.Empty;
}