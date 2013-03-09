using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace sync.remotesynctable.filesystem
{
    internal class SyncTablePersistor
    {
        public static void Save(string repoPath, List<SyncTableEntry> syncTable)
        {
            if (!Directory.Exists(repoPath)) Directory.CreateDirectory(repoPath);

            using (var sw = new StreamWriter(Build_sync_table_filename(repoPath)))
            {
                syncTable.ForEach(entry => sw.WriteLine("{0}\t{1}\t{2}\t{3:s}", entry.RelativeFilename, entry.Id, entry.User, entry.TimeStamp));
            }
        }


        public static List<SyncTableEntry> Load(string repoPath)
        {
            if (!File.Exists(Build_sync_table_filename(repoPath))) return new List<SyncTableEntry>();

            return new List<SyncTableEntry>(
                File.ReadAllLines(Build_sync_table_filename(repoPath))
                    .Select(l => {
                                    var parts = l.Split('\t');
                                    return new SyncTableEntry{RelativeFilename=parts[0], Id=parts[1], User=parts[2], TimeStamp=DateTime.Parse(parts[3])};
                                 })
                );
        }


        private static string Build_sync_table_filename(string repoPath)
        {
            return repoPath + @"\.sync";
        }
    }
}