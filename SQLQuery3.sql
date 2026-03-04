USE StajSistemiDb;
GO

-- 1. Dan��manlar Tablosu (Senin derste anlatt���n ana rol)
CREATE TABLE Advisors (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(50) NOT NULL,
    Surname NVARCHAR(50) NOT NULL,
    Email NVARCHAR(100)
);

-- 2. Öğrenciler Tablosu (Sistemin Ana Tablosu)
CREATE TABLE Students (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(50) NOT NULL,
    Surname NVARCHAR(50) NOT NULL,
    StudentNo NVARCHAR(20) NOT NULL UNIQUE, -- Okul numarası benzersiz olmalı [cite: 76]
    Email NVARCHAR(100) NOT NULL UNIQUE, -- Boş geçilemez ve benzersiz [cite: 77]
    GPA FLOAT DEFAULT 0.0, -- Stored Procedure'ün çalışması için gerekli
    DepartmentId INT -- Bölüm bağlantısı için
);

-- 3. Adminler Tablosu (Sistemi y�neten teknik ki�i)
CREATE TABLE Admins (
    Id INT PRIMARY KEY IDENTITY(1,1),
    AdminUsername NVARCHAR(50) NOT NULL,
    Email NVARCHAR(100)
);