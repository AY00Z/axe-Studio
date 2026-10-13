using backend.ApplicationCore.Auth;
using backend.ApplicationCore.Auth.Requests;
using backend.ApplicationCore.Auth.Responses;
using backend.Domain.Entities.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;


namespace backend.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private static readonly ConcurrentDictionary<string, DateTime> _blacklistedTokens = new ConcurrentDictionary<string, DateTime>();
        private readonly TimeSpan _tokenExpiration = TimeSpan.FromHours(1);
        private const string CredentialsFilePath = "C:\\Users\\aya20\\source\\repos\\backend\\backend\\WebApi\\Data\\credentials.json";

        public AuthController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel login)
        {
            try
            {
                var formData = new Dictionary<string, string>
        {
            {"client_id", "myapp-backend"},
            {"client_secret", "gtxuKz4iBomMtTQlHE21Y2dDOWkGQAAP"},
            {"username", login.Username},
            {"password", login.Password},
            {"grant_type", "password"}
        };

                using var client = new HttpClient();
                var response = await client.PostAsync(
                    "http://localhost:8080/realms/MyAppRealm/protocol/openid-connect/token",
                    new FormUrlEncodedContent(formData));

                if (!response.IsSuccessStatusCode)
                    return Unauthorized("Invalid credentials");

                var content = await response.Content.ReadAsStringAsync();
                var tokenResponse = JsonConvert.DeserializeObject<dynamic>(content);

                // Save credentials 
                SaveCredentialsToFile(login.Username, login.Password, tokenResponse);

              
                return Ok(new { token = (string)tokenResponse.access_token }); 
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        private void SaveCredentialsToFile(string username, string password, dynamic tokenResponse)
        {
            var credentialsData = new
            {
                Username = username,
                Password = password, // Note: Storing passwords in plaintext is not recommended
                Token = (string)tokenResponse.access_token,
                RefreshToken = (string)tokenResponse.refresh_token,
                ExpiresIn = (int)tokenResponse.expires_in,
                TokenType = (string)tokenResponse.token_type,
                Timestamp = DateTime.UtcNow
            };

            var jsonContent = JsonConvert.SerializeObject(credentialsData, Formatting.Indented);
            System.IO.File.WriteAllText(CredentialsFilePath, jsonContent);
        }


        private async Task<string> GetKeycloakToken(string username, string password)
        {
            var client = new HttpClient();
            var request = new FormUrlEncodedContent(new[]
            {
        new KeyValuePair<string, string>("client_id", "myapp-backend"),
        new KeyValuePair<string, string>("client_secret", "VW9NFNrpNFyyteV0oupi3i2OO1nCJu47"),
        new KeyValuePair<string, string>("username", username),
        new KeyValuePair<string, string>("password", password),
        new KeyValuePair<string, string>("grant_type", "password"),
    });

            var response = await client.PostAsync(
                "http://localhost:8080/realms/MyAppRealm/protocol/openid-connect/token",
                request);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var json = JsonConvert.DeserializeObject<dynamic>(content);
                return json?.access_token;
            }

            return null;
        }

        private void SaveCredentials(string username, string password, string token)
        {
            List<Credential> credentials = new List<Credential>();

            if (System.IO.File.Exists(CredentialsFilePath))
            {
                var existingJson = System.IO.File.ReadAllText(CredentialsFilePath);
                credentials = JsonConvert.DeserializeObject<List<Credential>>(existingJson) ?? new List<Credential>();
            }

            // Remove any existing credentials for this user
            credentials.RemoveAll(c => c.Username == username);

            // Add new credential with username, password (consider encryption), and token
            credentials.Add(new Credential
            {
                Username = username,
                Password = password, // Note: Consider encrypting this in production
                Token = token,
                LastUpdated = DateTime.UtcNow
            });

            var updatedJson = JsonConvert.SerializeObject(credentials, Formatting.Indented);
            System.IO.File.WriteAllText(CredentialsFilePath, updatedJson);
        }
        private async Task<bool> VerifyCredentials(string username, string password)
        {
            try
            {
                var authRequest = new
                {
                    Username = username,
                    Password = password,
                    UserEntity = "1"
                };

                var jsonContent = JsonConvert.SerializeObject(authRequest);
                var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                using var httpClient = new HttpClient();
                httpClient.Timeout = TimeSpan.FromSeconds(30);

                var endpointUrl = "http://localhost:222/iam/api/Authentication/ExternalLogin";
                var response = await httpClient.PostAsync(endpointUrl, httpContent);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();

                    // Check if response contains a token field
                    try
                    {
                        var jsonResponse = JsonConvert.DeserializeObject<dynamic>(responseContent);
                        return jsonResponse?.token != null;  // Returns true if token exists
                    }
                    catch
                    {
                        // If not valid JSON or missing token field
                        return false;
                    }
                }

                return false;
            }
            catch
            {
                return false;
            }
        }


        private string GenerateJwtToken(string username)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
        new Claim(JwtRegisteredClaimNames.Sub, username),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        new Claim(ClaimTypes.Name, username),
        new Claim("UserShortname", $"DOMAIN\\{username}") // Fixed claim
    };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.Add(_tokenExpiration),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        [HttpGet("[action]")]
        public IActionResult GetCredentials()
        {
            try
            {
                if (!System.IO.File.Exists(CredentialsFilePath))
                {
                    return NotFound("Credentials file not found");
                }

                var fileContent = System.IO.File.ReadAllText(CredentialsFilePath);

                if (string.IsNullOrWhiteSpace(fileContent))
                {
                    return NotFound("No credentials available");
                }

                var credentials = JsonConvert.DeserializeObject<List<Credential>>(fileContent);

                if (credentials == null || credentials.Count == 0)
                {
                    return NotFound("No credentials available");
                }

                // Get the most recently updated credential
                var latestCredential = credentials.OrderByDescending(c => c.LastUpdated).First();

                // Return all credential information including token
                return Ok(new
                {
                    Username = latestCredential.Username,
                    Password = latestCredential.Password,

                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while retrieving credentials: {ex.Message}");
            }
        }






        [HttpGet("[action]")]
        public IActionResult GetToken()
        {
            try
            {
                if (!System.IO.File.Exists(CredentialsFilePath))
                {
                    return NotFound("Credentials file not found");
                }

                var fileContent = System.IO.File.ReadAllText(CredentialsFilePath);

                if (string.IsNullOrWhiteSpace(fileContent))
                {
                    return NotFound("No credentials available");
                }

                var credentials = JsonConvert.DeserializeObject<List<Credential>>(fileContent);

                if (credentials == null || credentials.Count == 0)
                {
                    return NotFound("No credentials available");
                }

                // Get the most recently updated credential
                var latestCredential = credentials.OrderByDescending(c => c.LastUpdated).First();

                // Return all credential information including token
                return Ok(new
                {

                    Token = latestCredential.Token,
                    LastUpdated = latestCredential.LastUpdated
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while retrieving credentials: {ex.Message}");
            }
        }


        [HttpPost("logout")]
        public async Task<IActionResult> LogoutAsync()
        {
            try
            {
                // 1. Read and validate credentials file
                if (!System.IO.File.Exists(CredentialsFilePath))
                {
                    return BadRequest("No credentials file found");
                }

                var jsonContent = await System.IO.File.ReadAllTextAsync(CredentialsFilePath);

                if (string.IsNullOrWhiteSpace(jsonContent) || jsonContent == "{}")
                {
                    return BadRequest("No active session found");
                }

                // 2. Deserialize with correct property names
                var credentials = JsonConvert.DeserializeObject<Credentials>(jsonContent);
                string token = credentials?.Token; // Changed from access_token to Token

                if (string.IsNullOrEmpty(token))
                {
                    return BadRequest("No valid token found in credentials");
                }

                Console.WriteLine($"Logging out token: {token}");

                // 3. Add to blacklist
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(token);
                _blacklistedTokens.TryAdd(token, jwtToken.ValidTo);

                // 4. Revoke token with Keycloak
                await RevokeKeycloakToken(token);

                // 5. Clear credentials file
                await ClearCredentialsFile();

                return Ok(new { message = "Successfully logged out" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Logout failed: {ex.Message}");
            }
        }

        // Add this class to match your JSON structure
        public class Credentials
        {
            public string Username { get; set; }
            public string Password { get; set; }
            public string Token { get; set; }        // Was access_token
            public string RefreshToken { get; set; } // Was refresh_token
            public int ExpiresIn { get; set; }
            public string TokenType { get; set; }
            public DateTime Timestamp { get; set; }
        }

        private async Task ClearCredentialsFile()
        {
            try
            {
                await System.IO.File.WriteAllTextAsync(CredentialsFilePath, "{}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error clearing credentials: {ex.Message}");
            }
        }

        private async Task RevokeKeycloakToken(string token)
        {
            using var client = new HttpClient();
            var formData = new Dictionary<string, string>
    {
        {"client_id", _configuration["Keycloak:ClientId"]},
        {"client_secret", _configuration["Keycloak:ClientSecret"]},
        {"token", token},
        {"token_type_hint", "access_token"}
    };

            await client.PostAsync(
                $"{_configuration["Keycloak:Authority"]}/protocol/openid-connect/revoke",
                new FormUrlEncodedContent(formData));
        }


        private string ReadTokenFromCredentials()
        {
            if (!System.IO.File.Exists(CredentialsFilePath))
            {
                return null;
            }

            var json = System.IO.File.ReadAllText(CredentialsFilePath);
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            try
            {
                var credentials = JsonConvert.DeserializeObject<dynamic>(json);
                return credentials?.access_token?.ToString();
            }
            catch
            {
                return null;
            }
        }





        [HttpGet("validate")]
        public IActionResult ValidateToken([FromQuery] string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return BadRequest(new { valid = false, message = "Token is required" });
            }

            if (_blacklistedTokens.ContainsKey(token))
            {
                return Ok(new { valid = false, message = "Token has been invalidated" });
            }

            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();

                if (!tokenHandler.CanReadToken(token))
                {
                    return Ok(new { valid = false, message = "Invalid JWT format" });
                }

                var jwtToken = tokenHandler.ReadJwtToken(token);

                if (jwtToken.Payload.TryGetValue("exp", out var expObj))
                {
                    if (expObj is long expUnixTime)
                    {
                        var expiryDate = DateTimeOffset.FromUnixTimeSeconds(expUnixTime).UtcDateTime;
                        if (expiryDate < DateTime.UtcNow)
                        {
                            return Ok(new { valid = false, message = "Token has expired" });
                        }
                    }
                }

                try
                {
                    var secretKey = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);
                    tokenHandler.ValidateToken(token, new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(secretKey),
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ClockSkew = TimeSpan.Zero,
                        TokenDecryptionKey = null,
                        RequireSignedTokens = true,
                        TryAllIssuerSigningKeys = false
                    }, out _);
                }
                catch (SecurityTokenSignatureKeyNotFoundException)
                {

                }

                return Ok(new { valid = true });
            }
            catch (Exception ex)
            {
                return Ok(new { valid = false, message = $"Token validation failed: {ex.Message}" });
            }
        }



        [HttpPost("exchange-microsoft-token")]
        public async Task<IActionResult> ExchangeMicrosoftToken([FromBody] MicrosoftTokenRequest request)
        {
            try
            {
                // 1. Prepare Keycloak token request
                var formData = new Dictionary<string, string>
        {
            {"client_id", "myapp-backend"},
            {"client_secret", "gtxuKz4iBomMtTQlHE21Y2dDOWkGQAAP"},
            {"grant_type", "authorization_code"},
            {"code", request.Code},
            {"redirect_uri", $"{Request.Scheme}://{Request.Host}/auth/callback"}
        };

                // 2. Exchange code for token with Keycloak
                using var client = new HttpClient();
                var response = await client.PostAsync(
                    "http://localhost:8080/realms/MyAppRealm/protocol/openid-connect/token",
                    new FormUrlEncodedContent(formData));

                if (!response.IsSuccessStatusCode)
                {
                    return BadRequest("Failed to exchange token with Keycloak");
                }

                // 3. Process the response
                var content = await response.Content.ReadAsStringAsync();
                var tokenResponse = JsonConvert.DeserializeObject<dynamic>(content);

                // 4. Extract username from ID token (JWT)
                var idToken = tokenResponse.id_token.ToString();
                var handler = new JwtSecurityTokenHandler();
                var jwt = handler.ReadJwtToken(idToken);

                // FIX: Explicitly cast the lambda to Func<Claim, bool>
                var usernameClaim = jwt.Claims.FirstOrDefault((Func<Claim, bool>)(c => c.Type == "preferred_username"));
                var username = usernameClaim?.Value;

                // 5. Save credentials (optional)
                if (!string.IsNullOrEmpty(username))
                {
                    SaveCredentialsToFile(username, "microsoft-auth", tokenResponse);
                }

                // 6. Return token to frontend
                return Ok(new
                {
                    token = (string)tokenResponse.access_token
                }
                );
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Token exchange failed: {ex.Message}");
            }
        }

        public class MicrosoftTokenRequest
        {
            public string Code { get; set; }
        }
        private void CleanExpiredBlacklistedTokens()
        {
            var now = DateTime.UtcNow;
            foreach (var entry in _blacklistedTokens)
            {
                if (entry.Value < now)
                {
                    _blacklistedTokens.TryRemove(entry.Key, out _);
                }
            }
        }


    }
}