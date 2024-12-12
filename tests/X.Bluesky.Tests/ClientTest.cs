using System;
using System.Text;
using System.Threading.Tasks;
using X.Bluesky.Models;
using Xunit;

namespace X.Bluesky.Tests;

public class ClientTest
{
    private readonly string _identifier = "your-identifier";
    private readonly string _password = "your-password";
    
    [Fact(Skip = "On demand")]
    public async Task CheckFacets_Sending()
    {
        var client = new BlueskyClient(_identifier, _password);

        var sb = new StringBuilder();
        sb.AppendLine("Microsoft’s Vision for 2025 and Beyond: Copilot, Responsible AI, Security and Multicloud");
        sb.AppendLine();
        sb.AppendLine("#devdigest #microsoft");


        var text = sb.ToString();

        var uri = new Uri("https://devdigest.today/post/2905");

        await client.Post(text, uri);
    }

    [Fact(Skip = "On demand")]
    public async Task CheckImageUpload_Sending()
    {
        var client = new BlueskyClient(_identifier, _password);


        var sb = new StringBuilder();
        sb.AppendLine("Microsoft’s Vision for 2025 and Beyond: Copilot, Responsible AI, Security and Multicloud");
        sb.AppendLine();
        sb.AppendLine("#devdigest #microsoft");


        var text = sb.ToString();

        var uri = new Uri("https://devdigest.today/post/2905");

        Image image = new Image
        {
            Alt = "Test post",
            Content = Convert.FromBase64String(ExampleImage.Conent),
            MimeType = "image/jpg"
        };

        await client.Post(text, uri, image);
    }
}