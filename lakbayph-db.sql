CREATE DATABASE IF NOT EXISTS lakbayph_web;
USE lakbayph_web;

CREATE TABLE Users (
    UserID INT PRIMARY KEY AUTO_INCREMENT,
    Username VARCHAR(50) NOT NULL UNIQUE,
    Email VARCHAR(100) NOT NULL UNIQUE,
    Password VARCHAR(255) NOT NULL UNIQUE,
    FirstName VARCHAR(50) NOT NULL,
    LastName VARCHAR(50) NOT NULL,
    Phone VARCHAR(20),
    Address VARCHAR(255),
    DateOfBirth DATE,
    Gender Enum ('Male', 'Female', 'Other', 'Prefer not to say'),
    Role ENUM('Admin', 'User') NOT NULL DEFAULT 'User',
    IsActive BOOLEAN NOT NULL DEFAULT 1,
    CreatedAt TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE TourCategories (
    CategoryID INT PRIMARY KEY AUTO_INCREMENT, -- '1 - International', '2 - Domestic', '3 - Freediving'
    CategoryName VARCHAR(100) NOT NULL UNIQUE,
    CategoryType VARCHAR(50) NOT NULL, -- 'International', 'Domestic', 'Freediving'
    IsActive BOOLEAN NOT NULL DEFAULT 1,
    CreatedAt TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE TourPackages (
    PackageID VARCHAR(10) PRIMARY KEY,
    Title VARCHAR(200) NOT NULL,
    Location VARCHAR(200) NOT NULL,
    PackageType VARCHAR(50) NOT NULL, -- 'International', 'Domestic', 'Freediving'
    Price DECIMAL(10,2) NOT NULL,
    Duration INT NOT NULL, -- Duration in days
    DurationNights INT NOT NULL DEFAULT 0, -- Duration in nights
    CategoryID INT NOT NULL,
    TotalSlots INT NOT NULL DEFAULT 0,
    AvailableSlots INT NOT NULL DEFAULT 0,
    StartDate DATE NOT NULL,
    EndDate DATE NOT NULL,
    IsGroupPackage BOOLEAN NOT NULL DEFAULT 0,
    GroupSize INT DEFAULT 1,
    IsActive BOOLEAN NOT NULL DEFAULT 1,
    CreatedAt TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (CategoryID) REFERENCES TourCategories(CategoryID)
);

CREATE TABLE PackageAddOns (
    AddOnID INT PRIMARY KEY AUTO_INCREMENT,
    PackageID VARCHAR(10) NOT NULL,
    AddOnName VARCHAR(200) NOT NULL,
    Price DECIMAL(10,2) NOT NULL,
    Unit VARCHAR(50), -- 'per session', 'per dive', 'per day', 'per person', 'per pet'
    IsActive BOOLEAN NOT NULL DEFAULT 1,
    CreatedAt TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (PackageID) REFERENCES TourPackages(PackageID)
);

CREATE TABLE Bookings (
    BookingID INT PRIMARY KEY AUTO_INCREMENT,
    BookingReference VARCHAR(20) NOT NULL UNIQUE,
    UserID INT NOT NULL,
    PackageID VARCHAR(10) NOT NULL,
    Destination VARCHAR(100) NOT NULL,
    NumberOfPeople INT NOT NULL DEFAULT 1,
    FinalAmount DECIMAL(10,2) NOT NULL,
    BookingDate TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    Status VARCHAR(20) NOT NULL DEFAULT 'Pending', -- 'Pending', 'Confirmed', 'Cancelled', 'Completed'
    PaymentMethod VARCHAR(20) NOT NULL DEFAULT 'Pay on Trip',
    PaymentStatus VARCHAR(20) NOT NULL DEFAULT 'Pending', -- 'Pending', 'Paid', 'Refunded'
    CreatedAt TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (UserID) REFERENCES Users(UserID),
    FOREIGN KEY (PackageID) REFERENCES TourPackages(PackageID)
);

CREATE TABLE BookingHistory (
    UserID INT,
    HistoryID INT PRIMARY KEY AUTO_INCREMENT,
    BookingID INT NOT NULL,
	Destination VARCHAR(100) NOT NULL,
    TravelDate DATE NOT NULL,
	Duration INT NOT NULL,
    FinalAmount DECIMAL(10,2) NOT NULL,
    Rating INT CHECK (Rating BETWEEN 1 AND 5),
    FOREIGN KEY (BookingID) REFERENCES Bookings(BookingID),
    FOREIGN KEY (UserID) REFERENCES Users(UserID)
);

CREATE USER 'admin'@'localhost' IDENTIFIED BY 'lphttadmin';
GRANT ALL PRIVILEGES ON lakbayph_web.* TO 'admin'@'localhost';

CREATE USER 'user'@'localhost' IDENTIFIED BY 'lphttuser';
GRANT SELECT, INSERT, UPDATE ON lakbayph_web.Users TO 'user'@'localhost';
GRANT SELECT ON lakbayph_web.TourPackages TO 'user'@'localhost';
GRANT INSERT ON lakbayph_web.Bookings TO 'user'@'localhost';

INSERT INTO Users (Username, Email, Password, FirstName, LastName, Role)
VALUES ('admin_ken', 'adminken@email.com', 'lphttken', 'Ken', 'Harold', 'Admin');

select * from Users;

REPAIR TABLE mysql.db;