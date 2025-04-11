-- Tạo bảng Ingredients
CREATE TABLE Ingredients (
    Id SERIAL PRIMARY KEY,
    Name VARCHAR(255) NOT NULL,
    Unit VARCHAR(50),
    Description TEXT
);

-- Tạo bảng Suppliers
CREATE TABLE Suppliers (
    Id SERIAL PRIMARY KEY,
    Name VARCHAR(255) NOT NULL,
    Email VARCHAR(255),
    Phone VARCHAR(50),
    Address TEXT
);

-- Tạo bảng Inbounds
CREATE TABLE Inbounds (
    Id SERIAL PRIMARY KEY,
    IngredientId INT NOT NULL,
    Quantity REAL NOT NULL,
    InboundDate TIMESTAMP NOT NULL,
    ExpiryDate TIMESTAMP NOT NULL,
    SupplierId INT NOT NULL,
    Notes TEXT,
    TotalCost REAL NOT NULL,
    CONSTRAINT fk_ingredient FOREIGN KEY (IngredientId) REFERENCES Ingredients(Id),
    CONSTRAINT fk_supplier FOREIGN KEY (SupplierId) REFERENCES Suppliers(Id)
);

-- Tạo bảng Inventories
CREATE TABLE Inventories (
    Id SERIAL PRIMARY KEY,
    InboundId INT NOT NULL,
    Quantity REAL NOT NULL,
    InboundDate TIMESTAMP NOT NULL,
    ExpiryDate TIMESTAMP NOT NULL,
    CONSTRAINT fk_inbound FOREIGN KEY (InboundId) REFERENCES Inbounds(Id)
);

-- Tạo bảng CheckInventories
CREATE TABLE CheckInventories (
    Id SERIAL PRIMARY KEY,
    InventoryId INT NOT NULL,
    TheoryQuantity REAL NOT NULL,
    ActualQuantity REAL NOT NULL,
    CheckDate TIMESTAMP NOT NULL,
    Notes TEXT,
    CONSTRAINT fk_inventory FOREIGN KEY (InventoryId) REFERENCES Inventories(Id)
);

-- Tạo bảng Outbounds
CREATE TABLE Outbounds (
    Id SERIAL PRIMARY KEY,
    InventoryId INT NOT NULL,
    Quantity REAL NOT NULL,
    OutboundDate TIMESTAMP NOT NULL,
    Purpose TEXT,
    Notes TEXT,
    CONSTRAINT fk_inventory FOREIGN KEY (InventoryId) REFERENCES Inventories(Id)
);

-- Tạo bảng Customers
CREATE TABLE Customers (
    Id SERIAL PRIMARY KEY,
    Name VARCHAR(255) NOT NULL,
    Email VARCHAR(255),
    Phone VARCHAR(50),
    Address TEXT
);

-- Tạo bảng Categories
CREATE TABLE Categories (
    Id SERIAL PRIMARY KEY,
    Name VARCHAR(255) NOT NULL,
    ImageUrl TEXT
);

-- Tạo bảng Products
CREATE TABLE Products (
    Id SERIAL PRIMARY KEY,
    Name VARCHAR(255) NOT NULL,
    CategoryId INT,
    IsActive BOOLEAN NOT NULL DEFAULT TRUE,
    IsTopping BOOLEAN,
    Description TEXT,
    ImageUrl TEXT,
    CONSTRAINT fk_category FOREIGN KEY (CategoryId) REFERENCES Categories(Id)
);

-- Tạo bảng ProductVariants
CREATE TABLE ProductVariants (
    Id SERIAL PRIMARY KEY,
    ProductId INT NOT NULL,
    Size VARCHAR(20),
    Price REAL,
    Cost REAL,
    CONSTRAINT fk_product FOREIGN KEY (ProductId) REFERENCES Products(Id)
);

-- Tạo bảng RecipeDetails
CREATE TABLE RecipeDetails (
    Id SERIAL PRIMARY KEY,
    ProductVariantId INT NOT NULL,
    IngredientId INT NOT NULL,
    Quantity REAL NOT NULL,
    Unit VARCHAR(50),
    CONSTRAINT fk_product_variant FOREIGN KEY (ProductVariantId) REFERENCES ProductVariants(Id),
    CONSTRAINT fk_ingredient FOREIGN KEY (IngredientId) REFERENCES Ingredients(Id)
);

-- Tạo bảng Invoices
CREATE TABLE Invoices (
    Id SERIAL PRIMARY KEY,
    CustomerId INT,
    InvoiceDate TIMESTAMP NOT NULL,
    TotalAmount REAL NOT NULL DEFAULT 0.0,
    DeliveryFee REAL NOT NULL DEFAULT 0.0,
    OrderType VARCHAR(50),
    PaymentMethod VARCHAR(50),
    CreatedAt TIMESTAMP,
    UpdatedAt TIMESTAMP,
    CONSTRAINT fk_customer FOREIGN KEY (CustomerId) REFERENCES Customers(Id)
);

-- Tạo bảng InvoiceDetails
CREATE TABLE InvoiceDetails (
    Id SERIAL PRIMARY KEY,
    InvoiceId INT NOT NULL,
    ProductId INT NOT NULL,
    SugarLevel INT,
    IceLevel INT,
    Quantity INT,
    CONSTRAINT fk_invoice FOREIGN KEY (InvoiceId) REFERENCES Invoices(Id),
    CONSTRAINT fk_product_variant FOREIGN KEY (ProductId) REFERENCES ProductVariants(Id)
);

-- Tạo bảng OrderToppings
CREATE TABLE OrderToppings (
    Id SERIAL PRIMARY KEY,
    InvoiceDetailId INT NOT NULL,
    ProductId INT NOT NULL,
	Quantity INT,
    CONSTRAINT fk_invoice_detail FOREIGN KEY (InvoiceDetailId) REFERENCES InvoiceDetails(Id),
    CONSTRAINT fk_product_variant FOREIGN KEY (ProductId) REFERENCES ProductVariants(Id)
);

-- Tạo bảng Taxes
CREATE TABLE Taxes (
    Id SERIAL PRIMARY KEY,
    TaxName VARCHAR(255) NOT NULL,
    TaxRate DECIMAL NOT NULL
);

-- Tạo bảng InvoiceTaxes
CREATE TABLE InvoiceTaxes (
    Id SERIAL PRIMARY KEY,
    InvoiceId INT NOT NULL,
    TaxId INT NOT NULL,
    CONSTRAINT fk_invoice FOREIGN KEY (InvoiceId) REFERENCES Invoices(Id),
    CONSTRAINT fk_tax FOREIGN KEY (TaxId) REFERENCES Taxes(Id)
);

-- Tạo bảng Payments
CREATE TABLE Payments (
    Id SERIAL PRIMARY KEY,
    InvoiceId INT NOT NULL,
    PaymentDate TIMESTAMP NOT NULL,
    Amount REAL NOT NULL,
    PaymentMethod VARCHAR(255) NOT NULL,
    CONSTRAINT fk_invoice FOREIGN KEY (InvoiceId) REFERENCES Invoices(Id)
);

-- Tạo bảng ExpenseCategories
CREATE TABLE ExpenseCategories (
    Id SERIAL PRIMARY KEY,
    CategoryName VARCHAR(255) NOT NULL,
    Description TEXT
);

-- Tạo bảng Expenses
CREATE TABLE Expenses (
    Id SERIAL PRIMARY KEY,
    ExpenseCategoryId INT NOT NULL,
    Note TEXT,
    ReceiptUrl TEXT,
    CreatedAt TIMESTAMP,
    UpdatedAt TIMESTAMP,
    Amount REAL NOT NULL,
    ExpenseDate TIMESTAMP NOT NULL,
    CONSTRAINT fk_expense_category FOREIGN KEY (ExpenseCategoryId) REFERENCES ExpenseCategories(Id)
);