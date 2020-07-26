using System.IO;
using System.IO.Compression;
using System.Text;

namespace Blaggard.FileIO
{
    public struct JsonSave
    {
        public readonly string entry;
        public readonly string data;

        public JsonSave(string entry, string data)
        {
            this.entry = entry;
            this.data = data;
        }
    }

    public static class ZipUtils
    {
        public static void SaveJsonToZip(string path, params JsonSave[] jsonToSave)
        {
            using (FileStream file = new FileStream(path, FileMode.OpenOrCreate))
            {
                using (ZipArchive archive = new ZipArchive(file, ZipArchiveMode.Update))
                {
                    foreach (var save in jsonToSave)
                    {
                        var entry = archive.GetEntry(save.entry);
                        if (entry != null)
                        {
                            entry.Delete();
                        }
                        entry = archive.CreateEntry(save.entry);

                        using (Stream s = entry.Open())
                        {
                            s.Write(Encoding.UTF8.GetBytes(save.data));
                        }
                    }
                }
            }
        }

        public static string LoadJsonFromZip(string path, string entryName)
        {
            string result = null;
            using (ZipArchive archive = ZipFile.Open(path, ZipArchiveMode.Read))
            {
                var entry = archive.GetEntry(entryName);
                using (StreamReader sr = new StreamReader(entry.Open()))
                {
                    result = sr.ReadToEnd();
                }
            }
            return result != null ? result : throw new System.Exception("Failed to read from save file!");
        }

        public static bool TryLoadJsonFromZip(string path, string entryName, out string json)
        {
            json = null;
            using (ZipArchive archive = ZipFile.Open(path, ZipArchiveMode.Read))
            {
                var entry = archive.GetEntry(entryName);
                if (entry == null) return false;

                using (StreamReader sr = new StreamReader(entry.Open()))
                {
                    json = sr.ReadToEnd();
                }
            }
            return true;
        }

        public static bool JsonEntryExistsInZip(string path, string entryName)
        {
            bool found = false;
            using (ZipArchive archive = ZipFile.Open(path, ZipArchiveMode.Read))
            {
                found = archive.GetEntry(entryName) != null;
            }
            return found;
        }
    }
}