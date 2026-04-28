using System;
using System.Windows.Forms;

namespace PublishingHouseApp
{
    // Точка входа приложения
    static class Program
    {
        [STAThread]
        static void Main()
        {
            // Включаем визуальные стили Windows (скруглённые кнопки, темы)
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Запускаем приложение — первой открывается форма авторизации
            Application.Run(new AuthForm());
        }
    }
}
