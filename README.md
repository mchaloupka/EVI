# EVI
[![Build status](https://ci.appveyor.com/api/projects/status/0occxl9nsbjcmkc2/branch/master?svg=true)](https://ci.appveyor.com/project/mchaloupka/evi/branch/master)


http://mchaloupka.github.io/EVI/

.NET implementation of an RDB2RDF storage based on R2RML mapping file

## Development

To be able to develop for this project, you need to have installed:
* .NET SDK 8
* Docker

To test that you can build the whole project, run the following command:
```
dotnet fsi.build.fsx
```

### Integration tests (in Visual Studio)

The integration tests that are part of the solutions require to have databases available. To allow development, run the following command:

```
dotnet fsi build.fsx -t "PrepareDatabases"
```

That will start and initiate all databases necessary for the integration tests. From that moment, it is possible to run the integration tests during development. To delete all the databases (e.g. after development), run the following command:

```
dotnet fsi build.fsx -t "TearDownDatabases"
```

In case you run the full build using FAKE, it will run both `PrepareDatabases` and `TearDownDatabases` commands. Therefore, in case you had the databases running before, the full build command will remove them and it is needed to start them again.
