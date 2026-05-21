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
    /// Логика взаимодействия для BookListPage.xaml
    /// </summary>
    public partial class BookListPage : Page
    {
        public BookListPage()
        {
          
                InitializeComponent();
                LoadAllLists();
            }

            private void LoadAllLists()
            {
                // Проверка авторизации
                if (Core.CurrentUser == null)
                {
                    MessageBox.Show("Авторизуйтесь для просмотра списков книг");
                    return;
                }

                LoadList(ReadingList, "Reading");
                LoadList(ReadList, "Read");
                LoadList(PlannedList, "Planned");
                LoadList(DroppedList, "Dropped");
            }

            private void LoadList(ListBox targetList, string listType)
            {
                try
                {
                    // Получаем записи из UserBookLists
                    var userBookLists = Core.Context.UserBookLists
                        .Where(ub => ub.UserId == Core.CurrentUser.Id && ub.ListType == listType)
                        .ToList();

                    // Создаём список книг
                    var books = new List<Books>();
                    foreach (var ub in userBookLists)
                    {
                        if (ub.Books != null)
                        {
                            books.Add(ub.Books);
                        }
                    }

                    targetList.ItemsSource = books;
                    targetList.Tag = listType;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка загрузки списка: {ex.Message}");
                    targetList.ItemsSource = new List<Books>();
                }
            }

            private void MoveBook_Click(object sender, SelectionChangedEventArgs e)
            {
                var listBox = sender as ListBox;
                if (listBox == null || listBox.SelectedItem == null) return;

                var book = listBox.SelectedItem as Books;
                if (book == null) return;

                var currentListType = listBox.Tag?.ToString() ?? "";

                var dialog = new MoveBookDialog(book.Id, currentListType);
                if (dialog.ShowDialog() == true)
                {
                    LoadAllLists(); // обновляем все списки
                }
            }
        }
    }
