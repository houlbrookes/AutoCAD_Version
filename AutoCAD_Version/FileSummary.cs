using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;

namespace AutoCAD_Version
{
    /// <summary>
    /// Helper class that processes a list of AutoCAD files 
    /// and determines their AutoCAD version
    /// </summary>
    internal class FileSummary
    {
        /// <summary>
        /// Internal list of files and properties
        /// </summary>
        private ObservableCollection<FileVersion> _fileList { get; set; }
        /// <summary>
        /// Read-only Dictionary version vs file header values
        /// </summary>
        public Dictionary<string, string> VersionLookup { get; }

        /// <summary>
        /// Constructor initializes the VersionLookup dictionary from the AppSetting file
        /// </summary>
        public FileSummary()
        {
            var appSettings = System.Configuration.ConfigurationManager.AppSettings;
            VersionLookup = appSettings.Cast<string>()
                            .SelectMany(key => appSettings.GetValues(key), (key, value) => new { key, value })
                            .ToDictionary(setting => setting.key, setting => setting.value);

            _fileList = new ObservableCollection<FileVersion>();
        }
        /// <summary>
        /// Get the list of filepaths and versions for a given path
        /// </summary>
        /// <param name="path">Folder containing the AutoCAD files</param>
        /// <returns>Collection of Filepath / File Versions</returns>
        public ObservableCollection<FileVersion> GetFileList(string path)
        {
            Contract.Requires(!string.IsNullOrEmpty(path));
            Contract.Ensures(Contract.Result<ObservableCollection<FileVersion>>() != null);

            try
            {
                var fileList2 =
                Directory.GetFiles(path, $"*.{PublicConstants.AUTOCAD_FILE_SUFFIX}")
                        .Select(GetFirstChars)
                        .Where(res => !string.IsNullOrEmpty(res.firstChars))
                        .Select(res => new FileVersion(ReduceToFilename(res.path), 
                                                       GetCodeFromString(res.firstChars)));
                // convert list of FileVersion to an ObservableCollection
                // so it can be displayed in the ListView
                _fileList = new ObservableCollection<FileVersion>(fileList2);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"{ex.Message}", "Error Reading files");
                _fileList = new ObservableCollection<FileVersion>();
            }

            return _fileList;
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
                        byte[] buffer = new byte[PublicConstants.VERSION_NUMBER_LENGTH];

                        fs.Read(buffer, 0, PublicConstants.VERSION_NUMBER_LENGTH);
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
        [Pure]
        private string GetCodeFromString(string versionCode)
        {
            Contract.Requires(!string.IsNullOrEmpty(versionCode));
            // No Change of state
            Contract.Ensures(VersionLookup == Contract.OldValue(VersionLookup));
            Contract.Ensures(_fileList == Contract.OldValue(_fileList));
            // Does not return a null string
            Contract.Ensures(!string.IsNullOrEmpty(Contract.Result<string>()));

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
        [Pure]
        private string ReduceToFilename(string path)
        {
            return Path.GetFileName(path);
        }

        /// <summary>
        /// Get number of files and their verisons
        /// </summary>
        /// <returns></returns>
        [Pure]
        internal FileSummaryInfo GetSummaryInfo()
        {
            Contract.Requires(_fileList != null);
            Contract.Ensures(Contract.Result<FileSummaryInfo>().VersionsFound != null);

            var list = from fv in _fileList
                       group fv by fv.Version into versionGroup
                       select (versionGroup.Key, versionGroup.Count());

            return new FileSummaryInfo(_fileList.Count(), list);
        }

        internal (int NumberOfDWGFiles, IEnumerable<(string Key, int)> VersionsFound) GetSummaryInfo2()
        {
            var list = from fv in _fileList
                       group fv by fv.Version into versionGroup
                       select (versionGroup.Key, versionGroup.Count());

            return (_fileList.Count(), list);
        }

    }


}