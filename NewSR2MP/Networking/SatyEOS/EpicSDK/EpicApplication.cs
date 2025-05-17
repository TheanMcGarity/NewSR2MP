using Epic.OnlineServices;
using Epic.OnlineServices.Connect;
using Epic.OnlineServices.IntegratedPlatform;
using Epic.OnlineServices.Logging;
using Epic.OnlineServices.P2P;
using Epic.OnlineServices.Platform;
using Epic.OnlineServices.RTCAudio;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NewSR2MP.EpicSDK
{
    [RegisterTypeInIl2Cpp(false)]
    public class EpicApplication : MonoBehaviour
    {
        void Awake() => Instance = this;
        public static EpicApplication Instance;
        [DllImport("Kernel32.dll")]
        private static extern IntPtr LoadLibrary(string lpLibFileName);

        [DllImport("Kernel32.dll")]
        private static extern int FreeLibrary(IntPtr hLibModule);

        [DllImport("Kernel32.dll")]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

        private PlatformInterface epicPlatformInterface;
        
        public EpicAuthentication Authentication { get; private set; }
        public EpicMetrics Metrics { get; private set; }
        public EpicLobby Lobby { get; private set; }
        //public EpicVoice Voice { get; private set; }

        private void Start()
        {
            epicPlatformInterface = Initialize(PlatformFlags.DisableOverlay);

            if(epicPlatformInterface != null)
            {
                Authentication = new EpicAuthentication(epicPlatformInterface.GetConnectInterface());
                Metrics = new EpicMetrics(epicPlatformInterface.GetMetricsInterface());
                Lobby = new EpicLobby(epicPlatformInterface.GetLobbyInterface());
                //Voice = new EpicVoice(epicPlatformInterface.GetRTCInterface());
                
                
            }
        }

        private void Update()
        {
            epicPlatformInterface?.Tick();
            Lobby?.Tick();
        }

        private void OnApplicationQuit()
        {
            Shutdown();
        }

        internal void Shutdown()
        {
            if(Lobby.IsInLobby)
            {
                if(Lobby.IsLobbyOwner)
                {
                    Lobby.DestroyLobby();
                }
                else
                {
                    Lobby.LeaveLobby();
                }
            }

            epicPlatformInterface?.Release();
            epicPlatformInterface = null;

            PlatformInterface.Shutdown();
        }

        private PlatformInterface Initialize(PlatformFlags platformFlags = PlatformFlags.None)
        {

            var libraryPointer = LoadLibrary(EOS_SDK_PATH);
            if (libraryPointer == IntPtr.Zero)
            {
                throw new Exception($"Failed to load library from {EOS_SDK_PATH}");
            }

            Bindings.Hook(libraryPointer, GetProcAddress);
            WindowsBindings.Hook(libraryPointer, GetProcAddress);

            var initializeOptions = new InitializeOptions()
            {
                ProductName = "SRMPTest",
                ProductVersion = $"1.0.{Globals.Version}"
            };

            Result initializeResult = PlatformInterface.Initialize(ref initializeOptions);

            LoggingInterface.SetLogLevel(LogCategory.AllCategories, LogLevel.VeryVerbose);
            LoggingInterface.SetCallback((ref LogMessage message) => SRMP.Debug(message.Message));
            
            var options = new WindowsOptions()
            {
                ProductId = "5cabbf45e03042e9b93f40449849c50d",
                SandboxId = "dfcf7f2faa004223b14b04d7f5aaeac1",
                ClientCredentials = new ClientCredentials()
                {
                    ClientId = "xyza7891vEIt18NTG5woeNE6E7eKG7Yr",
                    ClientSecret = "AaAXPKUogRkC6C0J4N5512Ye81vzr+jJ2zV7nytjptU"
                },
                DeploymentId = "85657bc0947f45b8976082409f60b3ad",
                Flags = platformFlags,
                IsServer = false,
                //RTCOptions = new WindowsRTCOptions()
                //{
                //    BackgroundMode = RTCBackgroundMode.KeepRoomsAlive,
                //    PlatformSpecificOptions = new WindowsRTCOptionsPlatformSpecificOptions()
                //    {
                //        XAudio29DllPath = eosAudioPath
                //    }
                //}
            };

            PlatformInterface platformInterface = PlatformInterface.Create(ref options);

            if (platformInterface == null)
            {
                SRMP.Error("Platform interface could not be created!");
            }

            return platformInterface;
        }

        public EpicNetworkClient CreateClient()
        {
            return new EpicNetworkClient(epicPlatformInterface.GetP2PInterface());
        }

        public EpicNetworkServer CreateServer()
        {
            return new EpicNetworkServer(epicPlatformInterface.GetP2PInterface());
        }
    }
}
