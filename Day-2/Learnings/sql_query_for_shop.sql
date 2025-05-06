USE ShopDB;

-- Create Country Table
CREATE TABLE Country (
    Country_ID INT PRIMARY KEY IDENTITY(1,1),
    Name VARCHAR(50) NOT NULL UNIQUE     
);

-- Create State Table
CREATE TABLE State (
    State_ID INT PRIMARY KEY IDENTITY(1,1),
    Name VARCHAR(50) NOT NULL,
    Country_ID INT NOT NULL,
    CONSTRAINT FK_State_Country FOREIGN KEY (Country_ID) REFERENCES Country(Country_ID)
);

-- Create City Table
CREATE TABLE City (
    City_ID INT PRIMARY KEY IDENTITY(1,1),  
    Name VARCHAR(50) NOT NULL,
    State_ID INT NOT NULL,
    CONSTRAINT FK_City_State FOREIGN KEY (State_ID) REFERENCES State(State_ID)
);

-- Create Area Table
CREATE TABLE Area (
    Zipcode VARCHAR(20) PRIMARY KEY, 
    Name VARCHAR(50),
    City_ID INT NOT NULL,
    CONSTRAINT FK_Area_City FOREIGN KEY (City_ID) REFERENCES City(City_ID)
);

-- Create Address Table
CREATE TABLE Address (
    Address_ID INT PRIMARY KEY IDENTITY(1,1),
    Door_number VARCHAR(50),
    Address_Line VARCHAR(255) NOT NULL,
    Zipcode VARCHAR(20) NOT NULL,
    CONSTRAINT FK_Address_Area FOREIGN KEY (Zipcode) REFERENCES Area(Zipcode)
);

-- Create Products Table
CREATE TABLE Products (
    Product_ID INT PRIMARY KEY IDENTITY(1,1),
    Name VARCHAR(70) NOT NULL,
    Brand VARCHAR(50),
    Image VARCHAR(255), --Url or Path                       
    Price DECIMAL(10, 2) NOT NULL CHECK (Price >= 0),
    Stock INT NOT NULL CHECK (Stock >= 0)
);

-- Create Suppliers Table
CREATE TABLE Suppliers (
    Supplier_ID INT PRIMARY KEY IDENTITY(1,1),
    Supplier_Name VARCHAR(50) NOT NULL,
    Contact_Person VARCHAR(50),
    Phone VARCHAR(50), 
    Email VARCHAR(50),
    Address_ID INT,
    CONSTRAINT FK_Suppliers_Address FOREIGN KEY (Address_ID) REFERENCES Address(Address_ID)
);

-- Create Product_Supplier Table
CREATE TABLE Product_Supplier (
    Transaction_ID INT PRIMARY KEY IDENTITY(1,1), --Good to have deidicated PK instead of composite key
    Product_ID INT NOT NULL,
    Supplier_ID INT NOT NULL,
    Quantity INT NOT NULL CHECK (Quantity > 0),
    Supply_Price DECIMAL(10, 2) NOT NULL CHECK (Supply_Price >= 0),
    Supply_Date DATETIME NOT NULL,
    CONSTRAINT FK_Product_Supplier_Products FOREIGN KEY (Product_ID) REFERENCES Products(Product_ID), 
    CONSTRAINT FK_Product_Supplier_Suppliers FOREIGN KEY (Supplier_ID) REFERENCES Suppliers(Supplier_ID) 
);

-- Create Customer Table
CREATE TABLE Customer (
	Customer_ID INT PRIMARY KEY IDENTITY(1,1),
	Name VARCHAR(50) NOT NULL,
    Email VARCHAR(70) UNIQUE,
    Phone VARCHAR(20), 
    Address_ID INT,
    CONSTRAINT FK_Customer_Address FOREIGN KEY (Address_ID) REFERENCES Address(Address_ID)
);


-- Create Bills Table
CREATE TABLE Bills (
    Bill_ID INT PRIMARY KEY IDENTITY(1,1),
    Customer_ID INT NOT NULL,
    Bill_Date DATETIME NOT NULL,
    Bill_Status VARCHAR(50) NOT NULL DEFAULT 'Processing',
    CONSTRAINT CHK_Bill_Status CHECK (Bill_Status IN ('Pending Payment', 'Paid', 'Cancelled', 'Processing', 'Completed', 'Refunded')),
    CONSTRAINT FK_Bills_Customer FOREIGN KEY (Customer_ID) REFERENCES Customer(Customer_ID)
);

-- Create Bill_Items Table (Details of products in the bill)
CREATE TABLE Bill_Items (
    ID INT PRIMARY KEY IDENTITY(1,1), -- why not composite of Bill_ID, Product_ID as a product occurs only once per bill?
    Bill_ID INT NOT NULL,
    Product_ID INT NOT NULL,
    Quantity INT NOT NULL CHECK (Quantity > 0),
    Purchase_Price DECIMAL(10, 2) NOT NULL CHECK (Purchase_Price >= 0),
    CONSTRAINT FK_Bill_Items_Bills FOREIGN KEY (Bill_ID) REFERENCES Bills(Bill_ID),
    CONSTRAINT FK_Bill_Items_Products FOREIGN KEY (Product_ID) REFERENCES Products(Product_ID)
);

-- Create Payments Table
CREATE TABLE Payments (
    Payment_ID INT PRIMARY KEY IDENTITY(1,1),
    Bill_ID INT NOT NULL,
    Payment_Method VARCHAR(100),
    Payment_Status VARCHAR(50) NOT NULL DEFAULT 'Pending',
    CONSTRAINT CHK_Payment_Status CHECK (Payment_Status IN ('Success', 'Failed', 'Pending')),
    Payment_Date DATETIME NOT NULL,
    Amount DECIMAL(10, 2) NOT NULL CHECK (Amount > 0),
    CONSTRAINT FK_Payments_Bills FOREIGN KEY (Bill_ID) REFERENCES Bills(Bill_ID)
);