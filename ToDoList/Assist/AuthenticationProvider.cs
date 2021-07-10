using Microsoft.Graph;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace ToDoList.Assist
{
    public class AuthenticationProvider : IAuthenticationProvider
    {
        private IPublicClientApplication _msalClient;
        private string[] _scopes;

        private IAccount _userAccount;

        private AuthenticationResult authResult { get; set; }

        public AuthenticationProvider(string appId, string[] scopes)
        {
            _scopes = scopes;

            _msalClient = PublicClientApplicationBuilder.Create(appId)
                .WithAuthority("https://login.microsoftonline.com/common")
                .WithLogging((level, message, containsPii) =>
                {
                    Debug.WriteLine($"MSAL: {level} {message} ");
                }, LogLevel.Warning, enablePiiLogging: false, enableDefaultPlatformLogging: true)
                .WithUseCorporateNetwork(true)
                .WithRedirectUri("https://login.microsoftonline.com/common/oauth2/nativeclient")
                .Build();

            authResult = null;
        }

        public async Task<string> GetAccessToken()
        {
            IEnumerable<IAccount> accounts = await _msalClient.GetAccountsAsync().ConfigureAwait(false);
            IAccount firstAccount = accounts.FirstOrDefault();

            if (_userAccount == null)
            {
                try
                {
                    authResult = await _msalClient.AcquireTokenSilent(_scopes, firstAccount)
                                                      .ExecuteAsync();
                    _userAccount = authResult.Account;
                    return authResult.AccessToken;
                }
                catch (MsalUiRequiredException ex)
                {

                    System.Diagnostics.Debug.WriteLine($"MsalUiRequiredException: {ex.Message}");

                    try
                    {
                        authResult = await _msalClient.AcquireTokenInteractive(_scopes)
                                                          .ExecuteAsync();
                    }
                    catch (MsalException msalex)
                    {
                        if (msalex.ErrorCode == MsalError.AuthenticationCanceledError)
                            Debug.WriteLine($"MsalException, Authentication Canceled:{System.Environment.NewLine}{msalex}");
                        if (msalex.ErrorCode == MsalError.AuthenticationFailed)
                            Debug.WriteLine($"MsalException, Authentication Failed:{System.Environment.NewLine}{msalex}");
                        else if (msalex.ErrorCode == MsalError.RequestTimeout)
                            Debug.WriteLine($"MsalException, Request Timeout:{System.Environment.NewLine}{msalex}");
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    Debug.Write("ERROR: " + ex.Message);
                    return null;
                }

                return authResult == null ? "" : authResult.AccessToken;
            }
            else // Account Exists
            {

                var result = await _msalClient
                    .AcquireTokenSilent(_scopes, _userAccount)
                    .ExecuteAsync();

                return result.AccessToken;
            }
        }

        public async Task AuthenticateRequestAsync(HttpRequestMessage requestMessage)
        {
            requestMessage.Headers.Authorization =
                new AuthenticationHeaderValue("bearer", await GetAccessToken());
        }

        public async Task SignOut()
        {
            IEnumerable<IAccount> accounts = await _msalClient.GetAccountsAsync().ConfigureAwait(false);
            IAccount firstAccount = accounts.FirstOrDefault();

            try
            {
                if (firstAccount != null)
                {
                    await _msalClient.RemoveAsync(firstAccount).ConfigureAwait(false);
                    _userAccount = null;
                }
            }
            catch (MsalException ex)
            {
                System.Diagnostics.Debug.WriteLine($"MsalException, Error signing-out user: {ex.Message}");
                throw;
            }
        }
    }
}
