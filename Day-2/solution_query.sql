-- CREATE DATABASE Employees;
USE Employees;

-- Create Department table before Employee and Sales since it's referenced in both
CREATE TABLE Department (
    DeptName VARCHAR(50) PRIMARY KEY,
    FloorNo INT,
    Phone VARCHAR(20),
    ManagerID INT NOT NULL -- will be FK to Employee (will add later)
);

-- Insert data into Department table
INSERT INTO Department (DeptName, FloorNo, Phone, ManagerID) VALUES
('Management', 5, '34', 1),
('Books', 1, '81', 4),
('Clothes', 2, '24', 4),
('Equipment', 3, '57', 3),
('Furniture', 4, '14', 3),
('Navigation', 1, '41', 3),
('Recreation', 2, '29', 4),
('Accounting', 5, '35', 5),
('Purchasing', 5, '36', 7),
('Personnel', 5, '37', 9),
('Marketing', 5, '38', 2);

-- Create Employee table
CREATE TABLE Employee (
    EmployeeID INT PRIMARY KEY,
    Name VARCHAR(30),
    Salary DECIMAL(10, 2),
    DeptName VARCHAR(50) NULL,
    SupervisorID INT NULL,
    CONSTRAINT FK_Emp_Dept FOREIGN KEY (DeptName) REFERENCES Department(DeptName),
    CONSTRAINT FK_Emp_Supervisor FOREIGN KEY (SupervisorID) REFERENCES Employee(EmployeeID)
);

-- Now insert data into Employee table
INSERT INTO Employee (EmployeeID, Name, Salary, DeptName, SupervisorID) VALUES
(1, 'Alice', 75000.00, 'Management', NULL),
(2, 'Ned', 45000.00, 'Marketing', 1),
(3, 'Andrew', 25000.00, 'Marketing', 2),
(4, 'Clare', 22000.00, 'Marketing', 2),
(5, 'Todd', 38000.00, 'Accounting', 1),
(6, 'Nancy', 22000.00, 'Accounting', 5),
(7, 'Brier', 43000.00, 'Purchasing', 1),
(8, 'Sarah', 56000.00, 'Purchasing', 7),
(9, 'Sophile', 35000.00, 'Personnel', 1),
(10, 'Sanjay', 15000.00, 'Navigation', 3),
(11, 'Rita', 15000.00, 'Books', 4),
(12, 'Gigi', 16000.00, 'Clothes', 4),
(13, 'Maggie', 11000.00, 'Clothes', 4),
(14, 'Paul', 15000.00, 'Equipment', 3),
(15, 'James', 15000.00, 'Equipment', 3),
(16, 'Pat', 15000.00, 'Furniture', 3),
(17, 'Mark', 15000.00, 'Recreation', 3);

-- Now let's add the foreign key from Department.ManagerID to EMployee.EmployeeID
ALTER TABLE Department
ADD CONSTRAINT FK_Dept_Manager FOREIGN KEY (ManagerID) REFERENCES Employee(EmployeeID);

-- Create Item table
CREATE TABLE Item (
    ItemName VARCHAR(100) PRIMARY KEY,
    ItemType VARCHAR(50),
    ItemColor VARCHAR(50)
);

-- Insert data into Item table
INSERT INTO Item (ItemName, ItemType, ItemColor) VALUES
('Pocket Knife-Nile', 'E', 'Brown'),
('Pocket Knife-Avon', 'E', 'Brown'),
('Compass', 'N', NULL),
('Geo positioning system', 'N', NULL),
('Elephant Polo stick', 'R', 'Bamboo'),
('Camel Saddle', 'R', 'Brown'),
('Sextant', 'N', NULL),
('Map Measure', 'N', NULL),
('Boots-snake proof', 'C', 'Green'),
('Pith Helmet', 'C', 'Khaki'),
('Hat-polar Explorer', 'C', 'White'),
('Exploring in 10 Easy Lessons', 'B', NULL),
('Hammock', 'F', 'Khaki'),
('How to win Foreign Friends', 'B', NULL),
('Map case', 'E', 'Brown'),
('Safari Chair', 'F', 'Khaki'),
('Safari cooking kit', 'F', 'Khaki'),
('Stetson', 'C', 'Black'),
('Tent - 2 person', 'F', 'Khaki'),
('Tent -8 person', 'F', 'Khaki');

-- Create Sales table
CREATE TABLE Sales (
    SalesID INT PRIMARY KEY,
    SalesQty INT,
    ItemName VARCHAR(100) NOT NULL,
    DeptName VARCHAR(50) NOT NULL,
    CONSTRAINT FK_Sales_Item FOREIGN KEY (ItemName) REFERENCES Item(ItemName),
    CONSTRAINT FK_Sales_Dept FOREIGN KEY (DeptName) REFERENCES Department(DeptName)
);

-- Insert data into Sales table
INSERT INTO Sales (SalesID, SalesQty, ItemName, DeptName) VALUES
(101, 2, 'Boots-snake proof', 'Clothes'),
(102, 1, 'Pith Helmet', 'Clothes'),
(103, 1, 'Sextant', 'Navigation'),
(104, 3, 'Hat-polar Explorer', 'Clothes'),
(105, 5, 'Pith Helmet', 'Equipment'),
(106, 2, 'Pocket Knife-Nile', 'Clothes'),
(107, 3, 'Pocket Knife-Nile', 'Recreation'),
(108, 1, 'Compass', 'Navigation'),
(109, 2, 'Geo positioning system', 'Navigation'),
(110, 5, 'Map Measure', 'Navigation'),
(111, 1, 'Geo positioning system', 'Books'),
(112, 1, 'Sextant', 'Books'),
(113, 3, 'Pocket Knife-Nile', 'Books'),
(114, 1, 'Pocket Knife-Nile', 'Navigation'),
(115, 1, 'Pocket Knife-Nile', 'Equipment'),
(116, 1, 'Sextant', 'Clothes'),
(117, 1, 'Sextant', 'Equipment'),
(118, 1, 'Sextant', 'Recreation'),
(119, 1, 'Sextant', 'Furniture'),
(120, 1, 'Pocket Knife-Nile', 'Furniture'),
(121, 1, 'Exploring in 10 Easy Lessons', 'Books'),
(122, 1, 'How to win Foreign Friends', 'Books'),
(123, 1, 'Compass', 'Books'),
(124, 1, 'Pith Helmet', 'Books'),
(125, 1, 'Elephant Polo stick', 'Recreation'),
(126, 1, 'Camel Saddle', 'Recreation');
