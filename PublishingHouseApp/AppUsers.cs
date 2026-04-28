namespace PublishingHouseApp
{
    // Перечисление ролей пользователей системы
    public enum UserRole
    {
        None,       // роль не определена (неверный логин)
        Admin,      // администратор — полный доступ
        Editor,     // редактор — авторы, договоры, издания, этапы, экспертиза
        Manager     // менеджер — просмотр изданий, тиражи, отчёты
    }

    // Хранит учётные данные и выполняет проверку при входе.
    // Данные хранятся в коде приложения (не в БД) — проверка на уровне приложения.
    public static class AppUsers
    {
        // ── Учётные данные ────────────────────────────────────────────────────
        // Для смены паролей — редактируйте константы ниже
        private const string AdminLogin    = "admin";
        private const string AdminPassword = "admin123";

        private const string EditorLogin    = "editor";
        private const string EditorPassword = "editor123";

        private const string ManagerLogin    = "manager";
        private const string ManagerPassword = "manager123";

        // Проверяет логин и пароль, возвращает роль пользователя.
        // Если данные неверны — возвращает UserRole.None
        public static UserRole Authenticate(string login, string password)
        {
            if (login == AdminLogin   && password == AdminPassword)   return UserRole.Admin;
            if (login == EditorLogin  && password == EditorPassword)  return UserRole.Editor;
            if (login == ManagerLogin && password == ManagerPassword) return UserRole.Manager;

            // Ни одна пара не совпала — доступ запрещён
            return UserRole.None;
        }

        // Возвращает отображаемое имя роли на русском языке (для UI)
        public static string GetRoleDisplayName(UserRole role)
        {
            switch (role)
            {
                case UserRole.Admin:   return "Администратор";
                case UserRole.Editor:  return "Редактор";
                case UserRole.Manager: return "Менеджер";
                default:               return "Неизвестно";
            }
        }
    }
}
