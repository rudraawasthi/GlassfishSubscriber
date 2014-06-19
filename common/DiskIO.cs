using System;
using System.IO;

namespace GlassfishSubscriber
{
    public static class DiskIO
    {
        /// <summary>
        /// Saves the messages on the disk
        /// </summary>
        /// <param name="data">message</param>
        /// <param name="configuration">configuration to be used for saving</param>
        public static string Write(string filePath, byte[] data)
        {           
            File.WriteAllBytes(filePath, data);

            return filePath;
        }

        public static byte[] Read(string filePath)
        {
            return File.ReadAllBytes(filePath);
        }

        public static string CreateTempDirectory(string storageLocation)
        {
            string dirPath = DiskIOHelper.GetTempDirectoryFullName(storageLocation);

            if (!Directory.Exists(dirPath))
                Directory.CreateDirectory(dirPath);

            return dirPath;
        }

        public static void DeleteTemporaryDirectory(string storageLocation)
        {
            string dirPath = DiskIOHelper.GetTempDirectoryFullName(storageLocation);

            if (Directory.Exists(dirPath))
                Directory.Delete(dirPath, true);
        }
    }

    public static class DiskIOHelper
    {
        public static string TimeStamp
        {
            get
            {
                return DateTime.Now.ToString(AppConfigConstants.TIMESTAMPFORMAT);
            }
        }        

        public static string GetTempFileFullName(string storageLocation, string baseFileName, string destinationName, string messageType)
        {
            string fileName = string.Concat(AppConfigConstants.TEMPDIRECTORYNAME, AppConfigConstants.FILENAMESEPARATOR, destinationName, AppConfigConstants.FILENAMESEPARATOR, TimeStamp, ".", messageType);

            string tempDirectory = Path.Combine(storageLocation, AppConfigConstants.TEMPDIRECTORYNAME);

            string fullPath = Path.Combine(tempDirectory, fileName);

            return fullPath;
        }

        public static string GetTempDirectoryFullName(string storageLocation)
        {
            return Path.Combine(storageLocation, AppConfigConstants.TEMPDIRECTORYNAME);
        }
    }
}
