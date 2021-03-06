# Overview for Auth_Example

An example Blazor Server Application leveraging OIDC to login to the Solid Community Server locally. Makes use of [IdentityModel.OidcClient](https://github.com/IdentityModel/IdentityModel.OidcClient) which is licensed as Apache 2.0 and [DotNetRDF](https://dotnetrdf.org/) which is licensed MIT.

This project assumes you have started up the [Community Solid Server](https://github.com/solid/community-server) locally on your machine and that it is running at http://localhost:3000/ and you've run the setup. For startup instructions on the Solid Community Server, see [here](https://solidproject.org//self-hosting/css).

The goal of this project is to reproduce the actions of the [first app](https://solidproject.org/developers/tutorials/first-app) demo on the Solid Project website.

# Technical Notes

| Item                     | Version | Remarks           |
| ------------------------ | ------- | ----------------- |
| Community Solid Server   | 2.0.1   |                   |
| Auth_Example             | .NET 6  | Blazor Server App |
| IdentityModel.OdicClient | 5.0.0   | Apache License    |
| DotNetRDF                | 2.7.2   | MIT License       |
| Newtonsoft.Json          | 13.0.1  |                   |