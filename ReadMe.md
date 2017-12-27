1) Create the sample database (e.g. use SampleDatabaseAndTables.sql) or use your own database
2) Configure the database and the tables for change tracking e.g.

ALTER DATABASE SQLChangeTrackingTest  
SET CHANGE_TRACKING = ON  
(CHANGE_RETENTION = 2 DAYS, AUTO_CLEANUP = ON)
GO


ALTER TABLE Customer 
ENABLE CHANGE_TRACKING  
WITH (TRACK_COLUMNS_UPDATED = ON)

ALTER TABLE Employee 
ENABLE CHANGE_TRACKING  
WITH (TRACK_COLUMNS_UPDATED = ON)

3) Create the required table and stored procedures
e.g. Run the Tables.sql and StoredProcedures.sql files

4) Run the Sql.ChangeTracking.WindowsService project in debg mode
5) Run the Sql.ChangeTracking.Client project in debug mode
6) Do a change to table Customer e.g.
UPDATE Customer SET FirstName = 'Rahman' where Customer.FirstName = 'rahman'

You should get notification in the client!
