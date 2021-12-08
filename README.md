# MinimalHttpLogger

[![Build](https://github.com/johnkors/MinimalHttpLogger/workflows/CI/badge.svg)](https://github.com/johnkors/MinimalHttpLogger/actions)
 [![NuGet](https://img.shields.io/nuget/v/MinimalHttpLogger.svg)](https://www.nuget.org/packages/MinimalHttpLogger/)
[![NuGet](https://img.shields.io/nuget/dt/MinimalHttpLogger.svg)](https://www.nuget.org/packages/MinimalHttpLogger/)


## Why?
My logs were 
* hard to read 
* filling up space(*) 

## What is this?

It's not possible to configure the log pattern of the built-in HttpClient loggers. To modify, one has to replace them. This package replaces the default loggers with a logger that:

1. Reduces the number of log statements on httpclient requests from 4 to 1 
2. Logs 1 aggregated log statement: `{Method} {Uri} - {StatusCode} {StatusCodeLiteral} in {Time}ms`


### Change in output

Before:
```log
info: Start processing HTTP request GET https://www.google.com/
info: Sending HTTP request GET https://www.google.com/
info: Received HTTP response headers after 188.6041ms - 200
info: End processing HTTP request after 188.8026ms - 200
```

After:
```log
info: GET https://www.google.com/ - 200 OK in 186.4883ms
```


## Install

```sh
$ dotnet add package
```

## Usage

```csharp
services.UseMinimalHttpLogger();
```

---

(*) I'm cheap. My Papertrail account stops logging when reaching a certain szie, so reducing the log helps
