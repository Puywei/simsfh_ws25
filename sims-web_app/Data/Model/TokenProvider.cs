using sims_web_app.Components.Identity.Services;

namespace sims_web_app.Data.Model;

public class TokenProvider
{
    public string? AccessToken { get; private set; }

    public void SetToken(string token)
    {
        AccessToken = token;
    }

    public void ClearToken()
    {
        AccessToken = null;
    }
}
