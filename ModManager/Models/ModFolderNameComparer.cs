using System;
using System.Collections.Generic;

namespace ModManager.Models;
public class ModFolderNameComparer : Comparer<Mod>, IEqualityComparer<Mod>
{
    public override int Compare(Mod? x, Mod? y)
    {
        if (x == null || y == null) return 0;
        return string.Compare(x.DisplayName, y.DisplayName, StringComparison.Ordinal);
    }

    public bool Equals(Mod? x, Mod? y)
    {
        return Compare(x, y) == 0;
    }

    public int GetHashCode(Mod obj)
    {
        return HashCode.Combine(obj.FolderName, obj.Metadata);
    }
}
