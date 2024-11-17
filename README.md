# X.Bluesky
[![NuGet Version](http://img.shields.io/nuget/v/X.Bluesky.svg?style=flat)](https://www.nuget.org/packages/X.Bluesky/)
[![Twitter URL](https://img.shields.io/twitter/url/https/twitter.com/andrew_gubskiy.svg?style=social&label=Follow%20me!)](https://twitter.com/intent/user?screen_name=andrew_gubskiy)

The X.Bluesky is a .NET library designed to make it easy for developers to post messages to Bluesky, a decentralized social network. 

By leveraging the Bluesky API, this project allows for straightforward integration into .NET applications, enabling posts to be made programmatically.

## Features

- Post messages directly to Bluesky
- Attach links to posts, allowing for page previews within the Bluesky feed
- Authenticate with Bluesky using an identifier and password
- Automatically generate tags, mentions and url cards 

## Getting Started

### Prerequisites

- .NET SDK (compatible with the version used by the project)
- An account on Bluesky with an identifier and password

### Installation

To use the X.Bluesky library in your project, include it as a dependency in your project's file (e.g., `csproj`). 
See the [NuGet package page](https://www.nuget.org/packages/X.Bluesky/) for the latest version.

### Usage

```csharp
var identifier = "your.bluesky.identifier";
var password = "your-password-here";

IBlueskyClient client = new BlueskyClient(identifier, password);

await client.Post($"Read this post from #devdigest: https://yourlink.com/post/123");

await client.Post($"Read this post!", new Uri("https://yourlink.com/post/123");
```

