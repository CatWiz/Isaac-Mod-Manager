using System;
using System.Collections.Generic;

namespace ModManager.Models;
public class ModFolderNameComparer : Comparer<Mod>
{
    public override int Compare(Mod? x, Mod? y)
    {
        if (x == null || y == null) return 0;
        return string.Compare(x.DisplayName, y.DisplayName, StringComparison.Ordinal);
    }
}
