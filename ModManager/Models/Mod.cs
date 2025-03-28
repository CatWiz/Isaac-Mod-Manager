using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Avalonia.Controls.Selection;

namespace ModManager.Models;

public record Mod(string FolderName, ModMetadata? Metadata = null)
{
    public string DisplayName => Metadata?.Name ?? FolderName;
    public static Mod Load(string path)
    {
        if (!Directory.Exists(path))
        {
            throw new ArgumentException("The mod metadata file does not exist.");
        }
        
        var xmlPath = Path.Combine(path, "metadata.xml");
        if (File.Exists(xmlPath))
        {
            var serializer = new XmlSerializer(typeof(ModMetadata));
            using var reader = new StreamReader(xmlPath);
            var obj = serializer.Deserialize(reader);
            if (obj is not ModMetadata metadata)
            {
                throw new InvalidOperationException("The metadata file is invalid.");
            }

            return new Mod(Path.GetFileName(path), metadata);
        }

        return new Mod(Path.GetFileName(path));
    }
    public static List<Mod> GetModsList(string modsFolderPath)
    {
        if (!Directory.Exists(modsFolderPath))
        {
            throw new ArgumentException("The mods folder does not exist.");
        }
        
        return Directory.GetDirectories(modsFolderPath)
            .Select(folder => Load(Path.Combine(modsFolderPath, folder)))
            .ToList();
    }
}