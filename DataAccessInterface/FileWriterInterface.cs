using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessInterface
{
    public interface FileWriterInterface
    {
        bool WriteCompressedFile(byte[] compressedData, string outputPath);

        bool WriteDecompressedFile(byte[] decompressedData, string outputPath);
    }
}
