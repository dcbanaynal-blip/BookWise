MERGE INTO Accounts AS target
USING (VALUES
    ('1000', 'Cash', 'Asset'),
    ('4000', 'Product Revenue', 'Revenue'),
    ('5000', 'Cost of Goods Sold', 'Expense')
) AS source(ExternalAccountNumber, Name, Type)
    ON target.ExternalAccountNumber = source.ExternalAccountNumber
WHEN MATCHED THEN
    UPDATE SET Name = source.Name, Type = source.Type
WHEN NOT MATCHED THEN
    INSERT (ExternalAccountNumber, Name, Type)
    VALUES (source.ExternalAccountNumber, source.Name, source.Type);
