# ИС «Учёт работы издательства Просвещение»

Информационная система для автоматизации процессов управления деятельностью образовательного издательства. Разработана в рамках учебной практики.

---

## Возможности

- **Управление авторами** — добавление, редактирование, удаление
- **Управление договорами** — учёт дат, сумм, привязка к авторам
- **Управление изданиями** — тип, тематика, класс, ISBN, связь с договором
- **Этапы подготовки** — контроль последовательности статусов (Запланирован → В работе → Завершён)
- **Экспертиза** — результаты и сроки действия по этапам
- **Учёт тиражей** — год, количество, формат издания
- **Отчёты Excel** — 8 видов отчётов через EPPlus (сводный, за период, по каждой таблице)
- **Dashboard** — главная страница со статистикой и предупреждениями об истекающих договорах
- **Поиск с подсветкой** — встроен в каждый раздел, совпадения подсвечиваются жёлтым

---

## Роли пользователей

| Роль | Доступ |
|------|--------|
| Администратор | Все разделы, управление пользователями, настройки |
| Редактор | Авторы, договоры, издания, этапы, экспертиза |
| Менеджер | Просмотр изданий, тиражи, отчёты |

Учётные данные по умолчанию задаются в `AppUsers.cs`.

---

## Технологии

| Компонент | Версия |
|-----------|--------|
| Язык | C# |
| Платформа | .NET Framework 4.7.2 |
| UI | Windows Forms |
| СУБД | Microsoft SQL Server LocalDB |
| Excel-отчёты | EPPlus (EPPlusFree) 4.5.3 |
| Подключение к БД | System.Data.SqlClient 4.9.1 |

---

## Требования

- Windows 10 / Windows 11
- [.NET Framework 4.7.2](https://dotnet.microsoft.com/en-us/download/dotnet-framework/net472)
- [SQL Server LocalDB](https://learn.microsoft.com/ru-ru/sql/database-engine/configure-windows/sql-server-express-localdb) (входит в состав Visual Studio)
- Visual Studio 2019 / 2022

---

## Установка и запуск

### 1. Клонировать репозиторий

```bash
git clone https://github.com/<ваш-логин>/PublishingHouseApp.git
cd PublishingHouseApp
```

### 2. Создать базу данных

Откройте SQL Server Management Studio (SSMS) или выполните скрипты через sqlcmd:

```bash
# Создание структуры БД
sqlcmd -S "(localdb)\MSSQLLocalDB" -i "sql-файлы\создание базы данных и таблиц.sql"

# Заполнение тестовыми данными
sqlcmd -S "(localdb)\MSSQLLocalDB" -i "sql-файлы\заполнение бд.sql"
```

### 3. Настроить строку подключения

```bash
# Скопируйте шаблон
copy App.config.example App.config
```

Откройте `App.config` и убедитесь, что строка подключения верна:

```xml
<add name="PublishingDB"
     connectionString="Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=PublishingHouse01;Integrated Security=True;"
     providerName="System.Data.SqlClient" />
```

### 4. Установить NuGet-пакеты и собрать

Откройте решение в Visual Studio, затем:

```
Tools → NuGet Package Manager → Package Manager Console
```

```powershell
Install-Package EPPlusFree -Version 4.5.3.8
Install-Package System.Data.SqlClient -Version 4.9.1
```

Затем: **Build → Rebuild Solution** (Ctrl+Shift+B)

### 5. Запустить

Нажмите **F5** или запустите `bin\Debug\PublishingHouseApp.exe`.

---

## Структура проекта

```
PublishingHouseApp/
├── sql-файлы/
│   ├── создание базы данных и таблиц.sql   # DDL: создание таблиц и связей
│   └── заполнение бд.sql                   # DML: тестовые данные
│
├── Program.cs               # Точка входа
├── App.config.example       # Шаблон строки подключения (скопируй в App.config)
│
│   -- Авторизация и базовые классы --
├── AuthForm.cs              # Форма входа
├── AppUsers.cs              # Роли и учётные данные
├── BaseMainForm.cs          # Базовая форма с навигацией
├── SharedSections.cs        # RoleFormBase: хелперы, split-layout, мини-формы
│
│   -- Ролевые формы --
├── AdminForm.cs             # Форма администратора (все разделы)
├── EditorManagerForms.cs    # Формы редактора и менеджера
│
│   -- UI-компоненты --
├── DashboardPanel.cs        # Главная страница: статистика и договоры
├── ReportsPanel.cs          # Панель формирования отчётов
├── SectionSearchBar.cs      # Встроенная строка поиска с подсветкой
│
│   -- Вспомогательные классы --
├── AppColors.cs             # Цветовая палитра
├── UIHelper.cs              # Фабрика UI-элементов
├── ComboItem.cs             # Элемент ComboBox (ID + текст)
│
│   -- Данные и отчёты --
├── DatabaseHelper.cs        # SQL-запросы, SmartInsert, обработка ошибок
└── ReportHelper.cs          # Генерация Excel-отчётов (EPPlus)
```

---

## База данных

| Таблица | Описание |
|---------|----------|
| Author | Авторы (ФИО, email, телефон, ИНН) |
| Contract | Договоры (даты, сумма, автор) |
| Publication | Издания (название, ISBN, тип, тематика, класс) |
| PreparationStage | Этапы подготовки изданий |
| Expertise | Экспертизы по этапам |
| PrintRun | Тиражи изданий |
| Type | Справочник типов изданий |
| Subject | Справочник тематик |
| Class | Справочник классов |
| Format | Справочник форматов печати |

---

## Учётные данные по умолчанию

| Роль | Логин | Пароль |
|------|-------|--------|
| Администратор | `admin` | `admin123` |
| Редактор | `editor` | `editor123` |
| Менеджер | `manager` | `manager123` |

> Изменить можно в файле `AppUsers.cs` (константы `AdminLogin`, `AdminPassword` и т.д.)

---

## Лицензия

Проект создан в учебных целях.
