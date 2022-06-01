cd WebApi1C8Exchange
dotnet ef migrations remove --project ..\SqlServerMigrations -- --provider SqlServer
dotnet ef migrations remove --project ..\SqliteMigrations -- --provider Sqlite
dotnet ef migrations remove --project ..\PostgreSQLMigrations -- --provider PostgreSql
cd ..