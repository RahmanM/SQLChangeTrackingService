
IF EXISTS (
  SELECT * 
    FROM INFORMATION_SCHEMA.ROUTINES 
   WHERE SPECIFIC_SCHEMA = N'dbo'
     AND SPECIFIC_NAME = N'USP_TableVersionChangeTracking' 
)
   DROP PROCEDURE dbo.USP_TableVersionChangeTracking
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

Create PROCEDURE USP_TableVersionChangeTracking
	@VersionToStart Int = NULL -- Set it to null if you want to get chnage trackings as they are produced
AS

/*
	This is the main stored procedure that is responsible for tracking the table changes and returning the change tracking information.
	It track the changes and store the last processed version into LastVersionProcessed table so can track the future changes.
	Alternatively, we can get specific version changes by supplying the @VersionToStart parameter.

	Usage example:

	 DECLARE @VersionToStart int = null	 -- Set it to null if you want to get chnage trackings as they are produced
	EXEC USP_TableVersionChangeTracking @VersionToStart

*/
BEGIN
				
	/*
		Just to instruct Entity framework to generate columns
	*/
	SET FMTONLY OFF
	
	SET NOCOUNT ON;

	-- Begin batch transaction
	BEGIN TRANSACTION

		DECLARE @LastProcessedVersion BIGINT;
		SELECT @LastProcessedVersion = ISNULL([Version],-1) FROM LastVersionProcessed;
		DECLARE @PreviousVersion BIGINT =  COALESCE(@VersionToStart, @LastProcessedVersion, CHANGE_TRACKING_CURRENT_VERSION ()  );
	
		BEGIN TRY

			IF OBJECT_ID(N'tempdb.dbo.#TablesWithChangeTracking', 'U') IS NOT NULL
			BEGIN
			  DROP TABLE #TablesWithChangeTracking;
			END;

			IF OBJECT_ID(N'tempdb.dbo.#TableChanges', 'U') IS NOT NULL
			BEGIN
			  DROP TABLE #TableChanges;
			END;


			/*
				All the tables that change tracking is enabled for
			*/
			CREATE TABLE #TablesWithChangeTracking
			(
			Id INT Identity(1,1) Primary key,
			Name varchar(500),
			ObjectId int,
			ChangeVersion int,
			)

			INSERT INTO #TablesWithChangeTracking
				SELECT
					st.name,
					st.object_id,
					ct.is_track_columns_updated_on
				FROM sys.change_tracking_tables ct
				JOIN sys.tables st
					ON st.object_id = ct.object_id

			DECLARE @tablename NVARCHAR(500), 
					@id int;
			DECLARE @sql NVARCHAR(1000);

			/*
				Tables with change tracking records
			*/
			CREATE TABLE #TableChanges
			(
				Id INT Primary key IDENTITY(1,1),
				Name varchar(500) NULL,
				SysChangeOperation varchar(2) null,
				SysChangeVersion int null
			)

			While (SELECT COUNT(1) FROM #TablesWithChangeTracking) > 0
			BEGIN

				SELECT
					@tablename = name,
					@id = id
				FROM #TablesWithChangeTracking

				SET @sql =
					N'  DECLARE @ChangeVersion int, @SysChangeOperation varchar(2);

						SELECT TOP 1
							@ChangeVersion = Sys_change_version,
							@SysChangeOperation = sys_change_operation
						FROM CHANGETABLE
						(CHANGES ' + @tablename + ',  ' + Cast(@PreviousVersion AS VARCHAR) + ' ) AS CT
						ORDER by Sys_change_version DESC

						INSERT INTO #TableChanges (name, SysChangeOperation, SysChangeVersion)
										VALUES	  (''' + @tablename + ''', @SysChangeOperation, @ChangeVersion)
					'

				EXECUTE (@sql)

				-- Drope the row that is already processed
				DELETE #TablesWithChangeTracking WHERE Id = @id

			END
			
			IF(SELECT COUNT(*) FROM #TableChanges WHERE SysChangeVersion IS NOT NULL)  > 0 AND @VersionToStart IS NULL
			BEGIN
				DECLARE @LastVersionProcessed BIGINT = NULL;
				SELECT @LastVersionProcessed = ISNULL([Version], -1) FROM LastVersionProcessed

				IF(@LastVersionProcessed < 0)
					INSERT INTO LastVersionProcessed VALUES(@LastProcessedVersion);
				ELSE
					UPDATE LastVersionProcessed SET [Version] = @LastProcessedVersion + 1;
			END
			
			SELECT * FROM #TableChanges 
			WHERE SysChangeVersion IS NOT NULL

			DROP TABLE #TablesWithChangeTracking
			DROP table #TableChanges

		-- Commit all changes
		COMMIT;

	END TRY
	BEGIN Catch	
		
		-- Rollback transaction
		ROLLBACK

		SELECT  
		ERROR_NUMBER() AS ErrorNumber  
		,ERROR_SEVERITY() AS ErrorSeverity  
		,ERROR_STATE() AS ErrorState  
		,ERROR_PROCEDURE() AS ErrorProcedure  
		,ERROR_LINE() AS ErrorLine  
		,ERROR_MESSAGE() AS ErrorMessage;  
		
	END Catch

END
GO


IF EXISTS (
  SELECT * 
    FROM INFORMATION_SCHEMA.ROUTINES 
   WHERE SPECIFIC_SCHEMA = N'dbo'
     AND SPECIFIC_NAME = N'Usp_GetChangeTrackingVersion' 
)
   DROP PROCEDURE dbo.Usp_GetChangeTrackingVersion
GO

CREATE PROC Usp_GetChangeTrackingVersion
AS
/*
	Returns the current change tracking version from the database

	Usage example:

	EXEC Usp_GetChangeTrackingVersion

*/
BEGIN
	Select CHANGE_TRACKING_CURRENT_VERSION ()
END
	
GO