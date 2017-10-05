using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Diagnostics.Contracts;

namespace AutoCAD_Version
{
    /// <summary>
    /// The model part of MVVM for the main window
    /// </summary>
    public class MainWindowModel : WpfHelpers.NotifyPropertyChangedHost
    {

        #region FileList Property
        private ObservableCollection<FileVersion> _fileList = new ObservableCollection<FileVersion>();
        public ObservableCollection<FileVersion> FileList
        {
            get => _fileList;
            set => SetValue(ref _fileList, value);
        }
        #endregion

        #region AutoCADPath Property
        private string _autoCadPath = "";
        public string AutoCadPath
        {
            get => _autoCadPath;
            set => SetValue(ref _autoCadPath, value);
        }
        #endregion

        #region StatusBarText Property
        private string _statusBarText = "";
        public string StatusBarText
        {
            get => _statusBarText;
            set => SetValue(ref _statusBarText, value);
        }
        #endregion

        /// <summary>
        /// Command to browse for folder
        /// </summary>
        #region CmdBrowse Property
        private ICommand _cmdBrowse = null;
        public ICommand CmdBrowse
        {
            get => _cmdBrowse;
            private set => SetValue(ref _cmdBrowse, value);
        }
        #endregion

        /// <summary>
        /// Command to print versions
        /// </summary>
        #region CmdBrowse Property
        private ICommand _cmdPrint = new WpfHelpers.GenericCommand<string>(param => System.Windows.MessageBox.Show("{param}"), _ => true);
        public ICommand CmdPrint
        {
            get => _cmdPrint;
            private set => SetValue(ref _cmdPrint, value);
        }
        #endregion

        private FileSummary _fileSummary = new FileSummary();

        /// <summary>
        /// Construtor, sets up commands and reads VersionLookup from App.config
        /// </summary>
        public MainWindowModel()
        {
            Contract.Requires(_cmdBrowse == null);
            Contract.Requires(_cmdPrint == null);

            Contract.Ensures(_cmdBrowse != null);
            Contract.Ensures(_cmdPrint != null);

            CmdBrowse = new WpfHelpers.GenericCommand<string>(GetAutoCADFolder, AlwaysTrue);
            CmdPrint = new WpfHelpers.GenericCommand<string>(Print, IsValidPath);

            StatusBarText = Properties.Resources.StatusBarPrompt1;

            /// Local function that takes a string and return true
            bool AlwaysTrue(string _) => true;
        }

        [ContractInvariantMethod]
        protected void ObjectInvariant()
        {
            Contract.Invariant(FileList != null);
            Contract.Invariant(_fileSummary != null);
        }


        [ContractAbbreviator]
        private void StateNotChanged()
        {
            Contract.Ensures(_fileList == Contract.OldValue(_fileList));
            Contract.Ensures(_fileSummary == Contract.OldValue(_fileSummary));
            Contract.Ensures(_statusBarText == Contract.OldValue(_statusBarText));
            Contract.Ensures(_autoCadPath == Contract.OldValue(_autoCadPath));
            Contract.Ensures(_cmdBrowse == Contract.OldValue(_cmdBrowse));
            Contract.Ensures(_cmdPrint == Contract.OldValue(_cmdPrint));
        }

        /// <summary>
        /// Checks param path to see if it is a valid AutoCAD folder
        /// </summary>
        /// <param name="path">Path to AutoCAD folder</param>
        /// <returns>true if valid</returns>
        [Pure]
        private bool IsValidPath(string path)
        {
            return new Validators.FolderExistsValidator()
                .Validate(path, System.Globalization.CultureInfo.CurrentCulture)
                .IsValid;
        }

        /// <summary>
        /// Uses CommonDialog to get the AutoCAD folder from the user
        /// </summary>
        /// <param name="_">paramater is not used</param>
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
                if (IsValidPath(AutoCadPath))
                {
                    FileList = Execute(AutoCadPath);
                }
                else
                {
                    FileList = new ObservableCollection<FileVersion>();
                    System.Windows.MessageBox.Show("No AutoCAD files found", "Folder not found or no files in folder");
                }
            }
        }

        /// <summary>
        /// Populate the ListView with a list of filenames and versions
        /// </summary>
        /// <param name="path">AutoCAD filepath</param>
        /// <returns>List of Files and their versions</returns>
        /// <remarks>
        /// Requires: path must be valid
        /// Requires: _fileSummary must be populated
        /// Ensures: result is not null and populated with at least one value
        /// </remarks>

        private ObservableCollection<FileVersion> Execute(string path)
        {
            Contract.Requires(IsValidPath(path), "MainWindowModel.Execute: the path must exist and contain at least one AutoCAD file");
            Contract.Requires(_fileSummary != null, "Before MainWindowModel.Execute is called _fileSummary must be populated ");

            Contract.Ensures(Contract.Result<ObservableCollection<FileVersion>>() != null);
            Contract.Ensures(Contract.Result<ObservableCollection<FileVersion>>().Count > 0);

            var summaryInfo = _fileSummary.GetSummaryInfo2();
            Contract.Assert(summaryInfo.VersionsFound != null, "summaryInfo.VersionsFound cannot be null");

            if (summaryInfo?.VersionsFound?.Count() == 1)
                StatusBarText = $"{summaryInfo?.NumberOfDWGFiles} file(s) found, Version: {summaryInfo?.VersionsFound?.FirstOrDefault().Item1}";
            else
                StatusBarText = $"Multiple versions found";

            // create a list of FileVersion records from the folder
            return _fileSummary.GetFileList(path);

        }

        /// <summary>
        /// Print a list of files and versions
        /// Note: because the Can Execute of the Command Checks for
        /// for a valid path, we do not need to repeat the check here
        /// </summary>
        /// <param name=_">Not Used</param>
        private void Print(string _)
        {
            if (FileList != null && IsValidPath(AutoCadPath))
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
}