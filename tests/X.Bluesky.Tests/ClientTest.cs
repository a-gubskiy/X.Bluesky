using System;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace X.Bluesky.Tests;

public class ClientTest
{
    [Fact(Skip = "On demand")]
    public async Task CheckFacets_Sending()
    {
        var client = new BlueskyClient("", "");

        var sb = new StringBuilder();
        sb.AppendLine("Microsoft’s Vision for 2025 and Beyond: Copilot, Responsible AI, Security and Multicloud");
        sb.AppendLine();
        sb.AppendLine("#devdigest #microsoft");


        var text = sb.ToString();
        
        var uri = new Uri("https://devdigest.today/post/2905");
        
        await client.Post(text, uri);

    }
    
}