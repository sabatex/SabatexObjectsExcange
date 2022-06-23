cd WebApi1C8Exchange
dotnet ef migrations remove --context MSSQLDbContext -- --provider SqlServer
dotnet ef migrations remove --context SQLiteDbContext -- --provider Sqlite
dotnet ef migrations remove --context PostgresDbContext -- --provider PostgreSql
cd ..