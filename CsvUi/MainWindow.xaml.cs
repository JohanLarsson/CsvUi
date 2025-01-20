namespace CsvUi;

using Microsoft.Win32;
using System.Windows;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        this.InitializeComponent();
    }

    private void OnOpenClick(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Filter = "csv files|*.csv",
        };

        if (dialog.ShowDialog(this) is true)
        {
            this.DataGrid.ItemsSource = new CsvView(Csv.Read(System.IO.File.ReadAllText(dialog.FileName), ','));
        }
    }
}