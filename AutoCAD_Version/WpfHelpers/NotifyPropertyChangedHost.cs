using System.Runtime.CompilerServices;
using System.ComponentModel;

namespace AutoCAD_Version.WpfHelpers
{
    public class NotifyPropertyChangedHost : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void SetValue<T>(ref T theDestination, T theValue, [CallerMemberName] string name = "")
        {
            theDestination = theValue;
            NotifyPropertyChanged(name);
        }

        void NotifyPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    }

}
