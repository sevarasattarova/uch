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
    /// Логика взаимодействия для AddEditBookPage.xaml
    /// </summary>
    public partial class AddEditBookPage : Page
    {
        private Books _editingBook;

        public AddEditBookPage(Books book)
        {
            InitializeComponent();
            LoadGenres();
            if (book != null)
            {
                _editingBook = book;
                TitleBox.Text = book.Title;
                DescriptionBox.Text = book.Description;
                TextContentBox.Text = book.TextContent;
                CoverUrlBox.Text = book.CoverImageUrl;

                // Отмечаем выбранные жанры (через навигационное свойство)
                if (book.Genres != null)
                {
                    var bookGenreIds = book.Genres.Select(g => g.Id).ToList();
                    foreach (var item in GenresList.Items)
                    {
                        var genre = item as Genres;
                        if (genre != null && bookGenreIds.Contains(genre.Id))
                        {
                            GenresList.SelectedItems.Add(genre);
                        }
                    }
                }
            }
        }

        private void LoadGenres()
        {
            GenresList.ItemsSource = Core.Context.Genres.ToList();
        }

        private void SaveBook_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TitleBox.Text))
            {
                MessageBox.Show("Введите название книги");
                return;
            }

            try
            {
                var book = _editingBook ?? new Books();
                book.Title = TitleBox.Text;
                book.Description = DescriptionBox.Text;
                book.TextContent = TextContentBox.Text;
                book.CoverImageUrl = CoverUrlBox.Text;
                book.AuthorId = Core.CurrentUser.Id;
                book.CreatedAt = DateTime.Now;

                if (_editingBook == null)
                    Core.Context.Books.Add(book);

                Core.Context.SaveChanges();

                // Обновляем жанры через прямой SQL или временно отключаем
                // TODO: добавить сохранение жанров позже

                MessageBox.Show("Книга сохранена.");
                NavigationService.GoBack();
            }
            catch (Exception ex)
            {
                string error = ex.Message;
                if (ex.InnerException != null)
                    error += "\n" + ex.InnerException.Message;
                MessageBox.Show($"Ошибка: {error}");
            }
        }
    }
}