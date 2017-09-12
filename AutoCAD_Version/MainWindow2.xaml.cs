using System.Windows;

namespace AutoCAD_Version
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow()
        {

            InitializeComponent();

        }

        private void CommandBinding_Close(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            this.Close();
        }

        private void CommandBinding_Help(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            (new AboutBox()).ShowDialog();
        }
    }

}
