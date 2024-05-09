# X.Bluesky
[![NuGet version](https://badge.fury.io/nu/X.Bluesky.svg)](https://badge.fury.io/nu/X.Bluesky)
[![Twitter URL](https://img.shields.io/twitter/url/https/twitter.com/andrew_gubskiy.svg?style=social&label=Follow%20me!)](https://twitter.com/intent/user?screen_name=andrew_gubskiy)

The X.Bluesky is a .NET library designed to make it easy for developers to post messages to Bluesky, a decentralized social network. 

By leveraging the Bluesky API, this project allows for straightforward integration into .NET applications, enabling posts to be made programmatically.

## Features

- Post messages directly to Bluesky.
- Attach links to posts, allowing for page previews within the Bluesky feed.
- Authenticate with Bluesky using an identifier and password.

## Getting Started

### Prerequisites

- .NET SDK (compatible with the version used by the project)
- An account on Bluesky with an identifier and password

### Installation

To use the X.Bluesky library in your project, include it as a dependency in your project's file (e.g., `csproj`). Documentation on how to do this will be provided based on the package hosting solution used (e.g., NuGet).

### Usage

```csharp
var identifier = "your.bluesky.identifier";
var password = "your-password-here";

IBlueskyClient client = new BlueskyClient(identifier, password);

var link = new Uri("https://yourlink.com/post/123");

await client.Post("Hello world!", link);
```
