using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DataAccessInterface;

namespace DataAccess
{
    public class FileReader : FileReaderInterface
    {
        /// <summary>
        /// Reads an entire file into a byte array (synchronous).
        /// </summary>
        public byte[] FileToByteArray(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));

            if (!File.Exists(filePath))
                throw new FileNotFoundException("File not found.", filePath);

            try
            {
                return File.ReadAllBytes(filePath);
            }
            catch (Exception ex)
            {
                throw new IOException("An error occurred while reading the file.", ex);
            }
        }

     
        public async Task<byte[]> ReadAllBytesAsync(string filePath, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));

            if (!File.Exists(filePath))
                throw new FileNotFoundException("File not found.", filePath);

            try
            {
                using (var fs = new FileStream(
                    filePath,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.Read,
                    bufferSize: 4096,
                    useAsync: true))
                {
                    long fileLength = fs.Length;
                    if (fileLength > int.MaxValue)
                        throw new IOException("File too large to read into a single byte array.");

                    byte[] buffer = new byte[fileLength];
                    int offset = 0;

                    while (offset < buffer.Length)
                    {
                        int bytesRead = await fs.ReadAsync(buffer, offset, buffer.Length - offset, ct).ConfigureAwait(false);
                        if (bytesRead == 0)
                            break; // reached EOF
                        offset += bytesRead;
                    }

                    // In case fewer bytes were read than expected
                    if (offset < buffer.Length)
                        Array.Resize(ref buffer, offset);

                    return buffer;
                }
            }
            catch (OperationCanceledException)
            {
                throw; // propagate cancellation cleanly
            }
            catch (Exception ex)
            {
                throw new IOException("An error occurred while reading the file asynchronously.", ex);
            }
        }
    }
}
