SQL tracking solution that can be used in middleware caching invalidation. This enables:

- A windows service that is continiously polling for SQL change trackings
- Enables the clients to subscribe for a specific table
- Notifies the clients when change happens
- Enables the clients to unsubscribe if needed

To test:


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

3) Create the required table and stored procedures requried by this application
e.g. Run the Tables.sql and StoredProcedures.sql files

4) Run the Sql.ChangeTracking.WindowsService project in debug mode
5) Run the Sql.ChangeTracking.Client project in debug mode
6) Do a change into table Customer e.g.
UPDATE Customer SET FirstName = 'Rahman' where Customer.FirstName = 'rahman'

The client should get notification about this change!
