# X.Bluesky

[![NuGet Version](https://img.shields.io/nuget/v/X.Bluesky?style=for-the-badge&label=NuGet&color=004880&logo=nuget&logoColor=white)](https://www.nuget.org/packages/X.Bluesky)
[![NuGet Downloads](https://img.shields.io/nuget/dt/X.Bluesky?style=for-the-badge&label=Downloads&color=004880&logo=nuget&logoColor=white)](https://www.nuget.org/packages/X.Bluesky)
[![Sponsor on GitHub](https://img.shields.io/badge/Sponsor_on_GitHub-ff7f00?style=for-the-badge&logo=github&logoColor=white)](https://github.com/sponsors/a-gubskiy)
[![Subscribe on X](https://img.shields.io/badge/Subscribe_on_X-000000?style=for-the-badge&logo=x&logoColor=white)](https://x.com/andrew_gubskiy)

The X.Bluesky is a .NET library designed to make it easy for developers to post messages to Bluesky, a decentralized social network. 

By leveraging the Bluesky API, this project allows for straightforward integration into .NET applications, enabling posts to be made programmatically.

## Features

- Post text messages directly to Bluesky
- Attach images to posts with support for alt text and aspect ratio
- Add URLs to posts with automatic preview card generation
- Specify post language settings
- Authenticate with Bluesky using an identifier and password
- Session reuse between posts
- Automatically generate tags, mentions and URL cards
- Customizable base URI for different Bluesky instances
- Embed external content with rich preview cards

## Getting Started

### Prerequisites

- .NET 10 or .NET 11
- A Bluesky account identifier (handle or email) and password (an [app password](https://bsky.app/settings/app-passwords) is recommended)

### Installation

```bash
dotnet add package X.Bluesky
```

Targets `net10.0` and `net11.0`.

### Usage

#### Basic Usage

```csharp
using X.Bluesky;

var identifier = "your.bluesky.identifier";
var password = "your-app-password";

IBlueskyClient client = new BlueskyClient(identifier, password);

// Simple text post
await client.Post("Hello from X.Bluesky!");

// Post with hashtags and a link (a preview card is generated for the link)
await client.Post("Read this post from #devdigest: https://yourlink.com/post/123");

// Post with an explicit URL (generates preview card)
await client.Post("Read this post!", new Uri("https://yourlink.com/post/123"));
```

The client can also be created with a custom base URL, `IHttpClientFactory` and `ILogger<BlueskyClient>` (all optional):

```csharp
var client = new BlueskyClient(
    identifier,
    password,
    baseUrl: new Uri("https://bsky.social"),
    httpClientFactory: httpClientFactory,
    logger: logger);
```

#### Advanced Usage with Post Object

The core method of the library is `Post(Models.Post post)`, which gives you full control over your post content:

```csharp
using X.Bluesky;
using X.Bluesky.Models;

IBlueskyClient client = new BlueskyClient(identifier, password);

var post = new Post
{
    Text = "Check out this image with my thoughts on #AI development!",
    Images =
    [
        new Image
        {
            Content = await File.ReadAllBytesAsync("path/to/image.jpg"),
            MimeType = "image/jpeg",
            Alt = "A diagram showing AI architecture",
            Width = 1200,  // Optional: with Height, provides an aspect ratio hint
            Height = 800
        }
    ],
    Url = new Uri("https://yourblog.com/ai-article"),
    Languages = ["en"],
    GenerateCardForUrl = true  // Generate a rich preview card for the URL (ignored when images are attached)
};

await client.Post(post);
```

The Post object allows you to combine multiple elements like text, images, URLs, and metadata in a single post. 
This is especially useful for more complex posts that require fine-grained control over the content and presentation.

## License

This project is licensed under the MIT License. See [LICENSE.md](LICENSE.md) for details.
