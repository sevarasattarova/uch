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
    /// Логика взаимодействия для CatalogPage.xaml
    /// </summary>
    public partial class CatalogPage : Page
    {
        public CatalogPage()
        {
            InitializeComponent();
                LoadGenres();
                LoadBooks();
            }

            private void LoadGenres()
            {
                GenreFilter.ItemsSource = Core.Context.Genres.ToList();
            }

        private void LoadBooks()
        {
            var query = Core.Context.Books.AsQueryable();

            if (!string.IsNullOrWhiteSpace(SearchBox.Text))
                query = query.Where(b => b.Title.Contains(SearchBox.Text) || b.Users.Username.Contains(SearchBox.Text));

            if (GenreFilter.SelectedItem != null)
            {
                var genre = (Genres)GenreFilter.SelectedItem;
                query = query.Where(b => b.Genres.Any(bg => bg.Id == genre.Id));
            }

            // 4. Сортировка
            switch (SortCombo.SelectedIndex)
            {
                case 0: query = query.OrderBy(b => b.Title); break;
                case 1: query = query.OrderByDescending(b => b.Rating); break;
                default: query = query.OrderBy(b => b.Title); break;
            }

            // 5. Проекция в анонимный объект (как во втором примере)
            var books = query.Select(b => new
            {
                b.Id,
                b.Title,
                b.Rating,
                AuthorName = b.Users.Username,
                CoverImageUrl = b.CoverImageUrl,
                // При необходимости можно добавить CoverImageUrl, количество отзывов и т.д.
            }).ToList();

            // 6. Присвоение ItemsSource
            BooksGrid.ItemsSource = books;
            if (!Core.IsAdmin)
            {
                query = query.Where(b => b.IsFrozen == false);
            }

        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e) => LoadBooks();
            private void SortCombo_SelectionChanged(object sender, SelectionChangedEventArgs e) => LoadBooks();
            private void GenreFilter_SelectionChanged(object sender, SelectionChangedEventArgs e) => LoadBooks();
            private void ResetFilter_Click(object sender, RoutedEventArgs e)
            {
                SearchBox.Text = "";
                GenreFilter.SelectedItem = null;
                SortCombo.SelectedIndex = -1;
                LoadBooks();
            }

            private void ReadBook_Click(object sender, RoutedEventArgs e)
            {
                int bookId = (int)((Button)sender).Tag;
                NavigationService.Navigate(new BookPage(bookId));
            }

        private void AddToList_Click(object sender, RoutedEventArgs e)
        {
            if (!Core.IsAuthenticated)
            {
                MessageBox.Show("Войдите в аккаунт");
                return;
            }

            int bookId = (int)((Button)sender).Tag;

            // Прямое добавление в список "Читаю"
            var existing = Core.Context.UserBookLists
                .FirstOrDefault(ub => ub.UserId == Core.CurrentUser.Id && ub.BookId == bookId);

            if (existing != null)
            {
                MessageBox.Show("Книга уже есть в ваших списках");
                return;
            }

            Core.Context.UserBookLists.Add(new UserBookLists
            {
                UserId = Core.CurrentUser.Id,
                BookId = bookId,
                ListType = "Reading",
                AddedAt = DateTime.Now
            });

            Core.Context.SaveChanges();
            MessageBox.Show("Книга добавлена в список 'Читаю'");
        }
    }
    }