using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace AutoCAD_Version
{
    internal class FileSummary
    {
        /// <summary>
        /// 
        /// </summary>
        const int VERSION_NUMBER_LENGTH = 6;

        private ObservableCollection<FileVersion> fileList { get; set; }

        public Dictionary<string, string> VersionLookup { get; }

        public FileSummary()
        {
            var appSettings = System.Configuration.ConfigurationManager.AppSettings;
            VersionLookup = appSettings.Cast<string>()
                            .SelectMany(key => appSettings.GetValues(key), (key, value) => new { key, value })
                            .ToDictionary(setting => setting.key, setting => setting.value);
        }

        public ObservableCollection<FileVersion> GetFileList(string path)
        {
            var fileList2 =
                    Directory.GetFiles(path, "*.DWG")
                            .Select(GetFirstChars)
                            .Select(res => new FileVersion { Filename = ReduceToFilename(res.path),
                                                             Version = GetCodeFromString(res.firstChars) });
            // convert list of FileVersion to an ObservableCollection
            // so it can be displayed in the ListView
            fileList = new ObservableCollection<FileVersion>(fileList2);
            return fileList;
        }

        /// <summary>
        /// Read the first six characters of a binary file
        /// and combines it with the filename
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns>a tuple containing the path and first six chars</returns>
        /// <remarks>
        /// Returns any error message in the firstChars part of the tuple
        /// </remarks>
        public (string path, string firstChars) GetFirstChars(string filePath)
        {
            try
            {
                var result = "";
                if (File.Exists(filePath))
                {
                    using (FileStream fs = File.OpenRead(filePath))
                    {
                        byte[] buffer = new byte[VERSION_NUMBER_LENGTH];

                        fs.Read(buffer, 0, VERSION_NUMBER_LENGTH);
                        result = System.Text.Encoding.Default.GetString(buffer);
                    }
                }
                return (filePath, result);
            }
            catch (Exception e)
            {
                return (filePath, e.Message);
            }
        }

        /// <summary>
        /// Parse the AutoCAD version from a six character string
        /// </summary>
        /// <param name="versionCode"></param>
        /// <returns></returns>
        private string GetCodeFromString(string versionCode)
        {
            if (VersionLookup.ContainsKey(versionCode))
                return VersionLookup[versionCode];
            else
                return versionCode;
        }
        /// <summary>
        /// Returns the filename from the path
        /// </summary>
        /// <param name="path">Folder path to parse</param>
        /// <returns>folder element of the path</returns>
        private string ReduceToFilename(string path)
        {
            return Path.GetFileName(path);
        }

        public FileSummary(ObservableCollection<FileVersion> fileList)
        {
            this.fileList = fileList;
        }

        internal FileSummaryInfo GetSummaryInfo()
        {
            var list = from fv in fileList
                       group fv by fv.Version into versionGroup
                       select (versionGroup.Key, versionGroup.Count());

            return new FileSummaryInfo()
            {
                NumberOfDWGFiles = fileList.Count(),
                versionsFound = list,
            };
        }
    }

    internal class FileSummaryInfo
    {
        public int NumberOfDWGFiles = 0;
        public IEnumerable<(string, int)> versionsFound = null;
    }
}