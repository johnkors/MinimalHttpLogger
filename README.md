# MinimalHttpLogger

[![Build](https://github.com/johnkors/MinimalHttpLogger/workflows/CI/badge.svg)](https://github.com/johnkors/MinimalHttpLogger/actions)
 [![NuGet](https://img.shields.io/nuget/v/MinimalHttpLogger.svg)](https://www.nuget.org/packages/MinimalHttpLogger/)
[![NuGet](https://img.shields.io/nuget/dt/MinimalHttpLogger.svg)](https://www.nuget.org/packages/MinimalHttpLogger/)


## What is this?

Configures the HttpClient factory logging with a logger than reduces the number of log statements on httpclient requests from 4 to 1.

```log
info: LogicalHandler[100] Start processing HTTP request GET https://www.google.com/
info: ClientHandler[100] Sending HTTP request GET https://www.google.com/
info: ClientHandler[101] Received HTTP response headers after 188.6041ms - 200
info: LogicalHandler[101] End processing HTTP request after 188.8026ms - 200
```

to

```log
info: ClientHandler[101] GET https://www.google.com/ - OK in 186.4883ms
```


## Install

`$ dotnet add package `

## Usage

```csharp
services.UseMinimalHttpLogger();
```
