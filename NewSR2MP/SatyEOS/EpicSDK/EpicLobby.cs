using Epic.OnlineServices;
using Epic.OnlineServices.Lobby;
using Epic.OnlineServices.RTC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Il2CppSystem;
using Action = Il2CppSystem.Action;

namespace NewSR2MP.EpicSDK
{
    public class EpicLobby
    {
        internal string customServerCode = null;
        
        private LobbyInterface lobbyInterface;

        private ulong? addNotifyLobbyMemberStatusReceivedHandle;
        private ulong? addNotifyLobbyMemberUpdateReceivedHandle;
        private ulong? addNotifyLobbyUpdateReceivedHandle;
        private ulong? addNotifyRTCRoomConnectionChangedHandle;

        public Utf8String LobbyId { get; private set; }
        public bool IsInLobby => LobbyId != null;
        public bool IsLobbyOwner { get; private set; }

        public EpicNetworkClient NetworkClient { get; private set; }
        public EpicNetworkServer NetworkServer { get; private set; }

        public EpicLobby(LobbyInterface lobbyInterface)
        {
            //customServerCode = "TEST";
            
            this.lobbyInterface = lobbyInterface;
        }

        public void Tick()
        {
            NetworkClient?.Tick();
            NetworkServer?.Tick();
        }

        private void RegisterEvents()
        {
            if (!addNotifyLobbyMemberStatusReceivedHandle.HasValue)
            {
                var addNotifyLobbyMemberStatusReceivedOptions = new AddNotifyLobbyMemberStatusReceivedOptions()
                {

                };
                addNotifyLobbyMemberStatusReceivedHandle = lobbyInterface.AddNotifyLobbyMemberStatusReceived(ref addNotifyLobbyMemberStatusReceivedOptions, null, OnLobbyMemberStatusReceived);
            }

            if (!addNotifyLobbyMemberUpdateReceivedHandle.HasValue)
            {
                var notifyLobbyMemberUpdateReceivedOptions = new AddNotifyLobbyMemberUpdateReceivedOptions()
                {

                };
                addNotifyLobbyMemberUpdateReceivedHandle = lobbyInterface.AddNotifyLobbyMemberUpdateReceived(ref notifyLobbyMemberUpdateReceivedOptions, null, OnLobbyMemberUpdateReceived);
            }

            if (!addNotifyLobbyUpdateReceivedHandle.HasValue)
            {
                var addNotifyLobbyUpdateReceivedOptions = new AddNotifyLobbyUpdateReceivedOptions()
                {

                };
                addNotifyLobbyUpdateReceivedHandle = lobbyInterface.AddNotifyLobbyUpdateReceived(ref addNotifyLobbyUpdateReceivedOptions, null, OnLobbyUpdateReceived);
            }

            /*if (!addNotifyRTCRoomConnectionChangedHandle.HasValue)
            {
                var addNotifyRTCRoomConnectionChangedOptions = new AddNotifyRTCRoomConnectionChangedOptions()
                {

                };
                addNotifyRTCRoomConnectionChangedHandle = lobbyInterface.AddNotifyRTCRoomConnectionChanged(ref addNotifyRTCRoomConnectionChangedOptions, null, OnRTCRoomConnectionChanged);
            }

            var getRTCRoomNameOptions = new GetRTCRoomNameOptions()
            {
                LobbyId = LobbyId,
                LocalUserId = EpicApplication.Instance.Authentication.ProductUserId
            };
            var result = lobbyInterface.GetRTCRoomName(ref getRTCRoomNameOptions, out var roomName);
            if (result == Result.Success)
            {
                //EpicApplication.Instance.Voice.RegisterEvents(roomName);
            }
            else
            {
            }*/

            Application.quitting += new System.Action(() => { DestroyLobby();});
        }

        private void UnregisterEvents()
        {
            if (addNotifyLobbyMemberStatusReceivedHandle.HasValue)
            {
                lobbyInterface.RemoveNotifyLobbyMemberStatusReceived(addNotifyLobbyMemberStatusReceivedHandle.Value);
                addNotifyLobbyMemberStatusReceivedHandle = null;
            }
            if (addNotifyLobbyMemberUpdateReceivedHandle.HasValue)
            {
                lobbyInterface.RemoveNotifyLobbyMemberUpdateReceived(addNotifyLobbyMemberUpdateReceivedHandle.Value);
                addNotifyLobbyMemberUpdateReceivedHandle = null;
            }
            if (addNotifyLobbyUpdateReceivedHandle.HasValue)
            {
                lobbyInterface.RemoveNotifyLobbyUpdateReceived(addNotifyLobbyUpdateReceivedHandle.Value);
                addNotifyLobbyUpdateReceivedHandle = null;
            }
            if (addNotifyRTCRoomConnectionChangedHandle.HasValue)
            {
                lobbyInterface.RemoveNotifyRTCRoomConnectionChanged(addNotifyRTCRoomConnectionChangedHandle.Value);
                addNotifyRTCRoomConnectionChangedHandle = null;
            }

            //EpicApplication.Instance.Voice.UnregisterEvents();
        }

        private void OnRTCRoomConnectionChanged(ref RTCRoomConnectionChangedCallbackInfo data)
        {
        }

        private void OnLobbyUpdateReceived(ref LobbyUpdateReceivedCallbackInfo data)
        {
        }

        private void OnLobbyMemberUpdateReceived(ref LobbyMemberUpdateReceivedCallbackInfo data)
        {
        }

        private void OnLobbyMemberStatusReceived(ref LobbyMemberStatusReceivedCallbackInfo data)
        {
            if(data.TargetUserId == EpicApplication.Instance.Authentication.ProductUserId)
            {
                if(data.CurrentStatus == LobbyMemberStatus.Closed)
                {
                    NetworkClient.Shutdown();
                    NetworkClient = null;
                    UnregisterEvents();
                    LobbyId = null;
                }
            }
        }

        public void CreateLobby()
        {
            if(LobbyId != null)
            {
                return;
            }

            var createLobbyOptions = new CreateLobbyOptions()
            {
                BucketId = "SR2MP",
                MaxLobbyMembers = 64,
                AllowInvites = true,
                LobbyId = string.IsNullOrWhiteSpace(customServerCode) ? GenerateServerCode() : customServerCode,
                RejoinAfterKickRequiresInvite = true,
                LocalUserId = EpicApplication.Instance.Authentication.ProductUserId,
                EnableRTCRoom = false,
            };
            
            lobbyInterface.CreateLobby(ref createLobbyOptions, null, OnCreateLobby);
        }

        private void OnCreateLobby(ref CreateLobbyCallbackInfo data)
        {
            if(data.ResultCode != Result.Success)
            {
                SRMP.Error($"Failed to create lobby: Error - {data.ResultCode}");
                return;
            }

            IsLobbyOwner = true;
            LobbyId = data.LobbyId;
            RegisterEvents();

            NetworkServer = EpicApplication.Instance.CreateServer();
            NetworkServer.StartListen();
        }

        public void JoinLobby(string lobbyId)
        {
            if (LobbyId != null)
            {
                return;
            }

            var lobby = lobbyId.ToUpper();

            var joinLobbyOptions = new JoinLobbyByIdOptions()
            {
                CrossplayOptOut = false,
                LocalRTCOptions = new LocalRTCOptions()
                {
                    LocalAudioDeviceInputStartsMuted = false,
                    UseManualAudioInput = false,
                    UseManualAudioOutput = true,
                    Flags = 0
                },
                LocalUserId = EpicApplication.Instance.Authentication.ProductUserId,
                PresenceEnabled = false,
                LobbyId = lobby
            };
            lobbyInterface.JoinLobbyById(ref joinLobbyOptions, null, OnJoinLobby);
        }

        private void OnJoinLobby(ref JoinLobbyByIdCallbackInfo data)
        {
            if(data.ResultCode != Result.Success)
            {
                return;
            }

            IsLobbyOwner = false;
            LobbyId = data.LobbyId;
            RegisterEvents();

            var lobbyDetails = GetLobbyDetails();
            if(lobbyDetails == null)
            {
                return;
            }
            var lobbyDetailsGetLobbyOwnerOptions = new LobbyDetailsGetLobbyOwnerOptions() { };
            var ownerUserId = lobbyDetails.GetLobbyOwner(ref lobbyDetailsGetLobbyOwnerOptions);
            if(ownerUserId == null)
            {
                return;
            }

            NetworkClient = EpicApplication.Instance.CreateClient();
            NetworkClient.Connect(ownerUserId);

            lobbyDetails.Release();
            
            MultiplayerManager.BeginWaitingForSaveData();
        }

        public void LeaveLobby()
        {
            if (!ClientActive())
                return;
            
            var leaveLobbyOptions = new LeaveLobbyOptions()
            {
                LocalUserId = EpicApplication.Instance.Authentication.ProductUserId,
                LobbyId = LobbyId
            };
            lobbyInterface.LeaveLobby(ref leaveLobbyOptions, null, OnLeaveLobby);
        }

        private void OnLeaveLobby(ref LeaveLobbyCallbackInfo data)
        {
            if(data.ResultCode != Result.Success)
            {
                return;
            }

            NetworkClient?.Shutdown();
            NetworkClient = null;
            UnregisterEvents();
            LobbyId = null;
        }

        public void KickMember(ProductUserId targetUserId)
        {
            var kickMemberOptions = new KickMemberOptions()
            {
                LobbyId = LobbyId,
                LocalUserId = EpicApplication.Instance.Authentication.ProductUserId,
                TargetUserId = targetUserId
            };
            lobbyInterface.KickMember(ref kickMemberOptions, null, OnKickMember);
        }

        private void OnKickMember(ref KickMemberCallbackInfo data)
        {
        }

        public void DestroyLobby()
        {
            if (!ServerActive())
                return;
            
            var destroyLobbyOptions = new DestroyLobbyOptions()
            {
                LobbyId = LobbyId,
                LocalUserId = EpicApplication.Instance.Authentication.ProductUserId
            };
            lobbyInterface.DestroyLobby(ref destroyLobbyOptions, null, OnDestroyLobby);
        }

        private void OnDestroyLobby(ref DestroyLobbyCallbackInfo data)
        {
            if (data.ResultCode != Result.Success)
            {
                return;
            }

            NetworkServer.Shutdown();
            NetworkServer = null;
            UnregisterEvents();
            LobbyId = null;
        }

        private LobbyDetails GetLobbyDetails()
        {
            var copyLobbyDetailsHandleOptions = new CopyLobbyDetailsHandleOptions()
            {
                LobbyId = LobbyId,
                LocalUserId = EpicApplication.Instance.Authentication.ProductUserId
            };
            var result = lobbyInterface.CopyLobbyDetailsHandle(ref copyLobbyDetailsHandleOptions, out var lobbyDetails);
            if (result != Result.Success)
            {
                
            }
            return lobbyDetails;
        }

        public bool ContainsUserId(ProductUserId targetUserId)
        {
            
            var lobbyDetails = GetLobbyDetails();
            if (lobbyDetails == null) return false;

            var lobbyDetailsGetMemberCountOptions = new LobbyDetailsGetMemberCountOptions() { };
            for (uint i = 0; i < lobbyDetails.GetMemberCount(ref lobbyDetailsGetMemberCountOptions); i++)
            {
                var lobbyDetailsGetMemberByIndexOptions = new LobbyDetailsGetMemberByIndexOptions() { MemberIndex = i };
                var lobbyMember = lobbyDetails.GetMemberByIndex(ref lobbyDetailsGetMemberByIndexOptions);
                if(lobbyMember != null && lobbyMember == targetUserId)
                {
                    lobbyDetails.Release();
                    return true;
                }
            }
            lobbyDetails.Release();
            return false;
        }
    }
}
