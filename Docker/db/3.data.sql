INSERT INTO categories (Name, ImageUrl) VALUES ('Cà phê', 'coffee.jpg');
INSERT INTO categories (Name, ImageUrl) VALUES ('Trà sữa', 'milk_tea.jpg');
INSERT INTO categories (Name, ImageUrl) VALUES ('Trà', 'tea.jpg');
INSERT INTO categories (Name, ImageUrl) VALUES ('Đá xay', 'freeze.jpg');
INSERT INTO products (Name, IsTopping, CategoryId, ImageUrl) VALUES ('Cà phê đen', 'f', 1, 'coffee.jpg');
INSERT INTO products (Name, IsTopping, CategoryId, ImageUrl) VALUES ('Trà sữa', 'f', 2, 'milk_tea.jpg');
INSERT INTO products (Name, IsTopping, CategoryId, ImageUrl) VALUES ('Trà chanh', 'f', 3, 'tea.jpg');
INSERT INTO products (Name, IsTopping, CategoryId, ImageUrl) VALUES ('Đá xay oreo', 'f', 4, 'freeze.jpg');
INSERT INTO products (Name, IsTopping, CategoryId, ImageUrl) VALUES ('Cà phê espresso', 'f', 1, 'kohi_logo.png');
INSERT INTO products (Name, IsTopping, CategoryId, ImageUrl) VALUES ('Thạch trân châu', 't', 1, 'kohi_logo.png');
INSERT INTO products (Name, IsTopping, CategoryId, ImageUrl) VALUES ('Thạch củ năng', 't', 1, 'kohi_logo.png');
INSERT INTO productvariants (ProductId, Size, Price, Cost) VALUES (1, 'Size S', 30000, 18000);
INSERT INTO productvariants (ProductId, Size, Price, Cost) VALUES (1, 'Size L', 40000, 24000);
INSERT INTO productvariants (ProductId, Size, Price, Cost) VALUES (2, 'Size M', 35000, 21000);
INSERT INTO productvariants (ProductId, Size, Price, Cost) VALUES (3, 'Size S', 25000, 15000);
INSERT INTO productvariants (ProductId, Size, Price, Cost) VALUES (4, 'Size S', 45000, 27000);
INSERT INTO productvariants (ProductId, Size, Price, Cost) VALUES (5, 'Size S', 45000, 27000);
INSERT INTO productvariants (ProductId, Size, Price, Cost) VALUES (6, 'Không', 5000, 2000);
INSERT INTO productvariants (ProductId, Size, Price, Cost) VALUES (7, 'Không', 5000, 2000);
INSERT INTO ingredients (Name, Unit, Description) VALUES ('Sữa', 'lít', 'Sữa tươi');
INSERT INTO ingredients (Name, Unit, Description) VALUES ('Đường', 'kg', 'Đường trắng');
INSERT INTO ingredients (Name, Unit, Description) VALUES ('Lá trà', 'kg', 'Lá trà xanh');
INSERT INTO ingredients (Name, Unit, Description) VALUES ('Hạt cà phê', 'kg', 'Hạt Arabica');
INSERT INTO ingredients (Name, Unit, Description) VALUES ('Sốt trái cây', 'lít', 'Sốt trái cây tổng hợp');
INSERT INTO recipedetails (ProductVariantId, IngredientId, Quantity, Unit) VALUES (1, 1, 0.2, 'lít');
INSERT INTO recipedetails (ProductVariantId, IngredientId, Quantity, Unit) VALUES (2, 2, 0.1, 'Kg');
INSERT INTO recipedetails (ProductVariantId, IngredientId, Quantity, Unit) VALUES (3, 3, 0.15, 'Kg');
INSERT INTO recipedetails (ProductVariantId, IngredientId, Quantity, Unit) VALUES (4, 4, 0.25, 'Kg');
INSERT INTO recipedetails (ProductVariantId, IngredientId, Quantity, Unit) VALUES (5, 5, 0.3, 'lít');
INSERT INTO customers (Name, Email, Phone, Address) VALUES ('Nguyen Van A', 'a@example.com', '0901234567', 'Hanoi');
INSERT INTO customers (Name, Email, Phone, Address) VALUES ('Tran Thi B', 'b@example.com', '0912345678', 'Ho Chi Minh');
INSERT INTO customers (Name, Email, Phone, Address) VALUES ('Le Van C', 'c@example.com', '0923456789', 'Da Nang');
INSERT INTO customers (Name, Email, Phone, Address) VALUES ('Pham Thi D', 'd@example.com', '0934567890', 'Can Tho');
INSERT INTO customers (Name, Email, Phone, Address) VALUES ('Hoang Van E', 'e@example.com', '0945678901', 'Hai Phong');
INSERT INTO expensecategories (CategoryName, Description) VALUES ('Nguyên liệu', 'Chi phí nguyên liệu');
INSERT INTO expensecategories (CategoryName, Description) VALUES ('Tiện ích', 'Chi phí điện nước');
INSERT INTO expensecategories (CategoryName, Description) VALUES ('Nhân viên', 'Lương nhân viên');
INSERT INTO expensecategories (CategoryName, Description) VALUES ('Tiếp thị', 'Chi phí quảng cáo');
INSERT INTO expensecategories (CategoryName, Description) VALUES ('Thiết bị', 'Bảo trì máy móc');
INSERT INTO expenses (ExpenseCategoryId, Amount, expensedate) VALUES (1, 5000000, current_timestamp);
INSERT INTO expenses (ExpenseCategoryId, Amount, expensedate) VALUES (2, 2000000, current_timestamp);
INSERT INTO expenses (ExpenseCategoryId, Amount, expensedate) VALUES (3, 10000000, current_timestamp);
INSERT INTO expenses (ExpenseCategoryId, Amount, expensedate) VALUES (4, 3000000, current_timestamp);
INSERT INTO expenses (ExpenseCategoryId, Amount, expensedate) VALUES (5, 4000000, current_timestamp);
INSERT INTO suppliers (Name, Email, Phone, Address) VALUES ('Vinamilk', 'contact@vinamilk.com', '02412345678', 'Hanoi');
INSERT INTO suppliers (Name, Email, Phone, Address) VALUES ('SugarCo', 'info@sugarco.com', '02823456789', 'Ho Chi Minh');
INSERT INTO suppliers (Name, Email, Phone, Address) VALUES ('TeaFarm', 'sales@teafarm.com', '02334567890', 'Da Nang');
INSERT INTO suppliers (Name, Email, Phone, Address) VALUES ('CoffeeBean', 'support@coffeebean.com', '02545678901', 'Can Tho');
INSERT INTO suppliers (Name, Email, Phone, Address) VALUES ('FruitPuree', 'order@fruitpuree.com', '02756789012', 'Hai Phong');
INSERT INTO inbounds (IngredientId, Quantity, SupplierId, Notes, TotalCost, inbounddate, expirydate) VALUES (1, 100, 1, 'Lô sữa tươi', 2000000, current_timestamp, current_timestamp);
INSERT INTO inbounds (IngredientId, Quantity, SupplierId, Notes, TotalCost, inbounddate, expirydate) VALUES (2, 50, 2, 'Đường trắng', 3000000, current_timestamp, current_timestamp);
INSERT INTO inbounds (IngredientId, Quantity, SupplierId, Notes, TotalCost, inbounddate, expirydate) VALUES (3, 80, 3, 'Lá trà', 500000, current_timestamp, current_timestamp);
INSERT INTO inbounds (IngredientId, Quantity, SupplierId, Notes, TotalCost, inbounddate, expirydate) VALUES (4, 60, 4, 'Hạt cà phê', 100000, current_timestamp, current_timestamp);
INSERT INTO inbounds (IngredientId, Quantity, SupplierId, Notes, TotalCost, inbounddate, expirydate) VALUES (5, 70, 5, 'Sốt trái cây', 300000, current_timestamp, current_timestamp);
INSERT INTO invoices (CustomerId, TotalAmount, invoicedate) VALUES (1, 150000, current_timestamp);
INSERT INTO invoices (CustomerId, TotalAmount, invoicedate) VALUES (2, 200000, current_timestamp);
INSERT INTO invoices (CustomerId, TotalAmount, invoicedate) VALUES (3, 300000, current_timestamp);
INSERT INTO invoices (CustomerId, TotalAmount, invoicedate) VALUES (4, 250000, current_timestamp);
INSERT INTO invoices (CustomerId, TotalAmount, invoicedate) VALUES (5, 180000, current_timestamp);
INSERT INTO inventories (InboundId, Quantity, inbounddate, expirydate) VALUES (1, 90, current_timestamp, current_timestamp);
INSERT INTO inventories (InboundId, Quantity, inbounddate, expirydate) VALUES (2, 45, current_timestamp, current_timestamp);
INSERT INTO inventories (InboundId, Quantity, inbounddate, expirydate) VALUES (3, 75, current_timestamp, current_timestamp);
INSERT INTO inventories (InboundId, Quantity, inbounddate, expirydate) VALUES (4, 55, current_timestamp, current_timestamp);
INSERT INTO inventories (InboundId, Quantity, inbounddate, expirydate) VALUES (5, 65, current_timestamp, current_timestamp);
INSERT INTO invoicedetails (InvoiceId, ProductId, SugarLevel, IceLevel, Quantity) VALUES (1, 1, 50, 70, 1);
INSERT INTO invoicedetails (InvoiceId, ProductId, SugarLevel, IceLevel, Quantity) VALUES (2, 2, 30, 50, 1);
INSERT INTO invoicedetails (InvoiceId, ProductId, SugarLevel, IceLevel, Quantity) VALUES (3, 3, 40, 60, 1);
INSERT INTO invoicedetails (InvoiceId, ProductId, SugarLevel, IceLevel, Quantity) VALUES (4, 1, 60, 80, 2);
INSERT INTO invoicedetails (InvoiceId, ProductId, SugarLevel, IceLevel, Quantity) VALUES (5, 2, 20, 40, 3);
INSERT INTO payments (InvoiceId, Amount, PaymentMethod, paymentdate) VALUES (1, 150000, 'Cash', current_timestamp);
INSERT INTO payments (InvoiceId, Amount, PaymentMethod, paymentdate) VALUES (2, 200000, 'Credit Card', current_timestamp);
INSERT INTO payments (InvoiceId, Amount, PaymentMethod, paymentdate) VALUES (3, 300000, 'Mobile', current_timestamp);
INSERT INTO payments (InvoiceId, Amount, PaymentMethod, paymentdate) VALUES (4, 250000, 'Cash', current_timestamp);
INSERT INTO payments (InvoiceId, Amount, PaymentMethod, paymentdate) VALUES (5, 180000, 'Bank Transfer', current_timestamp);
INSERT INTO ordertoppings (InvoiceDetailId, ProductId, Quantity) VALUES (1, 2, 1);
INSERT INTO ordertoppings (InvoiceDetailId, ProductId, Quantity) VALUES (2, 2, 2);
INSERT INTO ordertoppings (InvoiceDetailId, ProductId, Quantity) VALUES (3, 2, 3);
INSERT INTO ordertoppings (InvoiceDetailId, ProductId, Quantity) VALUES (4, 2, 2);
INSERT INTO ordertoppings (InvoiceDetailId, ProductId, Quantity) VALUES (5, 2, 1);
INSERT INTO outbounds (InventoryId, Quantity, Purpose, Notes, outbounddate) VALUES (1, 10, 'Sản xuất', 'Dành cho cà phê đen', current_timestamp);
INSERT INTO outbounds (InventoryId, Quantity, Purpose, Notes, outbounddate) VALUES (2, 5, 'Sản xuất', 'Dành cho trà sữa', current_timestamp);
INSERT INTO outbounds (InventoryId, Quantity, Purpose, Notes, outbounddate) VALUES (3, 8, 'Sản xuất', 'Dành cho trà xanh', current_timestamp);
INSERT INTO outbounds (InventoryId, Quantity, Purpose, Notes, outbounddate) VALUES (4, 6, 'Sản xuất', 'Dành cho đá xay xoài', current_timestamp);
INSERT INTO outbounds (InventoryId, Quantity, Purpose, Notes, outbounddate) VALUES (5, 7, 'Sản xuất', 'Dành cho cà phê espresso', current_timestamp);
INSERT INTO taxes (TaxName, TaxRate) VALUES ('VAT 10%', '0.10');
INSERT INTO taxes (TaxName, TaxRate) VALUES ('Service Tax 5%', '0.05');
INSERT INTO taxes (TaxName, TaxRate) VALUES ('Special Tax 2%', '0.02');
INSERT INTO taxes (TaxName, TaxRate) VALUES ('No Tax', '0.00');
INSERT INTO taxes (TaxName, TaxRate) VALUES ('VAT 8%', '0.08');
INSERT INTO invoicetaxes (InvoiceId, TaxId) VALUES (1, 1);
INSERT INTO invoicetaxes (InvoiceId, TaxId) VALUES (2, 2);
INSERT INTO invoicetaxes (InvoiceId, TaxId) VALUES (3, 3);
INSERT INTO invoicetaxes (InvoiceId, TaxId) VALUES (4, 4);
INSERT INTO invoicetaxes (InvoiceId, TaxId) VALUES (5, 5);
INSERT INTO checkinventories (InventoryId, TheoryQuantity, ActualQuantity, Notes, checkdate) VALUES (2, 45, 43, 'Thiếu do hao hụt', current_timestamp);
INSERT INTO checkinventories (InventoryId, TheoryQuantity, ActualQuantity, Notes, checkdate) VALUES (4, 55, 55, 'Đủ', current_timestamp);
INSERT INTO checkinventories (InventoryId, TheoryQuantity, ActualQuantity, Notes, checkdate) VALUES (3, 75, 76, 'Dư có thể hoàn kho', current_timestamp);