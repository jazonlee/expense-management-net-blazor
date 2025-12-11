-- Seed a single test customer (idempotent)
IF NOT EXISTS (SELECT 1 FROM Customers WHERE CustomerNumber = 'CUST-1001')
BEGIN
    INSERT INTO Customers (CustomerId, CustomerNumber, Name, BillingAddressLine1, BillingCity, BillingPostalCode, BillingCountry,
                           ShippingAddressLine1, ShippingCity, ShippingPostalCode, ShippingCountry, CreditLimit, AvailableCredit, CurrentBalance)
    VALUES (NEWID(), 'CUST-1001', 'Test Customer Ltd', '123 Billing St', 'Billingville', '12345', 'USA',
            '456 Shipping Ave', 'Shipton', '67890', 'USA', 50000.00, 40000.00, 10000.00);
END

-- NOTE: Additional invoice, payment, activity, and notification seed data should be added here
-- once specific test scenarios are finalized for dashboard and invoices.
