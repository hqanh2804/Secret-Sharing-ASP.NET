CREATE DATABASE SecretSharing;
GO
USE SecretSharing;
GO

--Delete from Users;
--DBCC checkident('Users', reseed, 0);
--Delete from ManageFiles;

CREATE TABLE Users
(
	ID INT IDENTITY(1,1) PRIMARY KEY,
	Username NVARCHAR(30) NOT NULL,
	Password NVARCHAR(20) NOT NULL,
	Email NVARCHAR(100) NOT NULL,
);

--Drop table ManageFiles
CREATE TABLE ManageFiles
(
	ID INT NOT NULL,
	Filename NVARCHAR(200) NOT NULL,
	Url NVARCHAR(MAX) NOT NULL,
	CreatedDate datetime NOT NULL,
);

select * from Users;
select * from ManageFiles;