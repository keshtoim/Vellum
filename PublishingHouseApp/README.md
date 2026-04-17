# PublishingHouseApp — Шаг 1: Базовый каркас

## Что включено в этот пакет

| Файл | Назначение |
|------|-----------|
| `App.config` | Строка подключения к LocalDB |
| `Program.cs` | Точка входа приложения |
| `AppUsers.cs` | Логины/пароли для трёх ролей |
| `AppColors.cs` | Единая цветовая палитра |
| `DatabaseHelper.cs` | Методы для работы с БД (ExecuteQuery, ExecuteNonQuery, ExecuteScalar) |
| `UIHelper.cs` | Фабрика стандартных UI-элементов |
| `AuthForm.cs` | Форма авторизации |
| `BaseMainForm.cs` | Базовая форма с шапкой и навигацией |
| `AdminForm.cs` | Форма администратора (заглушки разделов) |
| `EditorForm.cs` | Форма редактора (заглушки разделов) |
| `ManagerForm.cs` | Форма менеджера (заглушки разделов) |

---

## Инструкция по настройке проекта в Visual Studio

### 1. Создать проект
- File → New → Project
- Тип: **Windows Forms App (.NET Framework)**
- Имя: `PublishingHouseApp`
- Framework: **.NET Framework 4.7.2**

### 2. Добавить файлы
Скопировать все .cs файлы в папку проекта.
Заменить стандартный `App.config` на предоставленный.
Удалить стандартные `Form1.cs`, `Form1.Designer.cs`.

### 3. Установить NuGet-пакеты
Tools → NuGet Package Manager → Package Manager Console:

```
Install-Package System.Data.SqlClient -Version 4.8.6
Install-Package EPPlus -Version 4.5.3.3
```

### 4. Сборка и запуск
Build → Rebuild Solution → F5

---

## Учётные данные по умолчанию

| Роль | Логин | Пароль |
|------|-------|--------|
| Администратор | `admin` | `admin123` |
| Редактор | `editor` | `editor123` |
| Менеджер | `manager` | `manager123` |

Изменить можно в файле `AppUsers.cs`.

---

## Следующий шаг
Наполнение разделов AdminForm: Авторы, Договоры, Издания и т.д.
