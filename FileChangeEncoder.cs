using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace NetworkFileSync
{
    class FileChangeEncoder : IEncoder<FileChange>
    {
        public byte[] Encode(FileChange fileChange)
        {
            using (var memoryStream = new MemoryStream())
            {
                new BinaryFormatter().Serialize(memoryStream, fileChange);
                return memoryStream.ToArray();
            }
        }

        public FileChange Decode(byte[] data)
        {
            using (var memoryStream = new MemoryStream(data))
            {
                return (FileChange)new BinaryFormatter().Deserialize(memoryStream);
            }
        }
    }
}