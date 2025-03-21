# X.Bluesky
[![NuGet Version](http://img.shields.io/nuget/v/X.Bluesky.svg?style=flat)](https://www.nuget.org/packages/X.Bluesky/)
[![Twitter URL](https://img.shields.io/twitter/url/https/twitter.com/andrew_gubskiy.svg?style=social&label=Follow%20me!)](https://twitter.com/intent/user?screen_name=andrew_gubskiy)

The X.Bluesky is a .NET library designed to make it easy for developers to post messages to Bluesky, a decentralized social network. 

By leveraging the Bluesky API, this project allows for straightforward integration into .NET applications, enabling posts to be made programmatically.

## Features

- Post text messages directly to Bluesky
- Attach images to posts with support for alt text
- Add URLs to posts with automatic preview card generation
- Specify post language settings
- Authenticate with Bluesky using an identifier and password
- Support for session management and reuse
- Automatically generate tags, mentions and URL cards
- Customizable base URI for different Bluesky instances
- Embed external content with rich preview cards

## Getting Started

### Prerequisites

- .NET SDK (compatible with the version used by the project)
- An account on Bluesky with an identifier and password

### Installation

To use the X.Bluesky library in your project, include it as a dependency in your project's file (e.g., `csproj`).

See the [NuGet package page](https://www.nuget.org/packages/X.Bluesky/) for the latest version.

### Usage

#### Basic Usage

```csharp
var identifier = "your.bluesky.identifier";
var password = "your-password-here";

IBlueskyClient client = new BlueskyClient(identifier, password);
```


```csharp
// Simple text post
await client.Post("Hello from X.Bluesky!");

// Post with hashtags
await client.Post("Read this post from #devdigest: https://yourlink.com/post/123");

// Post with URL (generates preview card)
await client.Post("Read this post!", new Uri("https://yourlink.com/post/123"));
```

```csharp
// Create a client
var client = new BlueskyClient(
identifier,
password,
reuseSession: true,
baseUri: new Uri("https://bsky.social"));
```

```csharp
// Create a fully customized post
var post = new Post
{
    Text = "Check out this image with my thoughts on #AI development!",
    Images = new[]
    {
        new Image
        {
            Content = File.ReadAllBytes("path/to/image.jpg"),
            MimeType = "image/jpeg",
            Alt = "A diagram showing AI architecture"
        }
    },
    Url = new Uri("https://yourblog.com/ai-article"),
    Languages = new[] { "en" },
    GenerateCardForUrl = true  // Generate a rich preview card for the URL
};
```

```csharp 
// Post to Bluesky
await client.Post(post);
```

Advanced Usage with Post Object
The core method of the library is Post(Models.Post post) which gives you full control over your post content:

```csharp
// Create a client
var client = new BlueskyClient(
identifier,
password,
reuseSession: true,
baseUri: new Uri("https://bsky.social"));

// Create a fully customized post
var post = new Post
{
    Text = "Check out this image with my thoughts on #AI development!",
    Images = new[]
    {
        new Image
        {
            Content = File.ReadAllBytes("path/to/image.jpg"),
            MimeType = "image/jpeg",
            Alt = "A diagram showing AI architecture"
        }
    },
    Url = new Uri("https://yourblog.com/ai-article"),
    Languages = new[] { "en" },
    GenerateCardForUrl = true  // Generate a rich preview card for the URL
};

// Post to Bluesky
await client.Post(post);
```

The Post object allows you to combine multiple elements like text, images, URLs, and metadata in a single post. 
This is especially useful for more complex posts that require fine-grained control over the content and presentation.