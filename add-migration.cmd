cd WebApi1C8Exchange
dotnet ef migrations add %1 --project ..\SqlServerMigrations -- --provider SqlServer
dotnet ef migrations add %1 --project ..\SqliteMigrations -- --provider Sqlite
dotnet ef migrations add %1 --project ..\PostgreSQLMigrations -- --provider PostgreSql
cd ..