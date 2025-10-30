using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataObjects
{
    public enum ArchiveType
    {
        None,
        Huffman,
        SlidingWindow,
        Zip,
        Rar,
        SevenZip,
        Tar,
        GZip,
        BZip2,
        XZ        
    }
    public static class ArchiveTypeExtensions
    {
        public static string GetExtension(this ArchiveType type)
        {
            switch (type)
            {                
                case ArchiveType.Huffman: return ".huff";
                case ArchiveType.SlidingWindow: return ".swc";
                default: return string.Empty;
            }

        }
    }
}
