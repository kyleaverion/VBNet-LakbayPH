-- ================================
-- LAKBAYPH DATABASE - 2NF NORMALIZED (NO STORED TOTALS)
-- ================================

CREATE DATABASE IF NOT EXISTS lakbayph_web;
USE lakbayph_web;

-- ================================
-- MAIN ENTITY TABLE
-- ================================

CREATE TABLE Users (
    UserID INT PRIMARY KEY AUTO_INCREMENT,
    Username VARCHAR(50) NOT NULL UNIQUE,
    Email VARCHAR(100) NOT NULL UNIQUE,
    Password VARCHAR(255) NOT NULL, 
    FirstName VARCHAR(50) NOT NULL,
    LastName VARCHAR(50) NOT NULL,
    Phone VARCHAR(20),
    Address VARCHAR(255),
    BirthDate DATE,
    Gender ENUM('Male', 'Female', 'Other', 'Prefer not to say'),
    Role ENUM('Admin', 'User') NOT NULL DEFAULT 'User',
    IsActive BOOLEAN NOT NULL DEFAULT 1,
    CreatedAt TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
);


-- ================================
-- TOUR/PACKAGES TABLES
-- ================================

CREATE TABLE TourCategories (
    CategoryID INT PRIMARY KEY AUTO_INCREMENT,
    CategoryName VARCHAR(100) NOT NULL UNIQUE,
    CategoryType VARCHAR(50) NOT NULL, -- 'Freediving', 'Domestic', 'International'
    IsActive BOOLEAN NOT NULL DEFAULT 1,
    CreatedAt TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- REMOVED: TotalSlots, AvailableSlots (will be calculated from bookings)
CREATE TABLE TourPackages (
    PackageID INT PRIMARY KEY AUTO_INCREMENT,
    Title VARCHAR(200) NOT NULL,
    PackageType VARCHAR(50) NOT NULL, -- 'Freediving', 'Domestic', 'International'
    Price DECIMAL(10,2) NOT NULL,
    Description VARCHAR(200) NOT NULL,
    Duration INT NOT NULL, -- Duration in days
    DurationNights INT NOT NULL DEFAULT 0, -- Duration in nights
    CategoryID INT NOT NULL,
    MaxSlots INT NOT NULL DEFAULT 10, -- Maximum capacity (base configuration)
    Status VARCHAR(50) DEFAULT "active",
    StartDate DATE NOT NULL,
    EndDate DATE NOT NULL,
    IsGroupPackage BOOLEAN NOT NULL DEFAULT 0,
    GroupSize INT DEFAULT 1,
    IsActive BOOLEAN NOT NULL DEFAULT 1,
    CreatedAt TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (CategoryID) REFERENCES TourCategories(CategoryID)
);

CREATE TABLE PackageLocations (
    PackageID INT PRIMARY KEY,
    Location VARCHAR(200) NOT NULL,
    FOREIGN KEY (PackageID) REFERENCES TourPackages(PackageID) ON DELETE CASCADE
);

CREATE TABLE PackageAddOns (
    AddOnID INT PRIMARY KEY AUTO_INCREMENT,
    PackageID INT NOT NULL,
    AddOnName VARCHAR(200) NOT NULL,
    Price DECIMAL(10,2) NOT NULL,
    Unit VARCHAR(50), -- 'per session', 'per dive', 'per day', 'per person', 'per pet'
    IsActive BOOLEAN NOT NULL DEFAULT 1,
    CreatedAt TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (PackageID) REFERENCES TourPackages(PackageID) ON DELETE CASCADE
);

CREATE TABLE PackageDetails (
    PackageID INT PRIMARY KEY,
    PackageType ENUM('Freediving', 'Domestic', 'International') NOT NULL,
    FlagSymbol VARCHAR(10),
    IdealFor VARCHAR(255),
    Inclusions TEXT,
    ImageFileName VARCHAR(50),
    Highlights TEXT,
    FOREIGN KEY (PackageID) REFERENCES TourPackages(PackageID) ON DELETE CASCADE
);


-- ================================
-- PROMO / DISCOUNT TABLES
-- ================================

CREATE TABLE PromoTypes (
    PromoTypeID INT PRIMARY KEY AUTO_INCREMENT,
    PromoTypeName ENUM('PWD', 'SENIOR CITIZEN') NOT NULL UNIQUE,
    DiscountPercentage DECIMAL(5,2) NOT NULL DEFAULT 20.00,
    Description VARCHAR(255),
    IsActive BOOLEAN NOT NULL DEFAULT 1,
    CreatedAt TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE PromoDiscounts (
    PromoID INT PRIMARY KEY AUTO_INCREMENT,
    UserID INT NOT NULL,
    PromoTypeID INT NOT NULL,
    DocumentNumber VARCHAR(100) NOT NULL, -- ID number
    Status ENUM('Pending', 'Approved', 'Rejected') NOT NULL DEFAULT 'Pending',
    ValidatedBy INT, -- Admin UserID who validated
    ValidationDate TIMESTAMP NULL,
    ExpiryDate DATE NULL, -- For IDs that expire
    Notes VARCHAR(255),
    CreatedAt TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    FOREIGN KEY (UserID) REFERENCES Users(UserID) ON DELETE CASCADE,
    FOREIGN KEY (PromoTypeID) REFERENCES PromoTypes(PromoTypeID) ON DELETE CASCADE,
    FOREIGN KEY (ValidatedBy) REFERENCES Users(UserID) ON DELETE SET NULL,
    
    -- One promo type per user
    UNIQUE KEY unique_user_promo (UserID, PromoTypeID),
    
    -- Indexes
    INDEX idx_user_id (UserID),
    INDEX idx_promo_type (PromoTypeID),
    INDEX idx_status (Status)
);


-- ================================
-- BOOKING TABLES (NO STORED TOTALS)
-- ================================

-- Main booking information (core booking data only)
CREATE TABLE Bookings (
    BookingID INT PRIMARY KEY AUTO_INCREMENT,
    BookingReference VARCHAR(20) NOT NULL UNIQUE,
    UserID INT NOT NULL,
    PackageID INT NOT NULL,
    NumberOfPeople INT NOT NULL DEFAULT 1,
    PromoID INT NULL, -- Reference to applied promo
    TravelDate DATE NOT NULL,
    EndDate DATE NOT NULL,
    BookingDate TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    Status VARCHAR(20) NOT NULL DEFAULT 'Pending', -- 'Pending', 'Confirmed', 'Cancelled', 'Completed'
    Notes VARCHAR(500),
    CreatedAt TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (UserID) REFERENCES Users(UserID) ON DELETE CASCADE,
    FOREIGN KEY (PackageID) REFERENCES TourPackages(PackageID) ON DELETE CASCADE,
    FOREIGN KEY (PromoID) REFERENCES PromoDiscounts(PromoID) ON DELETE SET NULL,
    
    -- Indexes for performance
    INDEX idx_user_id (UserID),
    INDEX idx_package_id (PackageID),
    INDEX idx_travel_date (TravelDate),
    INDEX idx_status (Status)
);

CREATE TABLE BookingPayments (
    BookingID INT PRIMARY KEY,
    PaymentMethod VARCHAR(20) NOT NULL DEFAULT 'Pay on Trip',
    PaymentStatus VARCHAR(20) NOT NULL DEFAULT 'Pending', -- 'Pending', 'Paid', 'Cancelled'
    PaymentDate TIMESTAMP NULL,
    TransactionReference VARCHAR(50) NULL,
    CreatedAt TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (BookingID) REFERENCES Bookings(BookingID) ON DELETE CASCADE
);

-- Store only base data, calculate totals in application
CREATE TABLE BookingAddOns (
    BookingAddOnID INT PRIMARY KEY AUTO_INCREMENT,
    BookingID INT NOT NULL,
    AddOnID INT NOT NULL,
    Quantity INT NOT NULL DEFAULT 1,
    UnitPrice DECIMAL(10,2) NOT NULL, -- Store unit price at time of booking for historical accuracy
    CreatedAt TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (BookingID) REFERENCES Bookings(BookingID) ON DELETE CASCADE,
    FOREIGN KEY (AddOnID) REFERENCES PackageAddOns(AddOnID) ON DELETE CASCADE,
    
    -- Indexes
    INDEX idx_booking_id (BookingID),
    INDEX idx_addon_id (AddOnID)
);


-- ================================
-- BOOKING HISTORY (2NF COMPLIANT)
-- ================================

CREATE TABLE BookingHistory (
    HistoryID INT PRIMARY KEY AUTO_INCREMENT,
    BookingID INT NOT NULL,
    UserID INT NOT NULL,
    ActionType VARCHAR(50) NOT NULL, -- 'Created', 'Modified', 'Cancelled', 'Completed', 'Paid'
    ActionDate TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    ActionDescription TEXT,
    PreviousStatus VARCHAR(20),
    NewStatus VARCHAR(20),
    ModifiedBy INT, -- UserID of who made the change (admin/user)
    CreatedAt TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (BookingID) REFERENCES Bookings(BookingID) ON DELETE CASCADE,
    FOREIGN KEY (UserID) REFERENCES Users(UserID) ON DELETE CASCADE,
    FOREIGN KEY (ModifiedBy) REFERENCES Users(UserID) ON DELETE SET NULL
);

CREATE TABLE IF NOT EXISTS BookingPackages (
    BookingPackageID INT AUTO_INCREMENT PRIMARY KEY,
    BookingID INT NOT NULL,
    PackageID INT NOT NULL,
    TravelDate DATE NOT NULL,
    EndDate DATE NOT NULL,
    NumberOfPeople INT NOT NULL,
    UnitPrice DECIMAL(10,2) NOT NULL,
    FOREIGN KEY (BookingID) REFERENCES Bookings(BookingID),
    FOREIGN KEY (PackageID) REFERENCES TourPackages(PackageID)
);

-- ================================
-- REVIEW TABLE
-- ================================

CREATE TABLE Reviews (
    ReviewID INT PRIMARY KEY AUTO_INCREMENT,
    UserID INT NOT NULL,
    BookingID INT NULL, -- Optional: link review to specific booking
    PackageID INT NULL,
    Rating INT NOT NULL CHECK (Rating BETWEEN 1 AND 5),
    Comment TEXT NULL,
    ReviewDate TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CreatedAt TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    FOREIGN KEY (UserID) REFERENCES Users(UserID) ON DELETE CASCADE,
    FOREIGN KEY (BookingID) REFERENCES Bookings(BookingID) ON DELETE SET NULL,
    FOREIGN KEY (PackageID) REFERENCES TourPackages(PackageID) ON DELETE SET NULL,
    
    -- Indexes for better performance
    INDEX idx_user_id (UserID),
    INDEX idx_booking_id (BookingID),
    INDEX idx_rating (Rating),
    INDEX idx_review_date (ReviewDate)
);


-- ================================
-- FINANCIAL TABLES (NO STORED TOTALS)
-- ================================

CREATE TABLE ExpenseCategories (
    ExpenseCategoryID INT AUTO_INCREMENT PRIMARY KEY,
    Category VARCHAR(100) NOT NULL UNIQUE,
    CategoryType ENUM('Startup', 'Operating') NOT NULL,
    IsActive BOOLEAN NOT NULL DEFAULT 1,
    CreatedAt TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
);

CREATE TABLE Expenses (
    ExpenseID INT AUTO_INCREMENT PRIMARY KEY,
    ExpenseItem VARCHAR(100) NOT NULL,
    ExpenseType ENUM('Startup', 'Operating') NOT NULL,
    ExpenseCategoryID INT NOT NULL,
    BookingID INT NULL,
    Amount DECIMAL(12,2) NOT NULL,
    ExpenseDate DATE NOT NULL,
    PaymentMethod VARCHAR(50),
    ReceiptNumber VARCHAR(100),
    Description VARCHAR(255),
    RecordedBy INT,
    RecordedAt TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    
    FOREIGN KEY (ExpenseCategoryID) REFERENCES ExpenseCategories(ExpenseCategoryID) ON DELETE CASCADE,
    FOREIGN KEY (BookingID) REFERENCES Bookings(BookingID) ON DELETE SET NULL,
    FOREIGN KEY (RecordedBy) REFERENCES Users(UserID) ON DELETE SET NULL
);

CREATE TABLE IncomeSources (
    IncomeSourceID INT AUTO_INCREMENT PRIMARY KEY,
    Category VARCHAR(100) NOT NULL,
    CommissionRate VARCHAR(20),
    MonthlyMin DECIMAL(12,2),
    MonthlyMax DECIMAL(12,2),
    AnnualMin DECIMAL(12,2),
    AnnualMax DECIMAL(12,2),
    IsActive BOOLEAN NOT NULL DEFAULT 1,
    CreatedAt TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
);

-- REMOVED: Amount (will be calculated from GrossAmount - CommissionAmount)
-- REMOVED: NetAmount (will be calculated in application)
CREATE TABLE Income (
    IncomeID INT AUTO_INCREMENT PRIMARY KEY,
    IncomeItem VARCHAR(100) NOT NULL,
    IncomeType ENUM('Operating') NOT NULL,
    IncomeSourceID INT NOT NULL,
    BookingID INT NULL,
    IncomeDate DATE NOT NULL,
    Description VARCHAR(255) NULL,
    RecordedBy INT,
    RecordedAt TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,

    FOREIGN KEY (BookingID) REFERENCES Bookings(BookingID) ON DELETE SET NULL,
    FOREIGN KEY (IncomeSourceID) REFERENCES IncomeSources(IncomeSourceID) ON DELETE CASCADE,
    FOREIGN KEY (RecordedBy) REFERENCES Users(UserID) ON DELETE SET NULL
);


-- ================================
-- CALCULATION VIEWS FOR DYNAMIC TOTALS
-- ================================

-- View to calculate booking amounts dynamically
CREATE OR REPLACE VIEW BookingAmounts AS
SELECT 
    b.BookingID,
    b.BookingReference,
    b.UserID,
    b.PackageID,
    b.NumberOfPeople,
    b.PromoID,
    
    -- Base package amount (Package price * number of people)
    (tp.Price * b.NumberOfPeople) AS BaseAmount,
    
    -- Add-ons total (calculated from BookingAddOns)
    COALESCE(addon_totals.AddOnTotal, 0) AS AddOnTotal,
    
    -- Subtotal before discount
    ((tp.Price * b.NumberOfPeople) + COALESCE(addon_totals.AddOnTotal, 0)) AS SubTotal,
    
    -- Discount calculation
    CASE 
        WHEN b.PromoID IS NOT NULL AND pd.Status = 'Approved' THEN
            ROUND(((tp.Price * b.NumberOfPeople) + COALESCE(addon_totals.AddOnTotal, 0)) * (pt.DiscountPercentage / 100), 2)
        ELSE 0
    END AS DiscountAmount,
    
    -- Final amount after discount
    ((tp.Price * b.NumberOfPeople) + COALESCE(addon_totals.AddOnTotal, 0)) - 
    CASE 
        WHEN b.PromoID IS NOT NULL AND pd.Status = 'Approved' THEN
            ROUND(((tp.Price * b.NumberOfPeople) + COALESCE(addon_totals.AddOnTotal, 0)) * (pt.DiscountPercentage / 100), 2)
        ELSE 0
    END AS FinalAmount,
    
    -- Promo details
    pt.PromoTypeName,
    pt.DiscountPercentage

FROM Bookings b
JOIN TourPackages tp ON b.PackageID = tp.PackageID
LEFT JOIN PromoDiscounts pd ON b.PromoID = pd.PromoID
LEFT JOIN PromoTypes pt ON pd.PromoTypeID = pt.PromoTypeID
LEFT JOIN (
    SELECT 
        BookingID,
        SUM(UnitPrice * Quantity) AS AddOnTotal
    FROM BookingAddOns
    GROUP BY BookingID
) addon_totals ON b.BookingID = addon_totals.BookingID;

-- View to calculate package availability dynamically
CREATE OR REPLACE VIEW PackageAvailability AS
SELECT 
    tp.PackageID,
    tp.Title,
    tp.PackageType,
    tp.MaxSlots,
    COALESCE(booked_slots.BookedSlots, 0) AS BookedSlots,
    (tp.MaxSlots - COALESCE(booked_slots.BookedSlots, 0)) AS AvailableSlots,
    tp.StartDate,
    tp.EndDate,
    tp.Status,
    tp.IsActive
FROM TourPackages tp
LEFT JOIN (
    SELECT 
        PackageID,
        SUM(NumberOfPeople) AS BookedSlots
    FROM Bookings
    WHERE Status IN ('Pending', 'Confirmed')
    GROUP BY PackageID
) booked_slots ON tp.PackageID = booked_slots.PackageID;

-- View to calculate income amounts dynamically
CREATE OR REPLACE VIEW IncomeAmounts AS
SELECT 
    IncomeID,
    IncomeItem,
    IncomeType,
    IncomeSourceID,
    BookingID,
    IncomeDate,
    Description,
    RecordedBy,
    RecordedAt
FROM Income;

-- View for recent transactions with calculated amounts

CREATE OR REPLACE VIEW RecentTransactions AS
SELECT
    'Income' AS Type,
    i.Description,
    ba.FinalAmount AS Amount,
    i.IncomeDate AS Date,
    bp.PaymentMethod,
    i.RecordedBy AS UserID,
    i.BookingID,
    tp.PackageType AS PackageCategory,
    b.BookingDate,
    NULL AS ExpenseItem,
    NULL AS ExpenseCategory,
    NULL AS ReceiptNumber,
    NULL AS AdminName,
    1 AS IsIncome
FROM Income i
LEFT JOIN Bookings b ON i.BookingID = b.BookingID
LEFT JOIN BookingPayments bp ON i.BookingID = bp.BookingID
LEFT JOIN TourPackages tp ON b.PackageID = tp.PackageID
LEFT JOIN BookingAmounts ba ON b.BookingID = ba.BookingID

UNION ALL

SELECT
    'Expense' AS Type,
    e.Description,
    e.Amount,
    e.ExpenseDate AS Date,
    e.PaymentMethod,
    NULL AS UserID,
    e.BookingID,
    NULL AS PackageCategory,
    NULL AS BookingDate,
    e.ExpenseItem,
    ec.Category AS ExpenseCategory,
    e.ReceiptNumber,
    CONCAT(u.FirstName, ' ', u.LastName) AS AdminName,
    0 AS IsIncome
FROM Expenses e
LEFT JOIN ExpenseCategories ec ON e.ExpenseCategoryID = ec.ExpenseCategoryID
LEFT JOIN Users u ON e.RecordedBy = u.UserID
ORDER BY Date DESC
LIMIT 20;

-- Complete booking information view with calculated amounts
CREATE OR REPLACE VIEW BookingCompleteView AS
SELECT 
    b.BookingID,
    b.BookingReference,
    b.UserID,
    u.FirstName,
    u.LastName,
    u.Email,
    u.Phone,
    b.BookingDate,
    b.TravelDate,
    b.EndDate,
    b.NumberOfPeople,
    b.Status,
    bp.PaymentMethod,
    bp.PaymentStatus,
    bp.PaymentDate,
    bp.TransactionReference,
    
    -- Calculated amounts from BookingAmounts view
    ba.BaseAmount,
    ba.AddOnTotal,
    ba.SubTotal,
    ba.DiscountAmount,
    ba.FinalAmount,
    ba.PromoTypeName,
    ba.DiscountPercentage,
    
    tp.PackageID,
    tp.Title AS PackageTitle,
    pl.Location AS PackageLocation,
    tp.PackageType AS PackageType

FROM Bookings b
JOIN Users u ON b.UserID = u.UserID
JOIN TourPackages tp ON b.PackageID = tp.PackageID
JOIN PackageLocations pl ON tp.PackageID = pl.PackageID
LEFT JOIN BookingPayments bp ON b.BookingID = bp.BookingID
LEFT JOIN BookingAmounts ba ON b.BookingID = ba.BookingID;

-- Booking with reviews view
CREATE OR REPLACE VIEW BookingWithReviewsView AS
SELECT 
    b.BookingID,
    b.BookingReference,
    b.UserID,
    CONCAT(u.FirstName, ' ', u.LastName) AS CustomerName,
    u.FirstName,
    u.LastName,
    u.Email,
    u.Phone,
    b.BookingDate,
    b.TravelDate,
    b.EndDate,
    DATEDIFF(b.EndDate, b.TravelDate) AS TripDuration,
    b.NumberOfPeople,
    b.Status,
    
    -- Calculated amounts
    ba.FinalAmount,
    ba.BaseAmount,
    ba.AddOnTotal,
    ba.DiscountAmount,

    tp.PackageID,
    tp.Title AS PackageTitle,
    tp.PackageType AS PackageType,
    pl.Location AS PackageLocation,

    bp.PaymentMethod,
    bp.PaymentStatus,
    bp.PaymentDate,
    bp.TransactionReference,

    r.Rating

FROM Bookings b
JOIN Users u ON b.UserID = u.UserID
JOIN TourPackages tp ON b.PackageID = tp.PackageID
JOIN PackageLocations pl ON tp.PackageID = pl.PackageID
LEFT JOIN BookingPayments bp ON b.BookingID = bp.BookingID
LEFT JOIN BookingAmounts ba ON b.BookingID = ba.BookingID
LEFT JOIN Reviews r ON b.BookingID = r.BookingID;

-- Review statistics view
CREATE OR REPLACE VIEW ReviewStatistics AS
SELECT 
    COUNT(*) as TotalReviews,
    ROUND(AVG(Rating), 2) as AverageRating,
    COUNT(CASE WHEN Rating = 5 THEN 1 END) as FiveStarCount,
    COUNT(CASE WHEN Rating = 4 THEN 1 END) as FourStarCount,
    COUNT(CASE WHEN Rating = 3 THEN 1 END) as ThreeStarCount,
    COUNT(CASE WHEN Rating = 2 THEN 1 END) as TwoStarCount,
    COUNT(CASE WHEN Rating = 1 THEN 1 END) as OneStarCount,
    COUNT(CASE WHEN Comment IS NOT NULL AND Comment != '' THEN 1 END) as ReviewsWithComments,
    COUNT(CASE WHEN Comment IS NULL OR Comment = '' THEN 1 END) as ReviewsWithoutComments,
    ROUND((COUNT(CASE WHEN Rating = 5 THEN 1 END) * 100.0 / COUNT(*)), 2) as FiveStarPercentage,
    ROUND((COUNT(CASE WHEN Rating = 4 THEN 1 END) * 100.0 / COUNT(*)), 2) as FourStarPercentage,
    ROUND((COUNT(CASE WHEN Rating = 3 THEN 1 END) * 100.0 / COUNT(*)), 2) as ThreeStarPercentage,
    ROUND((COUNT(CASE WHEN Rating = 2 THEN 1 END) * 100.0 / COUNT(*)), 2) as TwoStarPercentage,
    ROUND((COUNT(CASE WHEN Rating = 1 THEN 1 END) * 100.0 / COUNT(*)), 2) as OneStarPercentage
FROM Reviews;


-- ================================
-- SAMPLE QUERIES FOR CALCULATING AMOUNTS
-- ================================

-- Get booking total for a specific booking
-- SELECT FinalAmount FROM BookingAmounts WHERE BookingID = ?;

-- Get available slots for a package
-- SELECT AvailableSlots FROM PackageAvailability WHERE PackageID = ?;

-- Get net income amount
-- SELECT NetAmount FROM IncomeAmounts WHERE IncomeID = ?;

-- Get add-ons total for a booking
-- SELECT AddOnTotal FROM BookingAmounts WHERE BookingID = ?;


-- ================================
-- INITIAL DATA INSERTS
-- ================================

-- Insert admin user (same as before)
INSERT INTO Users (Username, Email, Password, FirstName, LastName, Role)
VALUES ('admin_ken', 'adminken@email.com', 'lphttken', 'Ken', 'Harold', 'Admin');

-- Insert tour categories (same as before)
INSERT INTO TourCategories (CategoryName, CategoryType) VALUES
('Basic Training', 'Freediving'),
('Certification Courses', 'Freediving'),
('Adventure Packages', 'Freediving'),
('Specialty Experiences', 'Freediving'),
('Island Paradise', 'Domestic'),
('Cultural Heritage', 'Domestic'),
('Nature & Adventure', 'Domestic'),
('Beach & Diving', 'Domestic'),
('Asian Discovery', 'International'),
('Western Culture', 'International'),
('Luxury Destinations', 'International'),
('Adventure Expeditions', 'International');

-- Insert tour packages (using MaxSlots instead of TotalSlots/AvailableSlots)
INSERT INTO TourPackages (Title, PackageType, Price, Description, Duration, DurationNights, CategoryID, MaxSlots, StartDate, EndDate, IsGroupPackage, GroupSize) VALUES
-- Basic Training Packages
('Basic Day Tour Dive', 'Freediving', 2500.00, 'Perfect for first-timers and those short on time!', 1, 0, 1, 15, '2025-08-01', '2025-12-31', 1, 6),
('Sunset Dive Experience', 'Freediving', 2800.00, 'A romantic and peaceful dive at golden hour.', 1, 0, 1, 12, '2025-08-01', '2025-12-31', 1, 6),
('Night Glow Dive Adventure', 'Freediving', 3200.00, 'Witness bioluminescent plankton in a serene night dive.', 1, 0, 4, 10, '2025-08-01', '2025-12-31', 1, 6),

-- Multi-day Training Packages
('2 Days 1 Night Open Water Package', 'Freediving', 6500.00, 'For those looking to go a bit deeper into the diving experience!', 2, 1, 1, 20, '2025-08-01', '2025-12-31', 1, 10),
('Adventure Dive & Island Hopping Package', 'Freediving', 7500.00, 'Combine a diving adventure with a scenic island escape!', 2, 1, 3, 16, '2025-08-01', '2025-12-31', 1, 6),
('Weekend Warrior Dive', 'Freediving', 7000.00, 'Maximize your weekend with action-packed dives!', 2, 1, 3, 14, '2025-08-01', '2025-12-31', 1, 6),

-- Certification Courses
('Freediving Training & Certification', 'Freediving', 10000.00, 'Ideal for divers looking to become certified freedivers!', 3, 2, 2, 12, '2025-08-01', '2025-12-31', 1, 6),
('Master Diver Elite Course', 'Freediving', 18000.00, 'Push your limits with elite training and mentorship.', 5, 4, 2, 8, '2025-08-01', '2025-12-31', 1, 4),

-- Specialty Experiences
('Family & Friends Ocean Retreat Package', 'Freediving', 12000.00, 'The perfect package for groups to enjoy relaxed diving and island getaway!', 3, 2, 4, 20, '2025-08-01', '2025-12-31', 1, 12),
('Freediving & Yoga Wellness Retreat', 'Freediving', 10500.00, 'Balance breath, body, and sea in this holistic retreat.', 3, 2, 4, 15, '2025-08-01', '2025-12-31', 1, 6),
('Underwater Photography Dive', 'Freediving', 8500.00, 'Capture marine beauty through your lens.', 2, 1, 4, 10, '2025-08-01', '2025-12-31', 1, 6),
('Couples Dive & Dine Getaway', 'Freediving', 9500.00, 'Dive together and enjoy a beachfront dinner under the stars.', 2, 1, 4, 8, '2025-08-01', '2025-12-31', 1, 2),

-- Domestic Packages
('Palawan Paradise Explorer', 'Domestic', 28000.00, 'Discover the pristine beauty of Palawan''s world-class beaches!', 6, 5, 5, 25, '2025-08-01', '2025-12-31', 1, 15),
('Boracay Beach Bliss', 'Domestic', 22000.00, 'Experience the famous white sand beaches of Boracay.', 4, 3, 5, 30, '2025-08-01', '2025-12-31', 1, 20),
('Siargao Surf & Island Life', 'Domestic', 25000.00, 'Surf, relax, and enjoy island life in Siargao.', 5, 4, 5, 20, '2025-08-01', '2025-12-31', 1, 12),
('Coron Shipwreck Dive Adventure', 'Domestic', 27000.00, 'Explore WWII shipwrecks in Coron''s crystal clear waters.', 5, 4, 8, 16, '2025-08-01', '2025-12-31', 1, 15),
('Camiguin Volcano & Waterfall Escape', 'Domestic', 16500.00, 'Discover the natural wonders of Camiguin Island.', 4, 3, 7, 18, '2025-08-01', '2025-12-31', 1, 12),
('Zambales Island Hopping Getaway', 'Domestic', 13500.00, 'Enjoy secluded beaches and island hopping in Zambales.', 3, 2, 5, 24, '2025-08-01', '2025-12-31', 1, 15),
('Bohol Heritage Adventure', 'Domestic', 18500.00, 'Explore the cultural and natural wonders of Bohol.', 4, 3, 6, 25, '2025-08-01', '2025-12-31', 1, 20),
('Banaue Rice Terraces Cultural Journey', 'Domestic', 19500.00, 'Witness the breathtaking rice terraces of Banaue.', 5, 4, 6, 20, '2025-08-01', '2025-12-31', 1, 15),
('Cebu Heritage & Island Discovery', 'Domestic', 24500.00, 'Experience the best of Cebu''s heritage and islands.', 6, 5, 6, 22, '2025-08-01', '2025-12-31', 1, 18),
('Ilocos Historic Road Trip', 'Domestic', 18500.00, 'Travel through history in the Ilocos region.', 5, 4, 6, 20, '2025-08-01', '2025-12-31', 1, 15),
('Davao Eco Adventure Tour', 'Domestic', 21000.00, 'Experience Davao''s natural wonders and wildlife.', 5, 4, 7, 18, '2025-08-01', '2025-12-31', 1, 12),
('Bicol Mayon & Whale Shark Expedition', 'Domestic', 23000.00, 'See Mayon Volcano and swim with whale sharks.', 5, 4, 7, 16, '2025-08-01', '2025-12-31', 1, 10),

-- International Packages
('Japan Cultural Discovery', 'International', 85000.00, 'Experience the rich culture and traditions of Japan.', 7, 6, 9, 20, '2025-09-01', '2025-12-31', 1, 15),
('Thailand Beach Paradise', 'International', 45000.00, 'Relax on Thailand''s beautiful beaches and islands.', 6, 5, 9, 25, '2025-08-01', '2025-12-31', 1, 20),
('Singapore City Explorer', 'International', 38000.00, 'Discover the modern marvels of Singapore.', 4, 3, 9, 20, '2025-08-01', '2025-12-31', 1, 15),
('South Korea K-Culture Tour', 'International', 65000.00, 'Immerse yourself in Korean culture and pop culture.', 6, 5, 9, 18, '2025-08-01', '2025-12-31', 1, 15),
('Vietnam Heritage Discovery', 'International', 52000.00, 'Explore Vietnam''s rich history and natural beauty.', 8, 7, 9, 22, '2025-08-01', '2025-12-31', 1, 18),
('Dubai Luxury Experience', 'International', 95000.00, 'Experience the luxury and opulence of Dubai.', 5, 4, 11, 12, '2025-08-01', '2025-12-31', 1, 10),
('Paris Romance & Culture', 'International', 78000.00, 'Fall in love with the romance of Paris.', 6, 5, 10, 15, '2025-09-01', '2025-12-31', 1, 12),
('New York Big Apple Experience', 'International', 125000.00, 'Experience the energy of New York City.', 6, 5, 10, 12, '2025-09-01', '2025-12-31', 1, 10),
('London Royal Heritage Tour', 'International', 88000.00, 'Discover the royal history of London.', 7, 6, 10, 15, '2025-09-01', '2025-12-31', 1, 12),
('Italy Grand Tour', 'International', 99000.00, 'Experience the art, history, and cuisine of Italy.', 9, 8, 10, 18, '2025-09-01', '2025-12-31', 1, 15),
('Sydney Harbor Adventure', 'International', 105000.00, 'Explore the iconic sights of Sydney.', 7, 6, 12, 15, '2025-08-01', '2025-12-31', 1, 12),
('Canada Rockies Expedition', 'International', 112000.00, 'Discover the breathtaking Canadian Rockies.', 7, 6, 12, 12, '2025-08-01', '2025-12-31', 1, 10);

-- Insert package locations
INSERT INTO PackageLocations (PackageID, Location) VALUES
(1, 'DOT Accredited Resort'),
(2, 'Batangas Sunset Bay'),
(3, 'Secret Bay, Batangas'),
(4, 'Freediving Resort'),
(5, 'Mabini Diving Sites'),
(6, 'Puerto Galera'),
(7, 'Certified Training Center'),
(8, 'Advanced Dive Camp'),
(9, 'Family-Friendly Resort'),
(10, 'Mindoro Wellness Resort'),
(11, 'Anilao Marine Sanctuary'),
(12, 'Private Resort, Batangas'),
(13, 'El Nido, Coron, Puerto Princesa'),
(14, 'Boracay Island, Aklan'),
(15, 'General Luna, Cloud 9, Magpupungko'),
(16, 'Coron, Palawan'),
(17, 'Camiguin Island'),
(18, 'Subic, Anawangin, Nagsasa, Capones'),
(19, 'Tagbilaran, Panglao, Chocolate Hills'),
(20, 'Banaue, Batad, Sagada, Baguio'),
(21, 'Cebu City, Oslob, Siquijor, Moalboal'),
(22, 'Vigan, Laoag, Pagudpud'),
(23, 'Davao City, Samal Island, Eden Park'),
(24, 'Legazpi, Donsol, Cagsawa Ruins'),
(25, 'Tokyo, Kyoto, Osaka'),
(26, 'Bangkok, Phuket, Phi Phi Islands'),
(27, 'Singapore'),
(28, 'Seoul, Busan'),
(29, 'Hanoi, Ha Long Bay, Ho Chi Minh'),
(30, 'Dubai, Abu Dhabi'),
(31, 'Paris, France'),
(32, 'New York, USA'),
(33, 'London, UK'),
(34, 'Rome, Florence, Venice'),
(35, 'Sydney, Australia'),
(36, 'Banff, Jasper, Lake Louise');

-- Insert package details
INSERT INTO PackageDetails (PackageID, PackageType, FlagSymbol, IdealFor, Inclusions, Highlights, ImageFileName) VALUES
-- Freediving Packages
(1, 'Freediving', '🏊‍♀️', 'Beginners and non-divers', 
'Day Tour Class (Open Water) training, 1 Fun Dive session with certified instructors, Access to DOT ACCREDITED Accommodation (Private AC Room), Access to resort amenities (lounge areas, showers), Light breakfast and local lunch',
'Perfect for first-timers and those short on time!', 'fdbasic.jpg'),

(2, 'Freediving', '🌅', 'Couples and sunset lovers', 
'Sunset Fun Dive session with certified instructors, Resort access with sunset viewing deck, Welcome drinks and light snacks, Photo souvenir and underwater shots, Optional romantic dinner package',
'A romantic and peaceful dive at golden hour.', 'fdsunset.jpg'),

(3, 'Freediving', '🌌', 'Adventurers and photographers', 
'Night Fun Dive with underwater light guides, Instructor-led safety orientation, Night snacks and hot drinks, Photo and video coverage, Resort amenities (lounge, rinse area)',
'Witness bioluminescent plankton in a serene night dive.', 'fdnight.jpg'),

(4, 'Freediving', '🤿', 'Beginners who want to explore more or divers seeking intensive experience', 
'2 Days 1 Night Class (Open Water) certification, 2 Fun Dive sessions with certified instructors, 1 Boat Dive experience with certified instructors, Island Hopping tour, Cabin Room Accommodation (Fan Room), All meals included, Access to resort amenities',
'For those looking to go a bit deeper into the diving experience!', 'fd2days.jpg'),

(5, 'Freediving', '🏝️', 'Intermediate divers looking for mix of adventure and relaxation', 
'1 Fun Dive session with certified instructors, 2 Boat Dive experiences with certified instructors, Island Hopping tour, Access to City Pool Dive for skill practice, DOT ACCREDITED Accommodation (Private AC Room), All meals included',
'Combine a diving adventure with a scenic island escape!', 'fdisland.jpg'),

(6, 'Freediving', '⚔️', 'Weekend travelers and active divers', 
'2 Boat Dive sessions at top Puerto Galera sites, 1 Night Dive experience, Cabin Room Accommodation (Fan), All meals included, Speedboat transportation',
'Maximize your weekend with action-packed dives!', 'fdwarrior.jpg'),

(7, 'Freediving', '🏆', 'Aspiring freedivers or those looking to advance freediving skills', 
'Molchanov''s Certification Course (Wave 1 or Wave 2), Basic and Advanced Line Training, Pool training and skill sessions at City Pool Dive, Island Hopping activity on rest days, DOT ACCREDITED Accommodation (Private AC Room), All meals and training materials included',
'Ideal for divers looking to become certified freedivers!', 'fdtrain.jpg'),

(8, 'Freediving', '🎓', 'Experienced divers aiming to go pro', 
'Advanced depth and static apnea training, Instructor mentorship and evaluations, Night dive and rescue scenarios, Training logbooks and certification, Camp-style shared accommodation',
'Push your limits with elite training and mentorship.', 'fdmaster.jpg'),

(9, 'Freediving', '👨‍👩‍👧‍👦', 'Families and friends seeking memorable experience together', 
'Choice of Fun Dive or Mermaid Tail Dive per person, Pool Training session for beginners, Boat Dive to top dive sites in Mabini, Island Hopping tour, DOT ACCREDITED Accommodation (AC or Cabin Room), All meals included, Mermaid tails provided if selected',
'The perfect package for groups to enjoy relaxed diving and island getaway!', 'fdfam.jpg'),

(10, 'Freediving', '🧘‍♀️', 'Relaxation seekers and wellness enthusiasts', 
'Daily yoga and breathwork sessions, Pool and line training dives, Beachside meditation and bonfire, Vegetarian wellness meals, Beachfront hut accommodation',
'Balance breath, body, and sea in this holistic retreat.', 'fdyoga.jpg'),

(11, 'Freediving', '📸', 'Photography enthusiasts and creative divers', 
'2 Guided underwater photo dives, Camera gear rental (optional), Photography workshop session, Accommodation with editing station access, Meal plan and island tour',
'Capture marine beauty through your lens.', 'fdphoto.jpg'),

(12, 'Freediving', '💑', 'Couples looking for romance and adventure', 
'Couples dive session with instructor, Private beachfront dinner setup, 1 Night AC Room Accommodation, Breakfast and sunset cocktails, Photo souvenir and dive certificate',
'Dive together and enjoy a beachfront dinner under the stars.', 'fdcouples.jpg'),

-- Domestic Packages
(13, 'Domestic', '🏝️', 'Beach lovers and adventure seekers', 
'Round-trip flights Manila-Puerto Princesa-Manila, 5 nights accommodation (2 nights Puerto Princesa, 3 nights El Nido), Daily breakfast and 3 seafood dinners, Airport transfers and land transportation, Underground River tour with permits, El Nido island hopping tours A & C, Snorkeling equipment and boat transfers, Environmental and entrance fees',
'UNESCO World Heritage Underground River, Big Lagoon and Small Lagoon in El Nido, Seven Commando Beach and Helicopter Island, Pristine white sand beaches, Crystal clear turquoise waters', 'dompalawan.jpg'),

(14, 'Domestic', '🏖️', 'Party-goers and beach enthusiasts', 
'Round-trip flights Manila-Kalibo-Manila, 3 nights accommodation beachfront hotel, Daily breakfast and sunset dinner cruise, Airport transfers and boat transfers to island, Island hopping with snorkeling, Sunset sailing and paraw boat experience, ATV adventure and helmet zorbing, White Beach access and activities',
'4-kilometer stretch of powdery White Beach, Vibrant nightlife and beach parties, Water sports: parasailing, jet skiing, diving, Stunning Boracay sunsets, Fresh seafood and tropical dining', 'domboracay.png'),

(15, 'Domestic', '🏄‍♂️', 'Surfers and island hoppers', 
'Round-trip flights Manila-Sayak Airport-Manila, 4 nights accommodation in General Luna, Daily breakfast and 2 local dinners, Airport transfers and motorcycle rentals, Surf lessons at Cloud 9 with board rental, Island hopping to Naked, Daku, and Guyam Islands, Magpupungko Rock Pools natural jacuzzi tour, Coconut and palm tree climbing experience',
'World-renowned Cloud 9 surfing break, Crystal clear Magpupungko Rock Pools, Pristine uninhabited islands, Laid-back island lifestyle, Fresh coconuts and tropical island vibes', 'domsiargao.jpg'),

(16, 'Domestic', '⚓', 'Scuba divers and underwater adventurers', 
'Round-trip flights Manila-Busuanga-Manila, 4 nights dive resort accommodation, Daily breakfast and 3 dive days, 2 shipwreck dives + 1 coral garden dive per day, All dive gear rental included, Island hopping: Kayangan Lake, Twin Lagoon, BBQ lunch on beach picnic',
'WWII Japanese shipwreck dives, Clear waters of Kayangan Lake, Stunning coral gardens, Thermal hot spring soaking, Dramatic limestone cliffs and lagoons', 'domcoron.jpg'),

(17, 'Domestic', '🌋', 'Nature lovers and outdoor explorers', 
'Round-trip flights Manila-Cagayan de Oro-Manila, 3 nights accommodation in Camiguin eco-lodge, Daily breakfast and welcome dinner, Ferry transfer to Camiguin, White Island sandbar tour, Tuasan and Katibawasan Falls trek, Sunken Cemetery snorkeling, Ardent Hot Spring and Giant Clam Sanctuary',
'Volcanic hot springs and waterfalls, Crystal clear waters and marine life, Historical Sunken Cemetery dive site, Charming small-island vibe, Majestic Mt. Hibok-Hibok', 'domcamiguin.jpg'),

(18, 'Domestic', '🏕️', 'Quick getaways and campers', 
'Private van transport from Manila (round-trip), Boat transfers to Anawangin, Nagsasa, and Capones Islands, 2 nights tent accommodation with camp gear, All meals during the tour (6 total), Bonfire night and stargazing activity, Entrance and environmental fees, Local guide and safety orientation',
'Secluded white sand coves, Camping by the beach with bonfires, Trekking and photo ops at Capones Lighthouse, Lush pine tree surroundings, Unplugged escape from the city', 'domzambales.jpg'),

(19, 'Domestic', '🌄', 'Families and nature enthusiasts', 
'Round-trip flights Manila-Tagbilaran-Manila, 3 nights accommodation in Panglao Island resort, Daily breakfast and welcome dinner, Airport transfers and tour transportation, Chocolate Hills viewing and Carmen tour, Tarsier Sanctuary visit, Loboc River cruise with lunch, Baclayon Church and Blood Compact Monument',
'1,776 geological formations of Chocolate Hills, World''s smallest primate - Philippine Tarsier, Scenic Loboc River floating restaurant, Beautiful Alona Beach in Panglao, Historic Baclayon Stone Church', 'dombohol.png'),

(20, 'Domestic', '🌾', 'Culture seekers and hikers', 
'Round-trip bus Manila-Baguio with sleeper accommodation, 4 nights accommodation (2 nights Banaue, 2 nights Baguio), Daily breakfast and traditional Cordillera meals, Land transfers and jeepney rides to viewpoints, Guided trek to Batad Rice Terraces, Sagada Hanging Coffins and Echo Valley tour, Baguio city tour: Burnham Park, Session Road, Strawberry picking in La Trinidad',
'UNESCO World Heritage Rice Terraces, 2000-year-old Ifugao engineering marvel, Mystical hanging coffins of Sagada, Cool mountain climate and pine forests, Indigenous Cordillera culture and traditions', 'dombanaue.jpg'),

(21, 'Domestic', '🐋', 'History buffs and marine life enthusiasts', 
'Round-trip flights Manila-Cebu-Manila, 5 nights accommodation (2 nights Cebu City, 3 nights southern Cebu), Daily breakfast and heritage dinners, Airport transfers and southern Cebu van transfers, Whale shark swimming experience in Oslob, Cebu Heritage Monument and Magellan''s Cross tour, Siquijor mystical island day trip, Moalboal sardine run and turtle watching',
'Swimming with gentle whale sharks, Birthplace of Christianity in Asia, Historic Fort San Pedro and Basilica del Santo Niño, Millions of sardines underwater tornado, Sea turtle encounters in Moalboal', 'domcebu.jpg'),

(22, 'Domestic', '🏛️', 'History lovers and road trippers', 
'Private van transport from Manila (round-trip), 4 nights accommodation (1 Vigan, 2 Laoag, 1 Pagudpud), Daily breakfast and 2 local Ilocano meals, Vigan Heritage Village walking tour, Pagudpud beach hopping (Blue Lagoon, Saud Beach), Visit Paoay Church, Cape Bojeador Lighthouse, Sand dunes 4x4 ride and sandboarding experience',
'UNESCO World Heritage Vigan, Paoay Church - Baroque architecture, Pagudpud''s scenic beaches, Thrilling La Paz sand dunes adventure, Calle Crisologo''s old-world charm', 'domilocos.jpg'),

(23, 'Domestic', '🌿', 'Eco-tourists and thrill-seekers', 
'Round-trip flights Manila-Davao-Manila, 4 nights accommodation (2 in Davao City, 2 in Samal Island), Daily breakfast and local cuisine lunch, Visit Philippine Eagle Center and Malagos Garden, Eden Nature Park zipline and sky cycling, Island hopping: Talicud Island, Coral Garden, Durian farm visit and city cultural tour',
'Meet the endangered Philippine Eagle, Sky cycling over Davao forests, Tropical beaches and snorkeling in Samal, Durian and pomelo farms, Cultural immersion and eco-learning', 'domdavao.jpg'),

(24, 'Domestic', '🌋🦈', 'Wildlife lovers and volcano trekkers', 
'Round-trip flights Manila-Legazpi-Manila, 4 nights accommodation (2 Legazpi, 2 Donsol), Daily breakfast and Bicolano spicy dinner feast, Mt. Mayon ATV adventure trail ride, Whale shark interaction tour (seasonal), Cagsawa Ruins historical walking tour, Firefly river cruise and cultural dance show',
'Perfect cone shape of Mayon Volcano, Gentle whale shark encounters in Donsol, Historic Cagsawa Church Ruins, Fiery Bicol cuisine experience, Evening firefly watching cruise', 'dombicol.jpg'),

-- International Packages (IDs 25-36)
(25, 'International', '🏯', 'Culture enthusiasts and first-time visitors to Japan', 
'Round-trip flights Manila-Tokyo, 6 nights accommodation in 4-star hotels, Daily breakfast and 3 authentic Japanese dinners, High-speed rail pass (JR Pass) for intercity travel, Guided tours of temples, shrines, and cultural sites, Visit to Mount Fuji and Hakone, Traditional tea ceremony experience, English-speaking tour guide',
'Cherry blossom viewing (seasonal), Shibuya Crossing and Tokyo Skytree, Fushimi Inari Shrine in Kyoto, Osaka Castle and Dotonbori district, Traditional ryokan stay experience', 'inttokyo.png'),

(26, 'International', '🛕', 'Beach lovers and adventure seekers', 
'Round-trip flights Manila-Bangkok, 5 nights accommodation (2 nights Bangkok, 3 nights Phuket), Daily breakfast and welcome dinner, Airport transfers and inter-city transportation, Island hopping tour to Phi Phi Islands, Bangkok city tour including Grand Palace, Thai massage session, Snorkeling equipment and boat transfers',
'Maya Bay (The Beach movie location), Floating markets of Bangkok, Patong Beach nightlife, Traditional longtail boat rides, Authentic Thai street food tours', 'intbangkok.png'),

(27, 'International', '🦁', 'Families and urban explorers', 
'Round-trip flights Manila-Singapore, 3 nights accommodation in Marina Bay area, Daily breakfast and 2 dinners, Airport transfers and MRT day passes, Gardens by the Bay admission, Universal Studios Singapore tickets, Singapore Zoo or Night Safari, City tour including Merlion and Chinatown',
'Marina Bay Sands SkyPark, Supertree Grove light show, Sentosa Island activities, Hawker center food tours, Clarke Quay riverside dining', 'intsingapore.jpg'),

(28, 'International', '🎎', 'K-pop fans and culture enthusiasts', 
'Round-trip flights Manila-Seoul, 5 nights accommodation in Myeongdong area, Daily breakfast and Korean BBQ dinners, KTX high-speed train to Busan, Gyeongbokgung Palace and Bukchon Hanok Village, Jeju Island day trip (optional), K-beauty shopping tour in Hongdae, Traditional hanbok rental experience',
'Gangnam district and Han River, Busan''s colorful Gamcheon Village, N Seoul Tower and Lotte World, Korean spa (jjimjilbang) experience, Street food tours in Myeongdong', 'intkorea.jpg'),

(29, 'International', '🏮', 'History buffs and nature lovers', 
'Round-trip flights Manila-Hanoi, 7 nights accommodation in heritage hotels, Daily breakfast and traditional Vietnamese meals, Domestic flights Hanoi-Ho Chi Minh City, Ha Long Bay overnight cruise, Cu Chi Tunnels and War Remnants Museum, Mekong Delta day tour, Motorbike street food tour in Ho Chi Minh',
'UNESCO World Heritage Ha Long Bay, Ancient temples and French colonial architecture, Traditional water puppet shows, Floating markets of Mekong Delta, Authentic pho and banh mi experiences', 'intvietnam.jpg'),

(30, 'International', '🕌', 'Luxury travelers and shopping enthusiasts', 	
'Round-trip flights Manila-Dubai, 4 nights accommodation in 5-star hotel, Daily breakfast and farewell dinner, Private airport transfers in luxury vehicles, Burj Khalifa At The Top tickets, Desert safari with BBQ dinner, Dubai Mall and Gold Souk shopping tours, Abu Dhabi city tour including Sheikh Zayed Mosque',
'Burj Al Arab and Palm Jumeirah, Dubai Fountain show and marina cruise, Traditional souks and spice markets, Luxury shopping in world''s largest mall, Atlantis Aquaventure waterpark', 'intdubai.jpg'),

(31, 'International', '🗼', 'Couples and art enthusiasts', 
'Round-trip flights Manila-Paris, 5 nights accommodation in boutique hotel near Champs-Élysées, Daily breakfast and 2 romantic dinners, Airport transfers and Metro passes, Skip-the-line Eiffel Tower and Louvre tickets, Seine River cruise with dinner, Day trip to Palace of Versailles, Wine tasting in Montmartre',
'Iconic Eiffel Tower views, Mona Lisa at the Louvre Museum, Notre-Dame Cathedral and Sainte-Chapelle, Romantic stroll along the Seine, Authentic French café culture', 'intparis.jpg'),

(32, 'International', '🗽', 'City lovers and Broadway fans', 
'Round-trip flights Manila-New York, 5 nights accommodation in Times Square area, Daily breakfast and 2 dinners, Airport transfers and MetroCard passes, Statue of Liberty and Ellis Island ferry, Broadway show tickets, Empire State Building observation deck, Central Park horse carriage ride',
'Times Square neon lights, 9/11 Memorial and One World Observatory, Brooklyn Bridge and High Line walks, Metropolitan Museum of Art, Fifth Avenue luxury shopping', 'intnewyork.jpeg'),

(33, 'International', '🏰', 'History lovers and royal enthusiasts', 
'Round-trip flights Manila-London, 6 nights accommodation near Hyde Park, Daily breakfast and traditional pub dinner, Oyster Card for unlimited transport, Buckingham Palace and Windsor Castle tours, Thames River cruise, Day trip to Stonehenge and Bath, West End musical show',
'Changing of the Guard ceremony, Tower of London and Crown Jewels, Big Ben and Westminster Abbey, British Museum treasures, Traditional afternoon tea experience', 'intlondon.jpg'),

(34, 'International', '🏛️', 'Art lovers and foodies', 
'Round-trip flights Manila-Rome, 8 nights accommodation in central hotels, Daily breakfast and 4 gourmet dinners, Vatican and Colosseum skip-the-line tickets, Train transfers between cities, Gondola ride in Venice, Cooking class in Florence, Walking tour of ancient Rome',
'Trevi Fountain and Spanish Steps, Leaning Tower of Pisa (optional trip), Uffizi Gallery and Renaissance art, St. Peter''s Basilica and Sistine Chapel, Venetian canals and Rialto Bridge', 'intitaly.jpg'),

(35, 'International', '🎭', 'Nature lovers and beach enthusiasts', 
'Round-trip flights Manila-Sydney, 6 nights accommodation with harbor views, Daily breakfast and farewell dinner, Airport transfers and Opal Card, Sydney Opera House guided tour, Blue Mountains day trip with wildlife park, Bondi Beach surfing lesson, Harbor Bridge climb experience',
'Iconic Opera House performances, Sydney Harbour Bridge panoramic views, Bondi to Coogee coastal walk, Taronga Zoo with harbor backdrop, The Rocks historic area exploration', 'intsydney.jpg'),

(36, 'International', '🍁', 'Nature photographers and adventure seekers', 
'Round-trip flights Manila-Calgary, 6 nights lodge accommodation, Daily breakfast and 2 campfire dinners, Banff and Jasper National Park entry, Glacier Skywalk and Columbia Icefield tour, Wildlife watching in Jasper, Lake Louise canoe rental, Scenic drive along Icefields Parkway',
'Snow-capped peaks and turquoise lakes, Moraine Lake and Lake Louise, Athabasca Glacier walking tour, Elk and bear sightings, Stargazing in Jasper''s dark sky preserve', 'intcanada.jpg');

-- Insert package add-ons
INSERT INTO PackageAddOns (PackageID, AddOnName, Price, Unit) VALUES
-- Basic packages add-ons
(1, 'Extra Fun Dive Session', 800.00, 'per session'),
(1, 'Underwater Photography Service', 1200.00, 'per session'),
(1, 'Equipment Rental (Full Set)', 500.00, 'per day'),
(2, 'Private Instructor', 1500.00, 'per day'),
(2, 'Room Upgrade to AC', 800.00, 'per night'),
(3, 'Romantic Dinner Setup', 2000.00, 'per couple'),
(3, 'Professional Photo Package', 1800.00, 'per session'),

-- Multi-day package add-ons
(4, 'Additional Boat Dive', 1200.00, 'per dive'),
(4, 'Equipment Insurance', 300.00, 'per trip'),
(5, 'Private Boat Charter', 3000.00, 'per day'),
(5, 'Massage Therapy Session', 1000.00, 'per session'),

-- Domestic package add-ons
(13, 'Domestic Flight Upgrade', 5000.00, 'per person'),
(13, 'Hotel Room Upgrade', 2000.00, 'per night'),
(13, 'Extra Island Tour', 1500.00, 'per person'),
(14, 'ATV Rental', 800.00, 'per day'),
(14, 'Massage Service', 1200.00, 'per session'),

-- International package add-ons
(25, 'Business Class Upgrade', 25000.00, 'per person'),
(25, '5-Star Hotel Upgrade', 8000.00, 'per night'),
(25, 'Private Tour Guide', 3000.00, 'per day'),
(26, 'Travel Insurance', 2500.00, 'per person'),
(26, 'Visa Processing Assistance', 1500.00, 'per person');

-- Insert promo types
INSERT INTO PromoTypes (PromoTypeName, DiscountPercentage, Description) VALUES
('PWD', 20.00, 'Person with Disability discount as per RA 7277'),
('SENIOR CITIZEN', 20.00, 'Senior Citizen discount as per RA 9994');

-- Insert income sources
INSERT INTO IncomeSources (Category, CommissionRate, MonthlyMin, MonthlyMax, AnnualMin, AnnualMax) VALUES
('Freediving Tour Packages', '12-18%', 20000.00, 50000.00, 240000.00, 600000.00),
('Domestic Tour Packages', '10-15%', 35000.00, 75000.00, 420000.00, 900000.00),
('International Tour Packages', '8-12%', 25000.00, 60000.00, 300000.00, 720000.00),
('Hotel Bookings', '8-10%', 15000.00, 30000.00, 180000.00, 360000.00),
('Flight Bookings', '2-5%', 8000.00, 20000.00, 96000.00, 240000.00),
('Transportation Services', '10-15%', 5000.00, 15000.00, 60000.00, 180000.00),
('Travel Insurance', '15-25%', 3000.00, 8000.00, 36000.00, 96000.00),
('Visa Processing Services', 'Fixed Fee', 5000.00, 12000.00, 60000.00, 144000.00);

-- Insert expense categories
INSERT INTO ExpenseCategories (Category, CategoryType) VALUES
('Technology & Website', 'Operating'),
('Content Creation & SEO', 'Operating'),
('Business Operations', 'Operating'),
('Legal & Compliance', 'Operating'),
('Personal Development', 'Operating');

-- Create database users and permissions
CREATE USER IF NOT EXISTS 'admin'@'localhost' IDENTIFIED BY 'lphttadmin';
GRANT ALL PRIVILEGES ON lakbayph_web.* TO 'admin'@'localhost';

CREATE USER IF NOT EXISTS 'app_user'@'localhost' IDENTIFIED BY 'lphttuser';
GRANT SELECT, INSERT, UPDATE ON lakbayph_web.Users TO 'app_user'@'localhost';
GRANT SELECT ON lakbayph_web.TourPackages TO 'app_user'@'localhost';
GRANT SELECT ON lakbayph_web.TourCategories TO 'app_user'@'localhost';
GRANT SELECT ON lakbayph_web.PackageLocations TO 'app_user'@'localhost';
GRANT SELECT ON lakbayph_web.PackageAddOns TO 'app_user'@'localhost';
GRANT SELECT, INSERT, UPDATE ON lakbayph_web.Bookings TO 'app_user'@'localhost';
GRANT SELECT, INSERT, UPDATE ON lakbayph_web.BookingPayments TO 'app_user'@'localhost';
GRANT SELECT, INSERT, UPDATE ON lakbayph_web.BookingAddOns TO 'app_user'@'localhost';
GRANT SELECT, INSERT ON lakbayph_web.Reviews TO 'app_user'@'localhost';
GRANT SELECT ON lakbayph_web.BookingCompleteView TO 'app_user'@'localhost';
GRANT SELECT ON lakbayph_web.BookingWithReviewsView TO 'app_user'@'localhost';

CREATE USER IF NOT EXISTS 'report_user'@'localhost' IDENTIFIED BY 'lphttreport';
GRANT SELECT ON lakbayph_web.* TO 'report_user'@'localhost';

FLUSH PRIVILEGES;