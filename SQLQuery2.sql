-- USUWAMY stare kolumny stringowe
ALTER TABLE dbo.Ule DROP COLUMN TypUla;
ALTER TABLE dbo.Ule DROP COLUMN Status;

-- DODAJEMY je od nowa jako int (enum)
ALTER TABLE dbo.Ule ADD TypUla int NULL;
ALTER TABLE dbo.Ule ADD Status int NULL;
