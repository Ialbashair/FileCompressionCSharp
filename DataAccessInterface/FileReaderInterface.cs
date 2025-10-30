using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DataAccessInterface
{
    public interface FileReaderInterface
    {
        byte[] FileToByteArray(string filePath);
        Task<byte[]> ReadAllBytesAsync(string filePath, CancellationToken ct);
    }
}
