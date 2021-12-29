using System.IO;

namespace NetworkFileSync
{
    internal class FileChange
    {
        public string Name { get; internal set; }
        public WatcherChangeTypes Type { get; internal set; }
        public string Content { get; internal set; }
    }
}