-- DeliveryApp MySQL Database Schema

CREATE DATABASE IF NOT EXISTS DeliveryAppDb;
USE DeliveryAppDb;

-- 1. Drivers Table
-- Stores the delivery personnel who will log into the mobile app
CREATE TABLE IF NOT EXISTS Drivers (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    FullName VARCHAR(100) NOT NULL,
    PhoneNumber VARCHAR(20) NOT NULL UNIQUE,
    PasswordHash VARCHAR(255) NOT NULL, -- Keep this secure!
    IsActive BOOLEAN DEFAULT TRUE,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
);

-- 2. ShopOwners Table
-- Stores the vendors/shop owners who are requesting the delivery
CREATE TABLE IF NOT EXISTS ShopOwners (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    ShopName VARCHAR(150) NOT NULL,
    OwnerName VARCHAR(100) NOT NULL,
    PhoneNumber VARCHAR(20) NOT NULL UNIQUE,
    Address TEXT,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
);

-- 3. Orders Table
-- Stores the delivery orders. Based on the Order.cs model in the MAUI app.
CREATE TABLE IF NOT EXISTS Orders (
    OrderId VARCHAR(50) PRIMARY KEY, -- Using VARCHAR to support UUIDs or custom order numbers (e.g., "ORD-123")
    CustomerName VARCHAR(100) NOT NULL,
    CustomerPhone VARCHAR(20) NOT NULL,
    ShopOwnerPhone VARCHAR(20) NOT NULL,
    DriverPhone VARCHAR(20),
    Items TEXT NOT NULL,
    Location TEXT NOT NULL,
    OrderDate DATETIME DEFAULT CURRENT_TIMESTAMP,
    Status VARCHAR(30) DEFAULT 'Assigned', -- "Pending", "Assigned", "OutForDelivery", "Delivered"
    
    -- Foreign Key Constraints (Optional but recommended for data integrity)
    FOREIGN KEY (DriverPhone) REFERENCES Drivers(PhoneNumber) ON DELETE SET NULL,
    FOREIGN KEY (ShopOwnerPhone) REFERENCES ShopOwners(PhoneNumber) ON DELETE CASCADE
);

-- ==========================================
-- INSERT SOME DUMMY DATA FOR LOCAL TESTING
-- ==========================================

-- Insert a dummy driver
INSERT IGNORE INTO Drivers (FullName, PhoneNumber, PasswordHash) 
VALUES ('John Doe', '555-0100', 'dummy_hash_for_now');

-- Insert a dummy shop owner
INSERT IGNORE INTO ShopOwners (ShopName, OwnerName, PhoneNumber, Address)
VALUES ('Super Mart', 'Alice Smith', '555-0200', '123 Market St');

-- Insert a dummy order assigned to the driver
INSERT IGNORE INTO Orders (OrderId, CustomerName, CustomerPhone, ShopOwnerPhone, DriverPhone, Items, Location, Status)
VALUES (
    'ORD-1001', 
    'Bob Johnson', 
    '555-0300', 
    '555-0200', 
    '555-0100', 
    '2x Apples, 1x Bread', 
    '456 Residential Blvd', 
    'Assigned'
);
