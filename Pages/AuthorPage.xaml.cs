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

namespace uch.Pages
{
    /// <summary>
    /// Логика взаимодействия для AuthorPage.xaml
    /// </summary>
    public partial class AuthorPage : Page
    
        {
            public AuthorPage()
            {
                InitializeComponent();
                LoadMyBooks();
            }

            private void LoadMyBooks()
            {
                var books = Core.Context.Books
                    .Where(b => b.AuthorId == Core.CurrentUser.Id)
                    .ToList();
                MyBooksList.ItemsSource = books;
            }

            private void AddBook_Click(object sender, RoutedEventArgs e)
            {
                NavigationService.Navigate(new AddEditBookPage(null));
            }

            private void EditBook_Click(object sender, RoutedEventArgs e)
            {
                int bookId = (int)((Button)sender).Tag;
                var book = Core.Context.Books.Find(bookId);
                if (book != null)
                    NavigationService.Navigate(new AddEditBookPage(book));
            }

            private void AppealFrozenBook_Click(object sender, RoutedEventArgs e)
            {
                var frozenBooks = Core.Context.Books
                    .Where(b => b.AuthorId == Core.CurrentUser.Id && b.IsFrozen)
                    .ToList();
                if (!frozenBooks.Any())
                {
                    MessageBox.Show("У вас нет замороженных книг.");
                    return;
                }
                var book = frozenBooks.First();

                var appeal = new FreezeAppeals
                {
                    UserId = Core.CurrentUser.Id,
                    TargetType = "Book",
                    TargetId = book.Id,
                    Reason = "Хочу оспорить заморозку",
                    Status = "Pending",
                    CreatedAt = DateTime.Now
                };
                Core.Context.FreezeAppeals.Add(appeal);
                Core.Context.SaveChanges();
                MessageBox.Show("Заявка на разморозку книги отправлена.");
            }
        }
    }