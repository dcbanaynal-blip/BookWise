MERGE INTO Accounts AS target
USING (VALUES
    ('1000', '1000', 1, 'Cash', 'Asset', NULL),
    ('4000', '4000', 1, 'Product Revenue', 'Revenue', NULL),
    ('5000', '5000', 1, 'Cost of Goods Sold', 'Expense', NULL)
) AS source(ExternalAccountNumber, SegmentCode, Level, Name, Type, ParentAccountId)
    ON target.ExternalAccountNumber = source.ExternalAccountNumber
WHEN MATCHED THEN
    UPDATE SET Name = source.Name,
               Type = source.Type,
               SegmentCode = source.SegmentCode,
               Level = source.Level,
               ParentAccountId = source.ParentAccountId
WHEN NOT MATCHED THEN
    INSERT (ExternalAccountNumber, SegmentCode, Level, Name, Type, ParentAccountId)
    VALUES (source.ExternalAccountNumber, source.SegmentCode, source.Level, source.Name, source.Type, source.ParentAccountId);
