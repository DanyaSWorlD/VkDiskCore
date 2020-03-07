using System;
using System.Threading.Tasks;
using VkDiskCore.Crypto;
using VkDiskCore.Utility;
using VkNet.AudioBypassService.Utils;
using VkNet.Model;

namespace VkDiskCore.Auth
{
    public class Auth
    {
        private readonly string _login;
        private readonly string _password;
        private Func<string> _action;

        public delegate void ResultDelegate(bool result);

        public Auth(string login, string password)
        {
            this._login = login;
            this._password = password;
        }

        public Auth WithTwoFactor(Func<string> action)
        {
            _action = action;
            return this;
        }

        public async Task LoginAsync()
        {
            await Task.Run(() => Login());
        }

        /// <summary>
        /// Попытка войти с сохраненными данными
        /// </summary>
        /// <returns>успешно или нет</returns>
        public static async Task<bool> TryLoginAsync(Func<string> twoFactor = null)
        {
            return await Task.Run(() => TryLogin(twoFactor));
        }

        public static async Task<bool> TryTokenLoginAsync()
        {
            return await Task.Run(() => TryTokenLogin());
        }

        public void Login()
        {
            VkDisk.VkApi.Authorize(new ApiAuthParams
            {
                //ApplicationId = VkDisk.ApplicationId,
                Login = _login,
                Password = _password,
                //Settings = VkDisk.Settings,
                //TwoFactorAuthorization = _action
            });

            //VkDisk.VkApi.Stats.TrackVisitor();

            if (VkDisk.VkApi.Token != null)
            {
                User.UserId = VkDisk.VkApi.UserId.ToString();
                PrivateDataManager.SaveLoginPass(_login, _password);
                PrivateDataManager.SaveToken(VkDisk.VkApi.Token, DateTime.Now);
            }
        }

        /// <summary>
        /// Попытка войти с сохраненными данными
        /// </summary>
        /// <returns>успешно или нет</returns>
        private static bool TryLogin(Func<string> twoFactor)
        {
            if (PrivateDataManager.Login == null) return false;

            var login = PrivateDataManager.Login;

            try
            {
                VkDisk.VkApi.Authorize(new ApiAuthParams
                {
                    //ApplicationId = VkDisk.ApplicationId,
                    //Settings = VkDisk.Settings,
                    Login = login,
                    Password = PrivateDataManager.Password,
                    //TwoFactorAuthorization = twoFactor
                });
            }
            catch
            {
                return false;
            }

            if (VkDisk.VkApi.Token == null) return false;

            User.UserId = VkDisk.VkApi.UserId.ToString();
            PrivateDataManager.SaveToken(VkDisk.VkApi.Token);

            //VkDisk.VkApi.Stats.TrackVisitor();

            return VkDisk.VkApi.Token != null;
        }

        private static bool TryTokenLogin()
        {
            if (PrivateDataManager.TokenAssigned == null) return false;

            if (PrivateDataManager.TokenExpire != null)
                if (PrivateDataManager.TokenAssigned.Value.AddSeconds(PrivateDataManager.TokenExpire.Value) <
                    DateTime.Now)
                    return false;

            if (PrivateDataManager.TokenAssigned.Value.AddMonths(3) < DateTime.Now)
            {
                var receipt = ((VkAndroidAuthorization)VkDisk.VkApi.AuthorizationFlow).ReceiptParser.GetReceipt().Result;
                var newToken = ((VkAndroidAuthorization)VkDisk.VkApi.AuthorizationFlow).RefreshTokenAsync(PrivateDataManager.Token, receipt).Result;
                PrivateDataManager.SaveToken(newToken);
            }

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

            //if (VkDisk.VkApi.Token == null) return false;

            return VkDisk.VkApi.IsAuthorized;
        }
    }
}
