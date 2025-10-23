using DataAccessInterface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess
{
    public class FileWriter : FileWriterInterface
    {
        public bool WriteCompressedFile(byte[] compressedData, string outputPath)
        {
            bool success = false;

            try
            {
                // Check if directory exists, if not create it
                string directory = Path.GetDirectoryName(outputPath);
                if (directory != null && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory); // Create.
                }

                // Write file data.
                File.WriteAllBytes(outputPath, compressedData);
                success = true;
            }
            catch (UnauthorizedAccessException)
            {
                throw new Exception("Permission denied at this write location.");
            }
            catch (DirectoryNotFoundException) // This should not occur due to the directory creation step, but just in case.
            {
                throw new Exception("The specified directory was not found.");
            }
            catch (IOException ioEx)
            {
                throw new Exception($"An I/O error occurred: {ioEx.Message}");
            }
            catch (Exception ex)
            {
                throw new Exception($"An unexpected error occurred: {ex.Message}");
            }

            return success;
        }

        public bool WriteDecompressedFile(byte[] decompressedData, string outputPath)
        {
            if (string.IsNullOrWhiteSpace(outputPath))
                throw new ArgumentException("Output path cannot be null or empty.", nameof(outputPath));

            if (decompressedData == null || decompressedData.Length == 0)
                throw new ArgumentException("Decompressed data cannot be null or empty.", nameof(decompressedData));

            bool success = false;

            try
            {
                File.WriteAllBytes(outputPath, decompressedData);
                success = true;
            }
            catch (Exception ex)
            {
                throw new IOException("An error occurred while writing the decompressed file.", ex);
            }
            return success;
        }
    }
}
