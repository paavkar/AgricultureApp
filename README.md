# AgricultureApp

## Introduction

This application currently has no real functionality.

The project structure is created by following Clean Architecture.

If this application was ran in production, it would be done with Containers.
The containerisation is done with Docker Compose and the API and (eventually)
the UI are served with Nginx.

Currently the application has a .NET Web API for authentication. Authentication
is created with ASP.NET Core Identity and JWT. The JWT setup has support for
multiple roles for users. There is a refresh token setup for the tokens. The refresh
tokens are currently saved with HybridCache.

## User Secrets / App settings

These are the needed key-value pairs to be set:
```
{
  "Kestrel:Certificates:Development:Password": "<SOME_GUID>",
  "Jwt:Key": "<256_BIT_VALUE>",
  "Jwt:Issuer": "Agriculture App",
  "Jwt:Audience": "Agriculture App",
  "ConnectionStrings:DefaultConnection": "<YOUR_SQL_SERVER_CONNECTION>"
}
```