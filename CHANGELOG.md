# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## 3.0.0

The new documentation is online on [https://fluentmigrator.github.io](https://fluentmigrator.github.io).

### Breaking changes

- [#850](https://github.com/fluentmigrator/fluentmigrator/issues/850): Set minimum .NET Framework version to 4.6.1. Older versions aren't supported anymore.
- `ProcessorOptions.Timeout` is now of type `System.TimeSpan?`
- `MigrationRunner.MaintenanceLoader` is now read-only
- `MigrationRunner.CaughtExceptions` returns now a `IReadOnlyList`
- `dotnet-fm` is now a global tool and requires at least the .NET Core tooling 2.1-preview2

### Added

- [#851](https://github.com/fluentmigrator/fluentmigrator/issues/851): Enable the usage of [Microsoft.Extensions.DependencyInjection](https://github.com/aspnet/DependencyInjection/)
- [#852](https://github.com/fluentmigrator/fluentmigrator/issues/852): Replace custom configuration mechanisms by using [Microsoft.Extensions.Options](https://github.com/aspnet/Options/)
- [#853](https://github.com/fluentmigrator/fluentmigrator/issues/853): Replace the announcer with [Microsoft.Extensions.Logging](https://github.com/aspnet/Logging/)
- Support for loading connection strings using a provided `IConfiguration` service ([Microsoft.Extensions.Configuration](https://github.com/aspnet/Configuration/))
- [#822](https://github.com/fluentmigrator/fluentmigrator/issues/822): `IMigrationExpressionValidator` for custom migration expression validation
- [#809](https://github.com/fluentmigrator/fluentmigrator/issues/809): Ability to add a schema owner during schema creation for SQL Server

### Fixed

- [#767](https://github.com/fluentmigrator/fluentmigrator/issues/767): Append `NULL` constraint for custom types for PostgreSQL and SQL Server

### Deprecated

- `IAssemblyCollection` and all its implementations
- `IAnnouncer` and all its implementations
- `IMigrationRunnerConventions.GetMigrationInfo`
- `IProfileLoader.ApplyProfiles()`
- `IProfileLoader.FindProfilesIn`
- `IMigrationProcessorOptions`
- `IMigrationProcessorFactory` and all its implementations
- `IRunnerContext` and `RunnerContext`, replaced by several dedicated options classes:
  - `RunnerOptions` are the new `RunnerContext` (minus some properties extracted into separate option classes)
  - `ProcessorOptions` for global processor-specific options 
  - `GeneratorOptions` to allow setting the compatibility mode
  - `TypeFilterOptions` for filtering migrations by namespace
  - `AnnouncerOptions` to enable showing SQL statements and the elapsed time
  - `SelectingProcessorAccessorOptions` allows selection of a processor by its identifier
  - `SelectingGeneratorAccessorOptions` allows selection of a generator by its identifier
  - `AppConfigConnectionStringAccessorOptions` to allow leading the connection strings from the *.config xml file (deprecated, only for transition to `Microsoft.Extensions.Configuration`)
- `CompatabilityMode` (is now `ComatibilityMode`)
- `ApplicationContext` in various interfaces/classes
- `ManifestResourceNameWithAssembly` replaced by `ValueTuple`
- `MigrationGeneratorFactory`
- `MigrationProcessorFactoryProvider`
- `ITypeMap.GetTypeMap(DbType, int, int)`
- `IDbFactory`: Only the implementations will remain
- Several non-DI constructors

### Additional information

#### Connection string handling

The library assumes that in `ProcessorOptions.ConnectionString` is either a connection string or
a connection string identifier. This are the steps to load the real connection string.

- Queries all `IConnectionStringReader` implementations
  - When a connection string is returned by one of the readers, then this
    connection string will be used
  - When no connection string is returned, try reading from the next `IConnectionStringReader`
- When no reader returned a connection string, then return `ProcessorOptions.ConnectionString`

The connection string stored in `ProcessorOptions.ConnectionString` might be overridden
by registering the `IConnectionStringReader` instance `PassThroughConnectionStringReader`
as scoped service.

When no connection string could be found, the `SelectingProcessorAccessor` returns
a `ConnectionlessProcessor` instead of the previously selected processor.

#### Instantiating a migration runner

```c#
// Initialize the services
var serviceProvider = new ServiceCollection()
    .AddLogging(lb => lb.AddFluentMigratorConsole())
    .AddFluentMigratorCore()
    .ConfigureRunner(
        builder => builder
            .AddSQLite()
            .WithGlobalConnectionString(connectionString)
            .WithMigrationsIn(typeof(AddGTDTables).Assembly))
    .BuildServiceProvider();

// Instantiate the runner
var runner = serviceProvider.GetRequiredService<IMigrationRunner>();

// Run the migrations
runner.MigrateUp();
```

This adds the FluentMigrator services to the service collection and
configures the runner to use SQLite with the given connection string,
announcer and migration assembly.

Now you can instantiate the runner using the built service provider and use
its functions.

## 2.0.7 (2018-04-27)

### Added

- [#856](https://github.com/fluentmigrator/fluentmigrator/pull/865) New constructors to enable passing a custom expression convention set

## 2.0.6 (2018-04-24)

### Fixed

- [#859](https://github.com/fluentmigrator/fluentmigrator/issues/859): The pound sign is only recognized when it's only preceeded by whitespace

## 2.0.5 (2018-04-23)

### Added

- net452 build for the console runner to enable usage of the latest MySQL ADO.NET provider
 
### Changed
 
- Added more ADO.NET providers for the console
- The tools are in platform-specific sub-directories again (e.g. `tools/net452/x86/Migrate.exe`)

This has become necessary to enable a better out-of-the-box experience for the migration tool.

## 2.0.4 (2018-04-23)

Unlisted due to unintentional breaking change.

## 2.0.3 (2018-04-22)

### Fixed

- [#858](https://github.com/fluentmigrator/fluentmigrator/issues/858): Don't even try to set the command timeout for SQL Server CE

## 2.0.2 (2018-04-17)

### Fixed

- [#856](https://github.com/fluentmigrator/fluentmigrator/issues/856): Don't fail when an assembly couldn't be loaded
- [#848](https://github.com/fluentmigrator/fluentmigrator/pull/848): `MySql4ProcessorFactory` used the `MySql5Generator`

## 2.0.1 (2018-04-16)

### Fixed

- `FluentMigrator.Console` now contains the migration tool in the `tools/` directory

### Added

- Obsolete `FluentMigrator.Tools` package added as upgrade path

## 2.0.0 (2018-04-15)

### Breaking changes

- `IQuerySchema.DatabaseType` now returns `SqlServer2016`, etc... and not `SqlServer` any more
- Database specific code was moved into its own assemblies
- `IMigrationConventions` was renamed to `IMigrationRunnerConventions`
- `IMigrationContext` doesn't contain the `IMigrationConventions` any more
  - Expression conventions are now bundled in the new `IConventionSet`
- `ICanBeConventional` was removed during the overhaul of the expression convention system
- Strings are now Unicode by default. Use `NonUnicodeString` for ANSI strings
- `FluentMigrator.Tools` was split into the following packages
  - `FluentMigrator.Console`: The `Migrate.exe` tool
  - `FluentMigrator.MSBuild`: The MSBuild `Migrate` task

### Added

- Framework: .NET Standard 2.0 support
- Database:
  - SQL Anywhere 16 support
  - SQL Server 2016 support
  - MySQL:
    - `ALTER/DROP DEFAULT` value support
  - MySQL 5:
    - New dialect
    - `NVARCHAR` for `AsString`
  - SQL Server 2005
    - `WITH (ONLINE=ON/OFF)` support
    - 64 bit identity support
  - Redshift (Amazon, experimental)
  - Firebird
    - New provider option: `Force Quote=true` to enforce quotes
  - All supported databases
    - Streamlined table/index schema quoting
- Unique Constraints: Non-Distinct NULL support (SQL Server 2008 and SQL Anywhere 16)
- Types: DateTime2 support
- Dialect: SQLite foreign key support
- Insert/Update/Delete: DbNull support
- Expression:
  - IfDatabase: Predicate support
  - IfDatabase: Method delegation support
  - Index: Creation with non-key columns
  - Conventions: Default schema name support
  - `SetExistingRowsTo` supports `SystemMethods`
  - Passing arguments to embedded SQL scripts
- Runner:
  - TaskExecutor: HasMigrationsToApply support
  - Case insensitive arguments support
  - `StopOnError` flag

### Changed

- Project:
  - Moving database specific code from `FluentMigrator.Runner` to `FluentMigrator.Runner.<Database>`
  - Extension methods for - e.g. SqlServer - are now in `FluentMigrator.Extensions.SqlServer`
- Database:
  - MySQL: Now announcing SQL scripts
- Runner:
  - Better error messages
  - ListMigrations: showing `(not applied)` for unapplied migrations
  - Show `(BREAKING)` for migrations with breaking changes
  - MSBuild task is available as separate package (with custom .targets file)
  - Use provider default command timeout when no global timeout is set

### Deprecated

- Generic:
  - `IAnnouncer.Write`

### Removed

- Generic:
  - Deprecated functions
  - SchemaDump experiment
  - T4 experiment
- Framework:
  - .NET Framework 3.5 support
- Runner:
  - NAnt build task

### Fixed

- Runner:
  - Match `TagAttribute` by inheritance
- Processors (database specific processing of expressions):
  - Using the new `SqlBatchParser` to parse batches of SQL statements (`GO` statement support)
- Database:
  - Hana: Fixed syntax for dropping a primary key
  - Oracle: Table schema now added more consistently
- Tests:
  - Mark integration tests as ignored when no active processor could be found
