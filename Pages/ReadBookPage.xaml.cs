using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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

namespace uch.Pages
{
    /// <summary>
    /// Логика взаимодействия для ReadBookPage.xaml
    /// </summary>
    public partial class ReadBookPage : Page
    {
      
            public ReadBookPage(int bookId)
            {
                InitializeComponent();
                var book = Core.Context.Books.Find(bookId);

                // Проверка: если книга заморожена и пользователь не админ
                if (book != null && book.IsFrozen && !Core.IsAdmin)
                {
                    MessageBox.Show("Эта книга заморожена и недоступна для чтения.",
                        "Доступ запрещён", MessageBoxButton.OK, MessageBoxImage.Warning);
                    NavigationService.GoBack();
                    return;
                }

                DataContext = book;
            }
        }
    }