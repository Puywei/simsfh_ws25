using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace sims_web_app.Services
{
    /// <summary>
    /// Store and manage the current user's authentication state as a browser Session JWT and in Server Side Blazor
    /// </summary>
    public class AuthService : IAuthService
    {
        const string AuthTokenName = "auth_token";
        public event Action<ClaimsPrincipal>? UserChanged;
        private ClaimsPrincipal? currentUser;
        private readonly ICustomSessionService _sessionService;
        private readonly ProtectedLocalStorage _protectedLocalStorage;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public AuthService(ICustomSessionService sessionService, ProtectedLocalStorage protectedLocalStorage, IConfiguration configuration)
        {
            _sessionService = sessionService;
            _protectedLocalStorage = protectedLocalStorage;
            _configuration = configuration;
        }

        public ClaimsPrincipal CurrentUser
        {
            get { return currentUser ?? new(); }
            set
            {
                currentUser = value;

                if (UserChanged is not null)
                {
                    UserChanged(currentUser);
                }
            }
        }

        public bool IsLoggedIn => CurrentUser.Identity?.IsAuthenticated ?? false;

        public async Task LogoutAsync()
        {
            //Update the Blazor Server State for the user to an anonymous user
            CurrentUser = new();

            //Remove the JWT from the browser session
            string authToken = await _sessionService.GetItemAsStringAsync(AuthTokenName);

            if (!string.IsNullOrEmpty(authToken))
            {
                await _sessionService.RemoveItemAsync(AuthTokenName);
            }
        }



        /// <summary>
        /// If the user somehow loses their server session, this method will attempt to restore the state from the JWT in the browser session
        /// </summary>
        /// <returns>True if the state was restored</returns>
        /*
        public async Task<bool> GetStateFromTokenAsync()
        {
            bool result = false;
            string authToken = await _sessionService.GetItemAsStringAsync(AuthTokenName);

            var identity = new ClaimsIdentity();

            if (!string.IsNullOrEmpty(authToken))
            {
                try
                {
                    //Ensure the JWT is valid
                    var tokenHandler = new JwtSecurityTokenHandler();
                    var key = System.Text.Encoding.UTF8.GetBytes(_configuration.GetSection("AppSettings:Token").Value);

                    tokenHandler.ValidateToken(authToken, new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ClockSkew = TimeSpan.Zero
                    }, out SecurityToken validatedToken);

                    var jwtToken = (JwtSecurityToken)validatedToken;
                    identity = new ClaimsIdentity(jwtToken.Claims, "jwt");
                    result = true;
                }
                catch
                {
                    //If the JWT is invalid, remove it from the session
                    await _sessionService.RemoveItemAsync(AuthTokenName);

                    //This is an anonymous user
                    identity = new ClaimsIdentity();
                }
            }

            var user = new ClaimsPrincipal(identity);

            //Update the Blazor Server State for the user
            CurrentUser = user;
            return result;
        }
        */

        public async Task<bool> GetStateFromTokenAsync()
        {
            bool result = false;
            ClaimsIdentity identity = new ClaimsIdentity();

            ProtectedBrowserStorageResult<string> tokenBrowserResult = default;
            
            try
            {
                tokenBrowserResult = await _protectedLocalStorage.GetAsync<string>("token");
            }
            catch (CryptographicException)
            {
               await _protectedLocalStorage.DeleteAsync("token");
               
               Console.WriteLine("FIX: Token has been modified, handle appropriately.");
            }

            if (tokenBrowserResult.Success)
            {
                var tokenHandler = new JwtSecurityTokenHandler();

                tokenHandler.ValidateToken(tokenBrowserResult.Value, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("noneofyourgoddamnbusinessthishastobesogoddamnverylongjesuschristandmariaandjoseph")), // key _ plsfix
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                identity = new ClaimsIdentity(jwtToken.Claims, "jwt");
                result = true;
            }
           
            ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(identity);
            
            this.CurrentUser = claimsPrincipal;

            return result;
        }

        public async Task Login(ClaimsPrincipal user)
        {
            //Update the Blazor Server State for the user
            CurrentUser = user;

            //Build a JWT for the user
            var result = await _httpClient.PostAsJsonAsync("auth/login", user);
            if (result.IsSuccessStatusCode)
            {
                var content = await result.Content.ReadAsStringAsync();
                var token = JsonConvert.DeserializeObject<JwtSecurityToken>(content);
                var jwt = new JwtSecurityTokenHandler().WriteToken(token);

                //Write a JWT to the browser session
                await _sessionService.SetItemAsStringAsync(AuthTokenName, jwt);
            }
            else
            {
            }
            
        }
    }


}
