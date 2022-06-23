cd WebApi1C8Exchange
dotnet ef migrations add %1 --context MSSQLDbContext --output-dir Data/Migrations/MSSQLMigrations -- --databaseProvider MSSQL
dotnet ef migrations add %1 --context SQLiteDbContext --output-dir Data/Migrations/SqliteMigrations -- --databaseProvider Sqlite
dotnet ef migrations add %1 --context PostgresDbContext --output-dir Data/Migrations/PostgresMigrations -- --provider Postgres
cd ..