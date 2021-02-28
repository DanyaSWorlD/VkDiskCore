using System;

using VkDiskCore.Crypto;

using VkNet;
using VkNet.AudioBypassService;
using VkNet.AudioBypassService.Exceptions;
using VkNet.Exception;
using VkNet.Model;
using VkNet.Model.RequestParams;

namespace VkDiskCore.Auth
{
    /// <summary>
    /// Provides everything needed to authenticate
    /// </summary>
    public static class Auth
    {
        /// <summary>
        /// Authorization via login & password, with saving credentials to <see cref="PrivateDataManager"/> (with cipher)
        /// </summary>
        /// <param name="login"> user's login </param>
        /// <param name="password"> user's password </param>
        /// <param name="twoFactor"> func returning user's 2factor code </param>
        /// <returns> is auth successful </returns>
        public static bool WithPass(string login, string password, Func<string> twoFactor = null)
        {
            try
            {
                VkDisk.VkApi.Authorize(
                    new ApiAuthParams
                    {
                        Login = login,
                        Password = password,
                        TwoFactorAuthorization = twoFactor
                    });
            }
            catch (Exception e) when (e is VkApiException || e is VkAuthException)
            {
                // catch wrong login or password exception
                return false;
            }

            // if token is null we might be not authorized 
            if (VkDisk.VkApi.Token == null) return VkDisk.VkApi.IsAuthorized;

            // get user id
            User.UserId = VkDisk.VkApi.UserId.ToString();

            // save login, pass and token
            PrivateDataManager.SaveLoginPass(login, password);
            PrivateDataManager.SaveToken(VkDisk.VkApi.Token, DateTime.Now);

            // return VkApi authorization state
            return VkDisk.VkApi.IsAuthorized;
        }

        /// <summary>
        /// Auth with saved in <see cref="PrivateDataManager"/> login & password />
        /// </summary>
        /// <param name="twoFactor">  func returning user's 2factor code </param>
        /// <returns> is auth successful </returns>
        public static bool WithSavedPass(Func<string> twoFactor = null)
        {
            // if any data is null, auth will fail with exception, so, return false right now
            if (PrivateDataManager.Login == null || PrivateDataManager.Password == null) return false;

            return WithPass(PrivateDataManager.Login, PrivateDataManager.Password, twoFactor);
        }

        /// <summary>
        /// Authorization via token. Token got from <see cref="PrivateDataManager"/>
        /// </summary>
        /// <returns> is auth successful? </returns>
        public static bool WithToken()
        {
            // check token exists
            if (PrivateDataManager.TokenAssigned == null) return false;

            // check token not expired
            if (PrivateDataManager.TokenExpire != null)
                if (PrivateDataManager.TokenAssigned.Value.AddSeconds(PrivateDataManager.TokenExpire.Value) < DateTime.Now)
                    return false;

            // renew token if token expired
            if (PrivateDataManager.TokenAssigned.Value.AddMonths(3) < DateTime.Now)
                UpdateToken();

            // handle on token expired event
            VkDisk.VkApi.OnTokenExpires += UpdateToken;

            // login with token
            try
            {
                VkDisk.VkApi.Authorize(new ApiAuthParams
                {
                    AccessToken = PrivateDataManager.Token
                });

                VkDisk.VkApi.UserId = long.Parse(PrivateDataManager.UserId);
            }
            catch
            {
                return false;
            }

            // renew token if token expired (if we authenticated we should 100% have chance to renew token)
            if (PrivateDataManager.TokenAssigned.Value.AddMonths(3) < DateTime.Now)
                UpdateToken();

            // check token usability
            try
            {
                VkDisk.VkApi.Groups.Get(new GroupsGetParams());
            }
            catch (AccessTokenInvalidException)
            {
                return false;
            }

            // all seems ok, let return vk net auth state
            return VkDisk.VkApi.IsAuthorized;
        }

        /// <summary>
        /// renew token
        /// </summary>
        /// <returns> auth success </returns>
        private static void UpdateToken(VkApi sender = null)
        {
            var receipt = ((VkAndroidAuthorization)VkDisk.VkApi.AuthorizationFlow).ReceiptParser.GetReceipt().Result;
            var newToken = ((VkAndroidAuthorization)VkDisk.VkApi.AuthorizationFlow).RefreshTokenAsync(PrivateDataManager.Token, receipt).Result;
            PrivateDataManager.SaveToken(newToken);
        }
    }
}
