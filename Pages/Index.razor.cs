namespace TwitterApiBlock.Pages;

using System.Text;
using System.Web;
using Microsoft.AspNetCore.Components;

public partial class Index : ComponentBase
{
    [Inject] public required NavigationManager NavMgr { get; init; }
    [Inject] public required HttpClient Http { get; init; }

    [Parameter, SupplyParameterFromQuery] public string? Code { get; init; }
    [Parameter, SupplyParameterFromQuery] public string? State { get; init; }

    private string _clientId = "";
    private string _secret = "";

    protected override async Task OnInitializedAsync()
    {
        if (Code is null || State is null)
            return;

        var split = HttpUtility.UrlDecode(State).Split(":");
        var challenge = new Guid(Convert.FromBase64String(split[0]));
        _clientId = Encoding.UTF8.GetString(Convert.FromBase64String(split[1]));
        _secret = Encoding.UTF8.GetString(Convert.FromBase64String(split[2]));

        var form = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "code", Code },
            { "grant_type", "authorization_code" },
            { "client_id", _clientId },
            { "redirect_uri", HttpUtility.UrlEncode("https://kanawanagasaki.github.io/TwitterApiBlock") },
            { "code_verifier", challenge.ToString("N") }
        });
        Http.DefaultRequestHeaders.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(_clientId + ":" + _secret)));
        var response = await Http.PostAsync("https://api.twitter.com/2/oauth2/token", form);
        Console.WriteLine(((int)response.StatusCode) + " " + response.StatusCode);
        Console.WriteLine(await response.Content.ReadAsStringAsync());
    }

    void OnAuth()
    {
        var challenge = Guid.NewGuid();
        var state = Convert.ToBase64String(challenge.ToByteArray())
            + ":" + Convert.ToBase64String(Encoding.UTF8.GetBytes(_clientId))
            + ":" + Convert.ToBase64String(Encoding.UTF8.GetBytes(_secret));
        var url = "https://twitter.com/i/oauth2/authorize"
            + "?response_type=code"
            + "&client_id=" + _clientId
            + "&redirect_uri=" + HttpUtility.UrlEncode("https://kanawanagasaki.github.io/TwitterApiBlock")
            + "&scope=tweet.read%20users.read%20block.write"
            + "&state=" + HttpUtility.UrlEncode(state)
            + "&code_challenge_method=plain"
            + "&code_challenge=" + challenge.ToString("N");
        NavMgr.NavigateTo(url);
    }
}
