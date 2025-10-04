using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DataObjects.enums;
using LogicLayerInterface;



namespace LogicLayer
{
    public class ArchiveTypeChecker : IArchiveTypeChecker
    {
                    

        // Date Created: 10/3/2025 10:22 PM
        // Last Modified: N/A
        // Description: Retruns the archive type based on file signature
        public ArchiveType GetArchiveType(string filePath)
        {            
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
                return ArchiveType.None;

            byte[] header = new byte[512]; // read enough for TAR checks too
            using (var stream = File.OpenRead(filePath))
            {
                stream.Read(header, 0, header.Length);
            }

            // ZIP: PK..
            if (header.Length >= 4 &&
                header[0] == 0x50 && header[1] == 0x4B &&
                (header[2] == 0x03 || header[2] == 0x05 || header[2] == 0x07))
            {
                return ArchiveType.Zip;
            }

            // RAR v4: 52 61 72 21 1A 07 00
            // RAR v5: 52 61 72 21 1A 07 01 00
            if (header.Length >= 7 &&
                header[0] == 0x52 && header[1] == 0x61 &&
                header[2] == 0x72 && header[3] == 0x21 &&
                header[4] == 0x1A && header[5] == 0x07 &&
                (header[6] == 0x00 || (header.Length > 7 && header[6] == 0x01 && header[7] == 0x00)))
            {
                return ArchiveType.Rar;
            }

            // 7z: 37 7A BC AF 27 1C
            if (header.Length >= 6 &&
                header[0] == 0x37 && header[1] == 0x7A &&
                header[2] == 0xBC && header[3] == 0xAF &&
                header[4] == 0x27 && header[5] == 0x1C)
            {
                return ArchiveType.SevenZip;
            }

            // GZip: 1F 8B
            if (header.Length >= 2 &&
                header[0] == 0x1F && header[1] == 0x8B)
            {
                return ArchiveType.GZip;
            }

            // BZip2: 42 5A 68 ("BZh")
            if (header.Length >= 3 &&
                header[0] == 0x42 && header[1] == 0x5A && header[2] == 0x68)
            {
                return ArchiveType.BZip2;
            }

            // XZ: FD 37 7A 58 5A 00
            if (header.Length >= 6 &&
                header[0] == 0xFD && header[1] == 0x37 &&
                header[2] == 0x7A && header[3] == 0x58 &&
                header[4] == 0x5A && header[5] == 0x00)
            {
                return ArchiveType.XZ;
            }

            // TAR: look for "ustar" at offset 0x101
            if (header.Length > 0x105 &&
                header[0x101] == (byte)'u' &&
                header[0x102] == (byte)'s' &&
                header[0x103] == (byte)'t' &&
                header[0x104] == (byte)'a' &&
                header[0x105] == (byte)'r')
            {
                return ArchiveType.Tar;
            }

            // if no supported signature is found.
            return ArchiveType.None;
        }
    }
}
