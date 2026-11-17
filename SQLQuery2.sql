USE LuxuryAuto;
GO

SELECT
    cc.definition AS AllowedValues
FROM sys.check_constraints cc
WHERE cc.name = 'CK_Orders_Status';