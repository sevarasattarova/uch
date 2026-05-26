using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace uch.Pages
{
    public partial class AppealPage : Page
    {
        public AppealPage()
        {
            InitializeComponent();
            LoadFreezeInfo();
        }

        private void LoadFreezeInfo()
        {
            if (Core.TempFrozenUser != null)
            {
                string reason = Core.TempFrozenUser.FreezeReason ?? "Причина не указана";
                FreezeReasonText.Text = reason;
            }
            else
            {
                FreezeReasonText.Text = "Причина не указана";
            }
        }

        private void SubmitAppeal_Click(object sender, RoutedEventArgs e)
        {
            if (Core.TempFrozenUser == null)
            {
                MessageBox.Show("Ошибка: данные пользователя не найдены.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Проверяем, нет ли уже отправленной заявки
            var existing = Core.Context.FreezeAppeals
                .FirstOrDefault(r => r.UserId == Core.TempFrozenUser.Id &&
                                    r.TargetType == "User" &&
                                    r.Status == "Pending");

            if (existing != null)
            {
                MessageBox.Show("Заявка на разморозку уже отправлена и ожидает рассмотрения.",
                    "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Создаём диалог для ввода причины
            var inputDialog = new ReasonDialog("Укажите причину для разморозки аккаунта:",
                "Уважаемый администратор, прошу разморозить мой аккаунт...");

            if (inputDialog.ShowDialog() == true && !string.IsNullOrWhiteSpace(inputDialog.Answer))
            {
                var appeal = new FreezeAppeals
                {
                    UserId = Core.TempFrozenUser.Id,
                    TargetType = "User",
                    TargetId = Core.TempFrozenUser.Id,
                    Reason = inputDialog.Answer,
                    Status = "Pending",
                    CreatedAt = DateTime.Now
                };

                Core.Context.FreezeAppeals.Add(appeal);
                Core.Context.SaveChanges();

                MessageBox.Show("✅ Заявка на разморозку аккаунта отправлена администратору.\n\nОжидайте решения.",
                    "Заявка отправлена", MessageBoxButton.OK, MessageBoxImage.Information);

                BackToLogin_Click(sender, e);
            }
        }

        private void BackToLogin_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}