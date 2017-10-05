using System.Runtime.CompilerServices;
using System.ComponentModel;
using System.Diagnostics.Contracts;

namespace AutoCAD_Version.WpfHelpers
{
    /// <summary>
    /// Helper class for class that implement INotifyPropertyChanged 
    /// It add a procedure for triggering the PropertyChanged Event
    /// and adds a method for updating a property set
    /// </summary>
    public class NotifyPropertyChangedHost : INotifyPropertyChanged
    {
        /// <summary>
        /// Dervied PropertyChanged event
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Method for updating a value and triggering a PropertyChanged event in one call
        /// </summary>
        /// <typeparam name="T">Type of the property to be updated</typeparam>
        /// <param name="theDestination">the property to be updated</param>
        /// <param name="theValue">the new value of the property</param>
        /// <param name="name">the name of the property</param>
        public void SetValue<T>(ref T theDestination, T theValue, [CallerMemberName] string name = "")
        {
            Contract.Requires(!string.IsNullOrEmpty(name), "Parameter name in PropertyChanged.SetValue is null or empty");

            theDestination = theValue;
            NotifyPropertyChanged(name);
        }

        /// <summary>
        /// Checks for subscribers to PropertyChanged and then triggers the event
        /// </summary>
        /// <param name="name"></param>
        void NotifyPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    }

}
