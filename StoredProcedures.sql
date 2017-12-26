﻿
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

Create PROCEDURE USP_TableVersionChangeTracking
	@VersionToStart Int = NULL
AS
BEGIN
				
	/*
		Just to instruct Entity framework to generate columns
	*/
	SET FMTONLY OFF
	
	SET NOCOUNT ON;

	-- Begin batch transaction
	BEGIN TRANSACTION

		DECLARE @LastProcessedVersion BIGINT;
		SELECT @LastProcessedVersion = [Version] FROM LastVersionProcessed;
		DECLARE @PreviousVersion BIGINT =  COALESCE(@LastProcessedVersion, @VersionToStart, CHANGE_TRACKING_CURRENT_VERSION ()  );
	
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

			DECLARE @LastVersionProcessed BIGINT = NULL;
			SELECT @LastVersionProcessed = [VERSION] FROM LastVersionProcessed

			IF(ISNULL(@LastVersionProcessed, -1) < 0)
				INSERT INTO LastVersionProcessed VALUES(CHANGE_TRACKING_CURRENT_VERSION () -1);
			ELSE
				Update LastVersionProcessed SET [Version] = CHANGE_TRACKING_CURRENT_VERSION () 

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