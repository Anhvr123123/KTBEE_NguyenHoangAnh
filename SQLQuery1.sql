USE [LuxuryAuto];
GO

SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    ALTER TABLE dbo.Orders
    DROP CONSTRAINT CK_Orders_Status;

    -- Đổi trạng thái của các đơn cũ.
    UPDATE dbo.Orders
    SET OrderStatus = N'Chờ xác nhận'
    WHERE OrderStatus = N'Chờ thanh toán';

    -- Cho phép các trạng thái đang dùng trong code.
    ALTER TABLE dbo.Orders WITH CHECK
    ADD CONSTRAINT CK_Orders_Status
    CHECK (OrderStatus IN (
        N'Chờ xác nhận',
        N'Đang xử lý',
        N'Đang vận chuyển',
        N'Đã giao',
        N'Đã hủy'
    ));

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    THROW;
END CATCH;
GO