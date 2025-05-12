# Windows

### Pre-Requisites

Moras.Net requires at least:

- Visual Studio 2015 Community edition ([download](https://visualstudio.microsoft.com/))
- .NET Framework 4.5 Developer Pack (installed with Visual Studio)
- Git command line tools ([download](https://git-scm.com/downloads/win) or install with Visual Studio)
- NuGet client 3.5.0 ([download](https://www.nuget.org/downloads) or install with Visual Studio)

### Clone the Moras.Net repository

```
git clone https://github.com/DiablosOffens/Moras.Net.git
git submodule update --init
```

### Restore global NuGet packages

First install NuGet using these [instructions](https://learn.microsoft.com/en-us/nuget/install-nuget-client-tools?tabs=windows#nugetexe-cli) and
make sure the `nuget.exe` is on your PATH environment variable.
Then just run the following on the command line in the same folder where `Moras.Net.sln` resides:

```
nuget restore
```


### Open in Visual Studio

- Open the `Moras.Net.sln` solution in Visual Studio 2015 or newer. The free Visual Studio Community edition works fine.
- Build the project by clicking on _Build -> Build Solution_ menu item or hit `Ctrl`+`Shift`+`b`.
- Click on the play icon or hit `F5` to run the `Moras.Net` main project in debugger and see the application in action.

# Linux/macOS

Building on Linux/macOS is currently not supported.
