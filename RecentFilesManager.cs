using System;
using System.Collections.Generic;
using System.IO;

namespace Notepad;

public static class RecentFilesManager
{
    private static readonly string FolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LegacyNotepad");
    private static readonly string FilePath = Path.Combine(FolderPath, "recent.txt");
    private const int MaxItems = 10;

    public static List<string> LoadRecentFiles()
    {
        var list = new List<string>();
        try
        {
            if (File.Exists(FilePath))
            {
                var lines = File.ReadAllLines(FilePath);
                foreach (var line in lines)
                {
                    string trimmed = line.Trim();
                    if (!string.IsNullOrWhiteSpace(trimmed) && File.Exists(trimmed))
                    {
                        list.Add(trimmed);
                    }
                }
            }
        }
        catch { /* Silently ignore reading errors to prevent crashes */ }
        return list;
    }

    public static void AddRecentFile(string path)
    {
        try
        {
            if (!Directory.Exists(FolderPath))
            {
                Directory.CreateDirectory(FolderPath);
            }

            var list = LoadRecentFiles();
            list.Remove(path); // Avoid duplicate entries
            list.Insert(0, path); // Move most recent to top

            if (list.Count > MaxItems)
            {
                list.RemoveAt(list.Count - 1);
            }

            File.WriteAllLines(FilePath, list);
        }
        catch { }
    }
}
