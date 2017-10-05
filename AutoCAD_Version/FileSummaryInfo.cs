using System.Collections.Generic;

namespace AutoCAD_Version
{
    /// <summary>
    /// Immutable struct for passing file summary info around
    /// </summary>
    internal struct FileSummaryInfo
    {
        public int NumberOfDWGFiles { get; }
        public IEnumerable<(string, int)> VersionsFound { get; }

        public FileSummaryInfo(int numberOfDWGFiles, IEnumerable<(string, int)> versionsFound)
        {
            NumberOfDWGFiles = numberOfDWGFiles;
            VersionsFound = versionsFound;
        }
    }
}