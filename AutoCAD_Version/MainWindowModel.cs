using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Collections.ObjectModel;
using System.Configuration;

namespace AutoCAD_Version
{
    /// <summary>
    /// The model part of MVVM for the main window
    /// </summary>
    public class MainWindowModel : WpfHelpers.NotifyPropertyChangedHost
    {

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
        /// Command to print versions
        /// </summary>
        public ICommand CmdPrint { get; set; }

        private FileSummary fileSummary = new FileSummary();

        /// <summary>
        /// Construtor, sets up commands and reads VersionLookup from App.config
        /// </summary>
        public MainWindowModel()
        {
            CmdBrowse = new WpfHelpers.GenericCommand<string>(GetAutoCADFolder, AlwaysTrue);
            CmdPrint = new WpfHelpers.GenericCommand<string>(Print, IsValidPath);

            StatusBarText = Properties.Resources.StatusBarPrompt1;

            /// Local function that takes a string and return true
            bool AlwaysTrue(string _) => true;
        }

        /// <summary>
        /// Checks param path to see if it is a valid AutoCAD folder
        /// </summary>
        /// <param name="path">Path to AutoCAD folder</param>
        /// <returns>true if valid</returns>
        private bool IsValidPath(string path)
        {
            System.Console.WriteLine($"IsValidPath called with: {path}");

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
            }
        }

        /// <summary>
        /// Populate the ListView with a list of filenames and versions
        /// </summary>
        /// <param name="path">AutoCAD filepath</param>
        private void Execute(string path)
        {
            // AutoCAD path must already exist because the command canExecute includes a valiator

            // create a list of FileVersion records from the folder
            var FileList = fileSummary.GetFileList(path);

            var summaryInfo = fileSummary.GetSummaryInfo();
            if (summaryInfo.versionsFound.Count() == 1)
                StatusBarText = $"{summaryInfo.NumberOfDWGFiles} file(s) found, Version: {summaryInfo.versionsFound.First().Item1}";
            else
                StatusBarText = $"Multiple versions found";
        }

        /// <summary>
        /// Print a list of files and versions
        /// Note: because the Can Execute of the Command Checks for
        /// for a valid path, we do not need to repeat the check here
        /// </summary>
        /// <param name="path">Folder containing the files</param>
        /// <param name="files">List of files and their versions</param>
        private void Print(string _)
        {
            var printResult = (new FileVersionPrinter(FileList, AutoCadPath))
                .Print();

            if (printResult.Success)
            {
                StatusBarText = Properties.Resources.StatusBarPrompt1;
            }
            else
            {
                StatusBarText = printResult.ErrorMessage;
            }
        }

    }
}