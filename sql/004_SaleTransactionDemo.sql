/* SSMS demo only: replace the demo IDs with rows in your disposable ApexPostDb. */
USE ApexPostDb;
GO
SET XACT_ABORT ON;
DECLARE @TransactionId varchar(32) = 'DEMO-TX-0001', @BranchCode varchar(12) = 'IND01',
        @CounterId varchar(32) = 'IND01-C1', @ClerkId varchar(32) = 'DEMO-CLERK',
        @Sku varchar(32) = 'APX-001', @Quantity int = 1, @UnitPrice decimal(18,2) = 25.00,
        @TaxRate decimal(6,4) = 0.1800, @LineTotal decimal(18,2);
SET @LineTotal = ROUND(@UnitPrice * @Quantity * (1 + @TaxRate), 2);
BEGIN TRY
    BEGIN TRANSACTION;
    INSERT dbo.CounterTransactions(Id, BranchCode, CounterId, ClerkId, Subtotal, Tax, Total, PaymentMethod, Status, CreatedAt, CompletedAt)
    VALUES (@TransactionId, @BranchCode, @CounterId, @ClerkId, @UnitPrice * @Quantity, @UnitPrice * @Quantity * @TaxRate,
            @LineTotal, 'Cash', 'Paid', SYSUTCDATETIME(), SYSUTCDATETIME());
    UPDATE dbo.BranchStock WITH (UPDLOCK, ROWLOCK)
       SET Quantity = Quantity - @Quantity, UpdatedAt = SYSUTCDATETIME(), ModifiedAtUtc = SYSUTCDATETIME(), UpdatedAt = SYSUTCDATETIME()
     WHERE BranchCode = @BranchCode AND Sku = @Sku AND Quantity >= @Quantity;
    IF @@ROWCOUNT <> 1 THROW 51100, 'Insufficient stock or stock row missing.', 1;
    INSERT dbo.TransactionLines(CounterTransactionId, Id, Kind, Description, Sku, Quantity, UnitPrice, TaxRate, LineTotal)
    VALUES (@TransactionId, 'DEMO-LINE-0001', 'RetailProduct', 'Demo product', @Sku, @Quantity, @UnitPrice, @TaxRate, @LineTotal);
    INSERT dbo.Payments(Id, CounterTransactionId, Method, Amount, PaidAtUtc)
    VALUES ('DEMO-PAYMENT-0001', @TransactionId, 'Cash', @LineTotal, SYSUTCDATETIME());
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO
