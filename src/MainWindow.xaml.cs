using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;

namespace Squirt
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow Instance { get; private set; }

        public MainWindow()
        {
            InitializeComponent();
            Instance = this;
            LoadLastSetting();
        }

        private void LoadLastSetting()
        {
            Registry registry = new Registry();
            this.selectedUpdateDirectory.Text = registry.FolderValue;
            this.selectedSource.Text = registry.DataSourceValue;
        }

        private void selectDirectory_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog browseDialog = new System.Windows.Forms.FolderBrowserDialog();
            var selection = browseDialog.ShowDialog();
            if (selection == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(browseDialog.SelectedPath))
            {
                this.selectedUpdateDirectory.Text = browseDialog.SelectedPath;
            }
        }

        private void selectFile_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog browseDialog = new System.Windows.Forms.OpenFileDialog();
            browseDialog.Filter = "MSSQL-Dateien (*.sdf)|*.sdf";
            browseDialog.CheckFileExists = true;
            browseDialog.Multiselect = false;
            var selection = browseDialog.ShowDialog();
            if (selection == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(browseDialog.FileName))
            {
                this.selectedSource.Text = string.Format("Data Source={0};Password=xxxx;Persist Security Info=True", browseDialog.FileName);
            }
        }

        private void startUpdate_Click(object sender, RoutedEventArgs e)
        {
            SaveLastSettings();
            if (string.IsNullOrWhiteSpace(this.selectedUpdateDirectory.Text))
            {
                MessageBox.Show("Bitte geben Sie ein Verzeichnis an");
                return;
            }

            if (string.IsNullOrWhiteSpace(this.selectedSource.Text))
            {
                MessageBox.Show("Bitte geben Sie eine Datenbankverbindung an");
                return;
            }

            var updateVersions = FileEngine.ReadSqlVersions(this.selectedUpdateDirectory.Text);
            DBEngine dbengine = new DBEngine(this.selectedSource.Text);
            foreach (var versionInformation in updateVersions)
            {
                if (!dbengine.ExistsVersion(versionInformation))
                {
                    dbengine.ExecuteSqlQuery(versionInformation.Query, 
                                             versionInformation.Version, 
                                             versionInformation.Subversion);
                    dbengine.InsertVersion(versionInformation);
                }
            }

            AddProtocolEntry("Fertig");
        }

        public void AddProtocolEntry(string entry)
        {
            StringBuilder text = new StringBuilder(this.protocol.Text);
            text.AppendLine(entry);
            this.protocol.Text = text.ToString();
        }

        private void SaveLastSettings()
        {
            Registry registry = new Registry();
            registry.DataSourceValue = this.selectedSource.Text;
            registry.FolderValue = this.selectedUpdateDirectory.Text;
        }
    }
}
