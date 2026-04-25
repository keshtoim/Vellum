-- СОЗДАНИЕ БАЗЫ ДАННЫХ
CREATE DATABASE PublishingHouse; -- Создание базы данных
GO

USE PublishingHouse; -- Выбор базы данных для работы
GO

-- СОЗДАНИЕ ТАБЛИЦ
-- Таблица "Класс" 
CREATE TABLE Class (
    class_id INT PRIMARY KEY NOT NULL, -- Создание поля с ограничениями: первичный ключ, не может быть нулевого значения
    class_level NVARCHAR(20) NOT NULL UNIQUE -- Создание поля с ограничениями: не может быть нулевого значения, уникальный
);
GO

-- Таблица "Предмет" 
CREATE TABLE Subject (
    subject_id INT PRIMARY KEY NOT NULL, -- Создание поля с ограничениями: первичный ключ, не может быть нулевого значения
    subject_name NVARCHAR(50) NOT NULL UNIQUE -- Создание поля с ограничениями: не может быть нулевого значения, уникальный
);
GO

-- Таблица "Тип" 
CREATE TABLE Type (
    type_id INT PRIMARY KEY NOT NULL, -- Создание поля с ограничениями: первичный ключ, не может быть нулевого значения
    type_name NVARCHAR(25) NOT NULL UNIQUE -- Создание поля с ограничениями: не может быть нулевого значения, уникальный
);
GO

-- Таблица "Формат" 
CREATE TABLE Format (
    format_id INT PRIMARY KEY IDENTITY(1,1) NOT NULL, -- Создание поля с ограничениями: первичный ключ, не может быть нулевого значения, автозаполнение
    format_name NVARCHAR(30) NOT NULL UNIQUE -- Создание поля с ограничениями: не может быть нулевого значения, уникальный
);
GO

-- Таблица "Автор"
CREATE TABLE Author (
    author_id INT PRIMARY KEY IDENTITY(1,1) NOT NULL, -- Создание поля с ограничениями: первичный ключ, не может быть нулевого значения, автозаполнение
    surname NVARCHAR(50) NOT NULL, -- Создание поля фамилии автора с ограничением: не может быть нулевого значения
    name NVARCHAR(50) NOT NULL, -- Создание поля имени автора с ограничением: не может быть нулевого значения
    patronymic NVARCHAR(50), -- Создание поля отчества автора
    email NVARCHAR(100) UNIQUE, -- Создание поля электронной почты с ограничением: уникальный
    phone NVARCHAR(20), -- Создание поля телефона автора
    tax_id NVARCHAR(12) UNIQUE -- Создание поля ИНН с ограничением: уникальный
);
GO

-- Таблица "Договор"
CREATE TABLE Contract (
    contract_id INT PRIMARY KEY IDENTITY(1,1) NOT NULL, -- Создание поля с ограничениями: первичный ключ, не может быть нулевого значения, автозаполнение
    signing_date DATE NOT NULL, -- Создание поля даты подписания с ограничением: не может быть нулевого значения
    valid_until DATE, -- Создание поля срока действия договора
    amount DECIMAL(10,2) CHECK (amount >= 0), -- Создание поля суммы договора с ограничением: проверка на неотрицательность
    author_id INT NOT NULL, -- Создание поля идентификатора автора с ограничением: не может быть нулевого значения
    CONSTRAINT FK_Contract_Author FOREIGN KEY (author_id) -- Создание внешнего ключа для связи с таблицей Author
        REFERENCES Author(author_id)
);
GO

-- Таблица "Издание"
CREATE TABLE Publication (
    publication_id INT PRIMARY KEY IDENTITY(1,1) NOT NULL, -- Создание поля с ограничениями: первичный ключ, не может быть нулевого значения, автозаполнение
    title NVARCHAR(200) NOT NULL, -- Создание поля названия издания с ограничением: не может быть нулевого значения
    isbn NVARCHAR(13) UNIQUE, -- Создание поля ISBN с ограничением: уникальный
    contract_id INT NOT NULL, -- Создание поля идентификатора договора с ограничением: не может быть нулевого значения
    type_id INT NOT NULL, -- Создание поля идентификатора типа с ограничением: не может быть нулевого значения
    subject_id INT NOT NULL, -- Создание поля идентификатора предмета с ограничением: не может быть нулевого значения
    class_id INT NOT NULL, -- Создание поля идентификатора класса с ограничением: не может быть нулевого значения
    CONSTRAINT FK_Publication_Contract FOREIGN KEY (contract_id) -- Создание внешнего ключа для связи с таблицей Contract
        REFERENCES Contract(contract_id),
    CONSTRAINT FK_Publication_Type FOREIGN KEY (type_id) -- Создание внешнего ключа для связи с таблицей Type
        REFERENCES Type(type_id),
    CONSTRAINT FK_Publication_Subject FOREIGN KEY (subject_id) -- Создание внешнего ключа для связи с таблицей Subject
        REFERENCES Subject(subject_id),
    CONSTRAINT FK_Publication_Class FOREIGN KEY (class_id) -- Создание внешнего ключа для связи с таблицей Class
        REFERENCES Class(class_id)
);
GO

-- Таблица "Тираж"
CREATE TABLE PrintRun (
    print_run_id INT PRIMARY KEY IDENTITY(1,1) NOT NULL, -- Создание поля с ограничениями: первичный ключ, не может быть нулевого значения, автозаполнение
    year INT NOT NULL, -- Создание поля года выпуска с ограничением: не может быть нулевого значения
    quantity INT NOT NULL CHECK (quantity > 0), -- Создание поля количества с ограничениями: не может быть нулевого значения, проверка на положительность
    format_id INT NOT NULL, -- Создание поля идентификатора формата с ограничением: не может быть нулевого значения
    publication_id INT NOT NULL, -- Создание поля идентификатора издания с ограничением: не может быть нулевого значения
    CONSTRAINT FK_PrintRun_Format FOREIGN KEY (format_id) -- Создание внешнего ключа для связи с таблицей Format
        REFERENCES Format(format_id),
    CONSTRAINT FK_PrintRun_Publication FOREIGN KEY (publication_id) -- Создание внешнего ключа для связи с таблицей Publication
        REFERENCES Publication(publication_id)
);
GO

-- Таблица "Этап подготовки"
CREATE TABLE PreparationStage (
    stage_id INT PRIMARY KEY IDENTITY(1,1) NOT NULL, -- Создание поля с ограничениями: первичный ключ, не может быть нулевого значения, автозаполнение
    stage_name NVARCHAR(100) NOT NULL, -- Создание поля названия этапа с ограничением: не может быть нулевого значения
    start_date DATE NOT NULL, -- Создание поля даты начала с ограничением: не может быть нулевого значения
    status NVARCHAR(30) NOT NULL, -- Создание поля статуса с ограничением: не может быть нулевого значения
    publication_id INT NOT NULL, -- Создание поля идентификатора издания с ограничением: не может быть нулевого значения
    CONSTRAINT FK_PreparationStage_Publication FOREIGN KEY (publication_id) -- Создание внешнего ключа для связи с таблицей Publication
        REFERENCES Publication(publication_id)
);
GO

-- Таблица "Экспертиза"
CREATE TABLE Expertise (
    expertise_id INT PRIMARY KEY IDENTITY(1,1) NOT NULL, -- Создание поля с ограничениями: первичный ключ, не может быть нулевого значения, автозаполнение
    date DATE NOT NULL, -- Создание поля даты с ограничением: не может быть нулевого значения
    result TEXT, -- Создание поля результата экспертизы
    valid_until DATE, -- Создание поля срока действия экспертизы
    stage_id INT NOT NULL, -- Создание поля идентификатора этапа с ограничением: не может быть нулевого значения
    CONSTRAINT FK_Expertise_PreparationStage FOREIGN KEY (stage_id) -- Создание внешнего ключа для связи с таблицей PreparationStage
        REFERENCES PreparationStage(stage_id)
);
GO
