open System
open System.Diagnostics




#load "ProcessUtils.fsx"
open ProcessUtils


let args = fsi.CommandLineArgs
Process.execSync($"dotnet")([|"ef migrations";"add MyMigration"; "--project ../SqlServerMigrations"; "-- --provider SqlServer"|])


    
