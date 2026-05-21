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
using uch.Pages;

namespace uch
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
                UpdateSidebarVisibility();
                MainFrame.Navigate(new CatalogPage());
            }

            private void UpdateSidebarVisibility()
            {
                if (Core.IsAdmin)
                    AdminBtn.Visibility = Visibility.Visible;
                if (Core.IsAuthor)
                    AuthorBtn.Visibility = Visibility.Visible;
                if (Core.CurrentUser?.IsFrozen == true)
                    WarningBtn.Visibility = Visibility.Visible;
            }

            private void Catalog_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new CatalogPage());
            private void Lists_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new BookListPage());
            private void Admin_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new AdminPage());
            private void Author_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new AuthorPage());
            private void Warning_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new AppealPage());
            private void Profile_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new ProfilePage());
        }
    }
