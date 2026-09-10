# LogInsight

LogInsight is a .NET Framework 4.8 solution for creating, reading, and analyzing logs through a shared library, a command-line interface, and an ASP.NET MVC web application.

## Components

The solution contains the following projects:

- **LogInsight**: shared `.dll` library for writing and reading logs.
- **LogInsight.CLI**: console application for generating and displaying logs.
- **LogInsight.Web**: ASP.NET MVC application for viewing and managing logs through a web browser.

## Requirements

- Windows
- Visual Studio 2022 or later
- .NET Framework 4.8
- IIS for hosting the web application
- Administrator permissions when using Windows Event Viewer

## Features

- Write logs to a local file.
- Optionally write logs to Windows Event Viewer.
- Read logs from a local file.
- Read logs from Windows Event Viewer.
- Support for log type, source, application, context, and message.
- Interactive command-line interface.
- ASP.NET MVC web interface.

## Build

1. Clone the repository:

2. Open `LogInsight.sln` in Visual Studio.
3. Restore the NuGet packages.
4. Build the solution using the `Debug` or `Release` configuration.

For the CLI and web application to work correctly, build the `LogInsight` project first. It generates `LogInsight.dll`.

## CLI Usage

Run `LogInsight.CLI.exe` from the project's output directory.

Available commands:

| Command | Description |
| --- | --- |
| `help` | Displays the available commands |
| `log` | Writes sample logs |
| `read -local` | Reads logs from the local file |
| `read -event` | Reads logs from Windows Event Viewer |
| `clear` | Clears the console |
| `exit` | Exits the application |

Example:
```
log read -local exit
```

## Configuration

Settings are read from the `appSettings` section of the application's configuration file.

Example:
```
<appSettings> <add key="WinLogName" value="Application" /> <add key="AppName" value="LogInsight" /> <add key="LogDirectory" value="Logs\Logs.txt" /> <add key="EnableEventViewer" value="false" /> </appSettings>
```

Available settings:

- `WinLogName`: Windows Event Viewer log name.
- `AppName`: application or log source name.
- `LogDirectory`: absolute or relative path to the log file.
- `EnableEventViewer`: enables (`true`) or disables (`false`) Windows Event Viewer.

When `EnableEventViewer` is set to `false`, logs are written to the local file.

## Library Usage

Example of writing a log:
using System.Diagnostics; using LogInsight;
Event.WriteLog( "Operation completed", EventLogEntryType.Information, "Application context");

To create or register the source in Windows Event Viewer:
```
Event.UseSource();
```

## IIS Deployment

1. Install IIS and ASP.NET 4.8 on the server.
2. Build the `LogInsight.Web` project.
3. Publish or copy the application output to a server directory.
4. Create an IIS website or application associated with that directory.
5. Use an application pool configured for `.NET Framework v4.0`.
6. Grant read and write permissions to the directory specified by `LogDirectory`.
7. Ensure that the application pool identity has the required permissions to access Event Viewer, if enabled.

## Permissions

Writing to Windows Event Viewer may require administrator privileges. If the application does not have the required permissions, LogInsight uses the local file as a fallback.

Ensure that the log directory is accessible to the user running the CLI or to the IIS application pool identity.

## License

This project is distributed under the [GPLv3 license](https://opensource.org/licenses/).
