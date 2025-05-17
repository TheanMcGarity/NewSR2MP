using Epic.OnlineServices;
using Epic.OnlineServices.Connect;
using Epic.OnlineServices.Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewSR2MP.EpicSDK
{
    public class EpicAuthentication
    {
        private ConnectInterface connectInterface;
        private ulong? connectNotifyLoginStatusChangedId;
        private ulong? connectNotifyAuthExpirationId;

        public bool DeviceCreated { get; private set; }
        public bool IsLoggedIn { get; private set; }
        public string Username { get; private set; } = "NoUsername";
        public ProductUserId ProductUserId { get; private set; }

        public EpicAuthentication(ConnectInterface connectInterface) 
        { 
            this.connectInterface = connectInterface;

            DeviceCreated = false;

            var createDeviceIdOptions = new CreateDeviceIdOptions()
            {
                DeviceModel = "PC Windows"
            };
            connectInterface.CreateDeviceId(ref createDeviceIdOptions, null, OnCreateDeviceId);
        }

        public void Login(string username)
        {
            if (!DeviceCreated)
            {
                return;
            }
            if(IsLoggedIn)
            {
                return;
            }

            Username = username;

            var connectLoginOptions = new Epic.OnlineServices.Connect.LoginOptions()
            {
                UserLoginInfo = new Epic.OnlineServices.Connect.UserLoginInfo()
                {
                    DisplayName = Username
                },
                Credentials = new Epic.OnlineServices.Connect.Credentials()
                {
                    Type = ExternalCredentialType.DeviceidAccessToken,
                    Token = null
                }
            };

            connectInterface.Login(ref connectLoginOptions, null, OnConnectLogin);
        }

        public void Logout()
        {
            if(!IsLoggedIn)
            {
                return;
            }

            var logoutOptions = new LogoutOptions()
            {
                LocalUserId = ProductUserId
            };
            connectInterface.Logout(ref logoutOptions, null, OnLogout);
        }

        private void OnLogout(ref LogoutCallbackInfo data)
        {
            if (data.ResultCode == Result.Success)
            {
                IsLoggedIn = false;
            }
            else if (Common.IsOperationComplete(data.ResultCode))
            {
            }
        }

        private void OnCreateDeviceId(ref CreateDeviceIdCallbackInfo data)
        {
            if (data.ResultCode == Result.Success || data.ResultCode == Result.DuplicateNotAllowed)
            {
                DeviceCreated = true;
            }
            else if (Common.IsOperationComplete(data.ResultCode))
            {
            }
        }

        private void OnConnectLogin(ref Epic.OnlineServices.Connect.LoginCallbackInfo loginCallbackInfo)
        {

            if (loginCallbackInfo.ResultCode == Result.Success)
            {
                ProductUserId = loginCallbackInfo.LocalUserId;

                if (!connectNotifyLoginStatusChangedId.HasValue)
                {
                    var addNotifyLoginStatusChangedOptions = new Epic.OnlineServices.Connect.AddNotifyLoginStatusChangedOptions();
                    var notifyId = connectInterface.AddNotifyLoginStatusChanged(ref addNotifyLoginStatusChangedOptions, null, OnConnectLoginStatusChanged);
                    if (notifyId != Common.InvalidNotificationid)
                    {
                        connectNotifyLoginStatusChangedId = notifyId;
                    }
                }

                if (!connectNotifyAuthExpirationId.HasValue)
                {
                    var addNotifyAuthExpirationOptions = new Epic.OnlineServices.Connect.AddNotifyAuthExpirationOptions();
                    var notifyId = connectInterface.AddNotifyAuthExpiration(ref addNotifyAuthExpirationOptions, null, OnConnectAuthExpiration);
                    if (notifyId != Common.InvalidNotificationid)
                    {
                        connectNotifyAuthExpirationId = notifyId;
                    }
                }

                IsLoggedIn = true;
            }
            else if (loginCallbackInfo.ResultCode == Result.InvalidUser)
            {
                var createUserOptions = new Epic.OnlineServices.Connect.CreateUserOptions()
                {
                    ContinuanceToken = loginCallbackInfo.ContinuanceToken
                };

                connectInterface.CreateUser(ref createUserOptions, null, OnCreateUser);
            }
            else if (Common.IsOperationComplete(loginCallbackInfo.ResultCode))
            {
            }
        }

        private void OnConnectAuthExpiration(ref AuthExpirationCallbackInfo authExpirationCallbackInfo)
        {
            IsLoggedIn = false;

            Login(Username);
        }

        private void OnConnectLoginStatusChanged(ref Epic.OnlineServices.Connect.LoginStatusChangedCallbackInfo loginStatusChangedCallbackInfo)
        {
            if (ProductUserId == loginStatusChangedCallbackInfo.LocalUserId)
            {
                if (loginStatusChangedCallbackInfo.PreviousStatus == LoginStatus.LoggedIn && loginStatusChangedCallbackInfo.CurrentStatus == LoginStatus.NotLoggedIn)
                {
                    IsLoggedIn = false;

                    if (connectNotifyLoginStatusChangedId.HasValue)
                    {
                        connectInterface.RemoveNotifyLoginStatusChanged(connectNotifyLoginStatusChangedId.Value);
                        connectNotifyLoginStatusChangedId = null;
                    }

                    if (connectNotifyAuthExpirationId.HasValue)
                    {
                        connectInterface.RemoveNotifyLoginStatusChanged(connectNotifyAuthExpirationId.Value);
                        connectNotifyAuthExpirationId = null;
                    }
                }
            }
        }

        private void OnCreateUser(ref CreateUserCallbackInfo createUserCallbackInfo)
        {

            if (createUserCallbackInfo.ResultCode == Result.Success)
            {
                Login(Username);
            }
            else if (Common.IsOperationComplete(createUserCallbackInfo.ResultCode))
            {
                
            }
        }
    }
}
