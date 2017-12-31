using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
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


namespace Windows10ClearIconCache
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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Title = Assembly.GetEntryAssembly().GetName().Name;
            if (IsAdministrator())
            {
                this.Title += " ";
                this.Title += Properties.Resources.Administrator;
                btnRunAsAdministrator.Visibility = Visibility.Hidden;
            }
        }

        bool MyDeleteFile2(string fullpath)
        {
            try
            {
                File.Delete(fullpath);
            }
            catch (Exception)
            { }

            return !File.Exists(fullpath);
        }
        bool MyDeleteFile1(string fullpath)
        {
            if (MyDeleteFile2(fullpath))
                return true;
            foreach (Process pro in Process.GetProcessesByName("explorer"))
            {
                try
                {
                    pro.Kill();
                    if (MyDeleteFile2(fullpath))
                        return true;
                }
                catch
                { }
            }
            return MyDeleteFile2(fullpath);
        }
        void MyDeleteFile(string fullpath)
        {
            if (!File.Exists(fullpath))
                return;
            bool ok = MyDeleteFile1(fullpath);
            Log(string.Format("Delete {0}: {1}",
                ok ? "OK" : "NG", fullpath));

        }
        void Log(string text)
        {
            txtLog.AppendText(text);
            txtLog.AppendText(Environment.NewLine);
        }
        void DeleteCache(DirectoryInfo di, string pattern)
        {
            FileInfo[] cacheFiles = di.GetFiles(pattern, SearchOption.TopDirectoryOnly);
            foreach (FileInfo fi in cacheFiles)
            {
                MyDeleteFile(fi.FullName);
            }
        }
        public static bool IsAdministrator()
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
        private void btnClearIconCache_Click(object sender, RoutedEventArgs e)
        {
            txtLog.Clear();

            string pathLocal = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            if (chkIconCacheDB.IsChecked ?? false)
            {
                string cacheDb = System.IO.Path.Combine(pathLocal, "IconCache.db");
                MyDeleteFile(cacheDb);
            }

            string cacheDir = System.IO.Path.Combine(pathLocal, @"Microsoft\Windows\Explorer");
            DirectoryInfo di = new DirectoryInfo(cacheDir);

            if (chkIconcacheALLDB.IsChecked ?? false)
                DeleteCache(di, "iconcache_*.db");
            if (chkThumbcacheALLDB.IsChecked ?? false)
                DeleteCache(di, "thumbcache_*.db");
        }

        public static string ProductName
        {
            get
            {
                AssemblyProductAttribute myProduct =
                    (AssemblyProductAttribute)AssemblyProductAttribute.GetCustomAttribute(Assembly.GetExecutingAssembly(),
                    typeof(AssemblyProductAttribute));
                return myProduct.Product;
            }
        }
        public static string ProductVersion
        {
            get
            {
                return Assembly.GetEntryAssembly().GetName().Version.ToString();
            }
        }
        private void btnRunAsAdministrator_Click(object sender, RoutedEventArgs e)
        {
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = Assembly.GetExecutingAssembly().Location;
            psi.Verb = "runas";
            try
            {
                Process.Start(psi);
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message,
                    ProductName,
                    MessageBoxButton.OK,
                    MessageBoxImage.Exclamation);
            }
        }

        private void btnAbout_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder message = new StringBuilder();
            message.Append(ProductName);
            message.Append(" version");
            message.Append(ProductVersion);
            message.AppendLine();
            message.AppendLine();
            message.Append("copyright 2018 Ambiesoft");
            message.AppendLine();
            message.Append("http://ambiesoft.fam.cx/");
            MessageBox.Show(
                message.ToString(),
                ProductName,
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
    }
}
