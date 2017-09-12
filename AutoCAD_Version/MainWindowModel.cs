using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Windows.Documents;
using System.Windows.Media;

namespace AutoCAD_Version
{
    /// <summary>
    /// The model part of MVVM for the main window
    /// </summary>
    public class MainWindowModel : WpfHelpers.NotifyPropertyChangedHost
    {
        const int VERSION_NUMBER_LENGTH = 6;


        /// <summary>
        /// AutoCAD first six letters vs AutoCAD version
        /// (Loaded from App.config)
        /// </summary>
        private Dictionary<string, string> VersionLookup = null;

        #region FileList Property
        ObservableCollection<FileVersion> _FileList = new ObservableCollection<FileVersion>();
        public ObservableCollection<FileVersion> FileList
        {
            get => _FileList;
            set => SetValue(ref _FileList, value);
        }
        #endregion

        #region AutoCADPath Property
        string _AutoCadPath = "";
        public string AutoCadPath
        {
            get => _AutoCadPath;
            set => SetValue(ref _AutoCadPath, value);
        }
        #endregion

        #region StatusBarText Property
        string _StatusBarText = "";
        public string StatusBarText
        {
            get => _StatusBarText;
            set => SetValue(ref _StatusBarText, value);
        }
        #endregion

        /// <summary>
        /// Command to browse for folder
        /// </summary>
        public ICommand CmdBrowse { get; set; }
        /// <summary>
        /// Command to read files and versions from given folder
        /// </summary>
        public ICommand CmdExecute { get; set; }
        /// <summary>
        /// Command to print versions
        /// </summary>
        public ICommand CmdPrint { get; set; }

        /// <summary>
        /// Construtor, sets up commands and reads VersionLookup from App.config
        /// </summary>
        public MainWindowModel()
        {
            CmdBrowse = new WpfHelpers.GenericCommand<string>(GetAutoCADFolder, _ => true);
            CmdExecute = new WpfHelpers.GenericCommand<string>(Execute, IsValidPath);
            CmdPrint = new WpfHelpers.GenericCommand<string>(Print, IsValidPath);

            var appSettings = ConfigurationManager.AppSettings;

            VersionLookup = appSettings.Cast<string>()
                            .SelectMany(key => appSettings.GetValues(key), (key, value) => new { key, value })
                            .ToDictionary(setting => setting.key, setting => setting.value);
                       
            StatusBarText = Properties.Resources.StatusBarPrompt1;
        }

        /// <summary>
        /// Checks param path to see if it is a valid AutoCAD folder
        /// </summary>
        /// <param name="path">Path to AutoCAD folder</param>
        /// <returns>true if valid</returns>
        private bool IsValidPath(string path)
        {
            return new Validators.FolderExistsValidator()
                .Validate(path, System.Globalization.CultureInfo.CurrentCulture)
                .IsValid;
        }

        /// <summary>
        /// Uses CommonDialog to get the AutoCAD folder from the user
        /// </summary>
        /// <param name="_"></param>
        private void GetAutoCADFolder(string _)
        {
            var currentDirectory = System.IO.Directory.GetCurrentDirectory();
            if (Directory.Exists(AutoCadPath))
                currentDirectory = AutoCadPath;

            var dlg = new CommonOpenFileDialog
            {
                Title = "AutoCAD File Folder...",
                IsFolderPicker = true,
                InitialDirectory = currentDirectory,

                AddToMostRecentlyUsedList = false,
                AllowNonFileSystemItems = false,
                DefaultDirectory = currentDirectory,
                EnsureFileExists = true,
                EnsurePathExists = true,
                EnsureReadOnly = false,
                EnsureValidNames = true,
                Multiselect = false,
                ShowPlacesList = true
            };

            if (dlg.ShowDialog() == CommonFileDialogResult.Ok)
            {
                // Update the view model for the path
                AutoCadPath = dlg.FileName;
                // Path must exist if we got here (validated by CommonDialog option EnsurePathExists)
                Execute(AutoCadPath);

                StatusBarText = Properties.Resources.StatusBarPrompt2;
            }
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
                        result = Encoding.Default.GetString(buffer);
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
            return System.IO.Path.GetFileName(path);
        }

        /// <summary>
        /// Populate the ListView with a list of filenames and versions
        /// </summary>
        /// <param name="path">AutoCAD filepath</param>
        private void Execute(string path)
        {
            // AutoCAD path must already exist because the command canExecute includes a valiator

            // create a list of FileVersion records from the folder
            var fileList =
                        Directory.GetFiles(path, "*.DWG")
                                    .Select(GetFirstChars)
                                    .Select(res => new FileVersion { Filename = ReduceToFilename(res.path), Version = GetCodeFromString(res.firstChars) });
            // convert list of FileVersion to an ObservableCollection
            // so it can be displayed in the ListView
            FileList = new ObservableCollection<FileVersion>(fileList);
        }

        /// <summary>
        /// Print a list of files and versions
        /// Note: because the Can Execute of the Command Checks for
        /// for a valid path, we do not need to repeat the check here
        /// </summary>
        /// <param name="path">Folder containing the files</param>
        /// <param name="files">List of files and their versions</param>
        private void Print(string path)
        {
            (new FileVersionPrinter(FileList, AutoCadPath))
                .Print();
            StatusBarText = Properties.Resources.StatusBarPrompt1;
        }
    }
}