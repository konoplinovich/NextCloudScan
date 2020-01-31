using System;
using System.Collections.Generic;

namespace NextCloudScan.Lib
{
    public class FileItem : IEquatable<FileItem>
    {
        public string Path { get; set; }
        public DateTime LastWriteTime { get; set; }

        public override bool Equals(object obj)
        {
            return Equals(obj as FileItem);
        }

        public bool Equals(FileItem other)
        {
            return other != null &&
                   Path == other.Path &&
                   LastWriteTime == other.LastWriteTime;
        }

        public override int GetHashCode()
        {
            var hashCode = -1469020353;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Path);
            hashCode = hashCode * -1521134295 + LastWriteTime.GetHashCode();
            return hashCode;
        }

        public override string ToString()
        {
            return $"{Path} [{LastWriteTime}]";
        }
    }
}