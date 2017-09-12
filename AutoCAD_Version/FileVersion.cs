namespace AutoCAD_Version
{
    /// <summary>
    /// Class to hold a combination of filename and version
    /// neither are editable so do not require NotifyOnPropertyChanged
    /// </summary>
    public class FileVersion
    {
        /// <summary>
        /// Name of file
        /// </summary>
        public string Filename { get; set; }
        /// <summary>
        /// Version of file
        /// </summary>
        public string Version { get; set; }
    }
}