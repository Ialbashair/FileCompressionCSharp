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
                default: return string.Empty;
            }
        }
    }
}
