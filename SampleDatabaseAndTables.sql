CREATE DATABASE SQLChangeTrackingTest
GO

ALTER DATABASE SQLChangeTrackingTest SET COMPATIBILITY_LEVEL = 100
GO

-- Enable database change tracking
ALTER DATABASE SQLChangeTrackingTest  
SET CHANGE_TRACKING = ON  
(CHANGE_RETENTION = 2 DAYS, AUTO_CLEANUP = ON)
GO

USE SQLChangeTrackingTest

Create Table Customer
(
	ID INT NOT NULL Primary KEY IDENTITY(1,1),
	FirstName Varchar(25) NULL,
	LastName  Varchar(25) NULL
)

Create Table Employee
(
	ID INT NOT NULL Primary KEY IDENTITY(1,1),
	FirstName Varchar(25) NULL,
	LastName  Varchar(25) NULL
)

-- Enable tables change tracking
ALTER TABLE Customer 
ENABLE CHANGE_TRACKING  
WITH (TRACK_COLUMNS_UPDATED = ON)

ALTER TABLE Employee 
ENABLE CHANGE_TRACKING  
WITH (TRACK_COLUMNS_UPDATED = ON)

-- Insert sample data for testing
INSERT INTO Customer (Customer.FirstName, Customer.LastName) VALUES('rahman', 'mahmoodi')
INSERT INTO Employee (Customer.FirstName, Customer.LastName) VALUES('roya', 'mahmoodi')