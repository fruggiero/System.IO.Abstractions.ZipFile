using System.IO.Compression;

namespace System.IO.Abstractions.ZipFile
{
    public static class FsAbstractionsExtensions
    {
        private static readonly ZipAbstraction ZipInstance = new ZipAbstraction();

        public static IZip Zip(this IFileSystem fs) => GetAbstraction(fs);

        private static IZip GetAbstraction(IFileSystem fs)
        {
            ZipInstance.Fs = fs;
            return ZipInstance;
        }

        private class ZipAbstraction : IZip
        {
            internal IFileSystem Fs { get; set; }

            public void CreateFromDirectory(string source, string destination)
            {
                using(var stream = Fs.FileStream.Create(destination, FileMode.CreateNew))
                using (var archive = new ZipArchive(stream, ZipArchiveMode.Update))
                {
                    foreach (var file in Fs.Directory.GetFiles(source))
                    {
                        var relativePath = source.EndsWith(@"\")
                            ? file.Substring(source.Length, file.Length - source.Length)
                            : file.Substring(source.Length + 1, file.Length - source.Length - 1);

                        var entry = archive.CreateEntry(relativePath);
                        using(var entryStream = entry.Open())
                        using (var fileStream = Fs.File.Open(file, FileMode.Open))
                        {
                            fileStream.CopyTo(entryStream);
                        }
                    }
                }
            }
        }

        public interface IZip
        {
            void CreateFromDirectory(string source, string destination);
        }
    }
}