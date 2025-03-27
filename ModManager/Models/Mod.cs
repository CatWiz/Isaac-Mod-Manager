using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ModManager.Models;

public record class Mod
{
    public required string FolderName { get; init; }
    public required bool Enabled { get; set; }

    public static List<Mod> GetModsList(string modsFolderPath)
    {
        if (!Directory.Exists(modsFolderPath))
        {
            throw new ArgumentException("The mods folder does not exist.");
        }
        
        return Directory.GetDirectories(modsFolderPath)
            .Select(folder => new Mod
            {
                FolderName = Path.GetFileName(folder),
                Enabled = !File.Exists(Path.Combine(folder, "disable.it"))
            })
            .ToList();
    }
}