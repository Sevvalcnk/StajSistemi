USE StajSistemiDb;
GO

-- 1. Bölümler Tablosu
CREATE TABLE Departments (
    Id INT PRIMARY KEY IDENTITY(1,1),
    DepartmentName NVARCHAR(100) NOT NULL
);

-- 2. Staj Ýlanlarý Tablosu
CREATE TABLE InternshipAds (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Title NVARCHAR(200) NOT NULL,
    Description NVARCHAR(MAX),
    City NVARCHAR(50),
    Quota INT -- Kontenjan [cite: 6]
);

-- 3. Baþvurular Tablosu (Öðrenci ve Ýlaný birbirine baðlar) [cite: 25]
CREATE TABLE Applications (
    Id INT PRIMARY KEY IDENTITY(1,1),
    StudentId INT NOT NULL,
    AdId INT NOT NULL,
    ApplicationDate DATETIME DEFAULT GETDATE(),
    Status NVARCHAR(50) DEFAULT 'Beklemede', -- Onay süreci için [cite: 5]
    FOREIGN KEY (StudentId) REFERENCES Students(Id),
    FOREIGN KEY (AdId) REFERENCES InternshipAds(Id)
);

-- 4. Giriþ ve Güvenlik (IP Log) Tablosu [cite: 28]
CREATE TABLE LoginLogs (
    Id INT PRIMARY KEY IDENTITY(1,1),
    UserEmail NVARCHAR(100),
    IPAddress NVARCHAR(50), -- IP kayýt sistemi için [cite: 17]
    LoginTime DATETIME DEFAULT GETDATE()
);

-- 5. Mevcut Students Tablosuna Bölüm Baðlantýsý Ekleme
ALTER TABLE Students ADD DepartmentId INT;
ALTER TABLE Students ADD FOREIGN KEY (DepartmentId) REFERENCES Departments(Id);