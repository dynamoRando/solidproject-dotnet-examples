# Overview For todo

An example Blazor Server Application intended to reproduce the Solid To Do App [example](https://www.freecodecamp.org/news/create-a-solid-to-do-app-with-react/).

This project assumes you have started up the [Community Solid Server](https://github.com/solid/community-server) locally on your machine and that it is running at http://localhost:3000/ and you've run the setup as file storage in a folder:

```
community-solid-server -c @css:config/file.json
```

For startup instructions on the Community Solid Server, see [here](https://solidproject.org//self-hosting/css).

# Technical Notes

| Item                     | Version | Remarks           |
| ------------------------ | ------- | ----------------- |
| Community Solid Server   | 2.0.1   |                   |
| todo                     | .NET 6  | Blazor Server App |
| IdentityModel.OdicClient | 5.0.0   | Apache License    |
| DotNetRDF                | 2.7.2   | MIT License       |
| Newtonsoft.Json          | 13.0.1  | MIT License       |
| 
# App Walkthru
## Quick Notes
One of the nice things about Solid is that it is built on top of [open standards](https://github.com/solid/solid#about-solid); therefore as long as the technology stack you are working with understands these standards, you can implement a front end to a Solid server in any platform you like.

At a rough level, this example application simply leverages -
* [RDF](https://www.w3.org/RDF/) (Resource Description Framework) - to store the To Do data at the Community Solid Server
* [HTTP](https://en.wikipedia.org/wiki/Hypertext_Transfer_Protocol) - to communicate with the Community Solid Server
* [OIDC](https://github.com/solid/webid-oidc-spec) (OpenId Connect) - to authenticate yourself to the Community Solid Server

## Items


(to be completed)