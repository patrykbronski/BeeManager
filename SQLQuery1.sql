-- DROP DEFAULT constraint for TypUla (if exists), then make column nullable
DECLARE @c nvarchar(200);

SELECT @c = dc.name
FROM sys.default_constraints dc
JOIN sys.columns col ON col.default_object_id = dc.object_id
JOIN sys.tables t ON t.object_id = col.object_id
WHERE t.name = 'Ule' AND col.name = 'TypUla';

IF @c IS NOT NULL
    EXEC('ALTER TABLE dbo.Ule DROP CONSTRAINT [' + @c + ']');

ALTER TABLE dbo.Ule ALTER COLUMN TypUla int NULL;
