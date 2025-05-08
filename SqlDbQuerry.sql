CREATE TABLE Projects (
    Id INT PRIMARY KEY,
    Name NVARCHAR(255),  -- You can adjust the length as needed
    SortBy NVARCHAR(255) -- You can adjust the length as needed
);


CREATE TABLE Tasks (
    Id INT PRIMARY KEY,
    ProjectId INT,
    Title NVARCHAR(255),  -- You can adjust the length as needed
    IsCompleted BIT,
    SortOrder INT
);


SELECT COLUMN_NAME, DATA_TYPE 
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Tasks';