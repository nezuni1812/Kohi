create role anon nologin;

grant usage on schema public to anon;
GRANT SELECT ON ALL TABLES IN SCHEMA public TO anon;

ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT SELECT ON TABLES TO anon;

-- create role authenticator noinherit login password '1234';

-- grant anon to authenticator;

-- CREATE TABLE admin_account (
-- 	admin_id INT PRIMARY KEY,
-- 	email VARCHAR(100) NOT NULL,
--     full_name VARCHAR(100),
--     password VARCHAR(255) NOT NULL,
--     created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
-- );

-- CREATE TABLE Categories (
--     Id serial PRIMARY KEY,         -- Primary Key with auto-increment
--     Name varchar(255) NOT NULL,               -- Name field, required
--     ImageUrl varchar(500) NULL                -- ImageUrl field, nullable
-- );

-- CREATE TABLE Products (
--     Id serial PRIMARY KEY,         -- Primary Key with auto-increment
--     Name varchar(255) NOT NULL,               -- Name field, required
--     CategoryId INT NULL,                       -- Foreign Key to Category (nullable)
--     Price FLOAT NOT NULL,                      -- Price field, required
--     IsActive boolean NOT NULL DEFAULT true,           -- IsActive field with default value of true (1)
--     Description varchar(500) NULL,            -- Description field, nullable
--     ImageUrl varchar(500) NULL,               -- ImageUrl field, nullable
--     Cost FLOAT NOT NULL,                       -- Cost field, required
--     CONSTRAINT FK_Products_CategoryId FOREIGN KEY (CategoryId) REFERENCES Categories(Id) -- Foreign Key constraint
-- );