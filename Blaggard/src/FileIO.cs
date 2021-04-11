using System;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace Blaggard {
  public struct JsonSave {
    public string Entry { get; init; }
    public string Data { get; init; }
  }

  public static class ZipUtils {
    public static void SaveJsonToZip(string path, params JsonSave[] jsonToSave) {
      using FileStream file = new(path, FileMode.OpenOrCreate);
      using ZipArchive archive = new(file, ZipArchiveMode.Update);
      foreach (var save in jsonToSave) {
        var entry = archive.GetEntry(save.Entry);
        if (entry != null) {
          entry.Delete();
        }
        entry = archive.CreateEntry(save.Entry);

        using Stream s = entry.Open();
        s.Write(Encoding.UTF8.GetBytes(save.Data));
      }
    }

    public static string LoadJsonFromZip(string path, string entryName) {
      string result = null;
      using (ZipArchive archive = ZipFile.Open(path, ZipArchiveMode.Read)) {
        var entry = archive.GetEntry(entryName);
        using StreamReader sr = new(entry.Open());
        result = sr.ReadToEnd();
      }
      return result ?? throw new Exception("Failed to read from save file!");
    }

    public static bool TryLoadJsonFromZip(string path, string entryName, out string json) {
      json = null;
      using (ZipArchive archive = ZipFile.Open(path, ZipArchiveMode.Read)) {
        var entry = archive.GetEntry(entryName);
        if (entry == null) return false;

        using StreamReader sr = new(entry.Open());
        json = sr.ReadToEnd();
      }
      return true;
    }

    public static bool JsonEntryExistsInZip(string path, string entryName) {
      bool found = false;
      using (ZipArchive archive = ZipFile.Open(path, ZipArchiveMode.Read)) {
        found = archive.GetEntry(entryName) != null;
      }
      return found;
    }
  }
}
