using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
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

namespace kangarup.test
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            X509Store store = new X509Store(StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadOnly);
            var x509Certificate2 = store.Certificates.Find(X509FindType.FindBySubjectName, "TestProgramm", false).OfType<X509Certificate2>().First();
            var deployment = new Deployment(new Uri(updateUri.Text));
            deployment.Logger = new DebugLogger();
            deployment.SignaturePrivateCertificate = x509Certificate2;
            var updateInfo = await deployment.CreateUpdateInfoAsync(".", "Initial", Version.Parse("1.0.1.0"));

            var deployed = await deployment.DeployAsync(updateInfo, new NetworkCredential("zok", pwBox.Text));
        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var updater = new Updater(new Uri(updateUri.Text));
            updater.Logger = new DebugLogger();
            var updateInfo = await updater.FetchUpdateInfoAsync();
            if (Version.Parse(updateInfo.Version) > Assembly.GetEntryAssembly().GetName().Version)
            {
                // new update there

                await updater.UpdateNewFilesAsync(updateInfo);

                Debug.WriteLine("yo");
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            Updater.RestartApplication();
        }
    }
}
