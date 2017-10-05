namespace AutoCAD_Version
{
    /// <summary>
    /// Struct to hold a combination of filename and version
    /// neither are editable so do not require NotifyOnPropertyChanged
    /// </summary>
    public struct FileVersion
    {
        /// <summary>
        /// Name of file
        /// </summary>
        public string Filename { get;  }
        /// <summary>
        /// Version of file
        /// </summary>
        public string Version { get;  }

        public FileVersion(string fileName, string version)
        {
            Filename = fileName;
            Version = version;
        }
    }
}