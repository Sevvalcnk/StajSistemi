USE StajSistemiDb;
GO

-- 1. STORED PROCEDURE (Plandaki maddeyi karþýlar)
CREATE PROCEDURE sp_BasariliOgrencileriGetir
    @MinGPA FLOAT 
AS
BEGIN
    SELECT Name, Surname, StudentNo, GPA 
    FROM Students 
    WHERE GPA >= @MinGPA
    ORDER BY GPA DESC;
END;
GO

-- 2. TRIGGER (Plandaki maddeyi ve IP log sistemini destekler)
CREATE TRIGGER Trg_YeniOgrenciKayitLog
ON Students
AFTER INSERT
AS
BEGIN
    INSERT INTO LoginLogs (UserEmail, IPAddress, LoginTime)
    SELECT Email, 'Sistem Kaydý', GETDATE() 
    FROM inserted;
END;
GO