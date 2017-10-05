using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Controls;

namespace AutoCAD_Version.Validators
{
    /// <summary>
    /// Helper class that is used to validate a TextBox holding a
    /// path to a folder / directory
    /// Also checks that this folder contains at least one DWG file
    /// </summary>
    public class FolderExistsValidator : ValidationRule
    {
        /// <summary>
        /// Called by the TextBoxes validation event
        /// </summary>
        /// <param name="value"></param>
        /// <param name="cultureInfo"></param>
        /// <returns></returns>
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value is string stringValue)
            {
                if (Directory.Exists(stringValue))
                {
                    var theDWGFiles = Directory.GetFiles(stringValue, $"*.{PublicConstants.AUTOCAD_FILE_SUFFIX}");
                    var qtyOfFiles = theDWGFiles.Count();

                    if (qtyOfFiles == 0)
                    {
                        return new ValidationResult(false, $"No {PublicConstants.AUTOCAD_FILE_SUFFIX} files found in this folder");
                    }
                    else
                    {
                        return new ValidationResult(true, null);
                    }
                }
                else
                {
                    return new ValidationResult(false, "Folder does not exist");
                }
            }
            else
                return new ValidationResult(false, "Expecting a string");
        }
    }
}
