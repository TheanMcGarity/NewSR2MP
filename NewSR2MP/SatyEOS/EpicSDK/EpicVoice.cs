/*using Epic.OnlineServices;
using Epic.OnlineServices.RTC;
using Epic.OnlineServices.RTCAudio;
using NewSR2MP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Events;
using static UnityEngine.Windows.WebCam.VideoCapture;

namespace NewSR2MP.EpicSDK
{
    public class EpicVoice
    {
        private RTCInterface rtcInterface;

        private Utf8String currentRoomName;
        private ulong? notifyAudioBeforeRenderHandle;
        private ulong? notifyAudioDevicesChangedHandle;
        private ulong? notifyParticipantUpdatedHandle;
        private ulong? notifyParticipantStatusChangedHandle;
        private Utf8String currentAudioInputDevice;
        private Utf8String currentAudioOutputDevice;

        public UnityAction<ProductUserId, AudioBuffer> OnAudioReceived;
        public List<InputDeviceInformation> InputDevices { get; private set; }
        public List<OutputDeviceInformation> OutputDevices { get; private set; }
        public RTCAudioStatus AudioStatus { get; private set; }
        public InputDeviceInformation? CurrentInputDevice => currentAudioInputDevice == null ? new InputDeviceInformation?() : InputDevices.FirstOrDefault(o => o.DeviceId == currentAudioInputDevice);
        public OutputDeviceInformation? CurrentOutputDevice => currentAudioOutputDevice == null ? new OutputDeviceInformation?() : OutputDevices.FirstOrDefault(o => o.DeviceId == currentAudioOutputDevice);


        public EpicVoice(RTCInterface rtcInterface)
        {
            this.rtcInterface = rtcInterface;

            InputDevices = new List<InputDeviceInformation>();
            OutputDevices = new List<OutputDeviceInformation>();
        }

        public void RegisterEvents(Utf8String roomName)
        {
            ModEntry.Log(roomName);

            currentRoomName = roomName;

            if (!notifyParticipantStatusChangedHandle.HasValue)
            {
                var addNotifyParticipantStatusChangedOptions = new AddNotifyParticipantStatusChangedOptions()
                {
                    LocalUserId = EpicApplication.Instance.Authentication.ProductUserId,
                    RoomName = roomName
                };
                notifyParticipantStatusChangedHandle = rtcInterface.AddNotifyParticipantStatusChanged(ref addNotifyParticipantStatusChangedOptions, null, OnParticipantStatusChanged);
            }

            if (!notifyParticipantUpdatedHandle.HasValue)
            {
                var addNotifyParticipantUpdatedOptions = new AddNotifyParticipantUpdatedOptions()
                {
                    LocalUserId = EpicApplication.Instance.Authentication.ProductUserId,
                    RoomName = roomName
                };
                notifyParticipantUpdatedHandle = rtcInterface.GetAudioInterface().AddNotifyParticipantUpdated(ref addNotifyParticipantUpdatedOptions, null, OnParticipantUpdated);
            }

            if (!notifyAudioBeforeRenderHandle.HasValue)
            {
                var addNotifyAudioBeforeRenderOptions = new AddNotifyAudioBeforeRenderOptions()
                {
                    LocalUserId = EpicApplication.Instance.Authentication.ProductUserId,
                    UnmixedAudio = true,
                    RoomName = roomName
                };
                notifyAudioBeforeRenderHandle = rtcInterface.GetAudioInterface().AddNotifyAudioBeforeRender(ref addNotifyAudioBeforeRenderOptions, null, OnAudioBeforeRender);
            }
            if(!notifyAudioDevicesChangedHandle.HasValue)
            {
                var addNotifyAudioDevicesChangedOptions = new AddNotifyAudioDevicesChangedOptions()
                {

                };
                notifyAudioDevicesChangedHandle = rtcInterface.GetAudioInterface().AddNotifyAudioDevicesChanged(ref addNotifyAudioDevicesChangedOptions, null, OnAudioDevicesChanged);
            }

            UpdateSending(RTCAudioStatus.Disabled);

            InputDevices.Clear();
            OutputDevices.Clear();

            var queryInputDevicesInformationOptions = new QueryInputDevicesInformationOptions()
            {

            };
            rtcInterface.GetAudioInterface().QueryInputDevicesInformation(ref queryInputDevicesInformationOptions, null, OnInputDevicesInformation);
            var queryOutputDevicesInformationOptions = new QueryOutputDevicesInformationOptions()
            {

            };
            rtcInterface.GetAudioInterface().QueryOutputDevicesInformation(ref queryOutputDevicesInformationOptions, null, OnOutputDevicesInformation);
        }

        private void OnParticipantStatusChanged(ref ParticipantStatusChangedCallbackInfo data)
        {
            ModEntry.Log(data.ParticipantId, data.ParticipantStatus);
        }

        private void OnParticipantUpdated(ref ParticipantUpdatedCallbackInfo data)
        {
            if(PlayerManager.TryGetPlayer(data.ParticipantId, out var player))
            {
                player.UpdateVoiceStatus(data.AudioStatus, data.Speaking);
            }
            //ModEntry.Log(data.ParticipantId, data.AudioStatus, data.Speaking);
        }

        public void UnregisterEvents()
        {
            ModEntry.Log();
            if (notifyParticipantStatusChangedHandle.HasValue)
            {
                rtcInterface.RemoveNotifyParticipantStatusChanged(notifyParticipantStatusChangedHandle.Value);
                notifyParticipantStatusChangedHandle = null;
            }
            if (notifyParticipantUpdatedHandle.HasValue)
            {
                rtcInterface.GetAudioInterface().RemoveNotifyParticipantUpdated(notifyParticipantUpdatedHandle.Value);
                notifyParticipantUpdatedHandle = null;
            }
            if (notifyAudioDevicesChangedHandle.HasValue)
            {
                rtcInterface.GetAudioInterface().RemoveNotifyAudioDevicesChanged(notifyAudioDevicesChangedHandle.Value);
                notifyAudioDevicesChangedHandle = null;
            }
            if (notifyAudioBeforeRenderHandle.HasValue)
            {
                rtcInterface.GetAudioInterface().RemoveNotifyAudioBeforeRender(notifyAudioBeforeRenderHandle.Value);
                notifyAudioBeforeRenderHandle = null;
            }
        }

        private void OnAudioDevicesChanged(ref AudioDevicesChangedCallbackInfo data)
        {
            InputDevices.Clear();
            OutputDevices.Clear();

            var queryInputDevicesInformationOptions = new QueryInputDevicesInformationOptions()
            {

            };
            rtcInterface.GetAudioInterface().QueryInputDevicesInformation(ref queryInputDevicesInformationOptions, null, OnInputDevicesInformation);
            var queryOutputDevicesInformationOptions = new QueryOutputDevicesInformationOptions()
            {

            };
            rtcInterface.GetAudioInterface().QueryOutputDevicesInformation(ref queryOutputDevicesInformationOptions, null, OnOutputDevicesInformation);
        }

        private void OnOutputDevicesInformation(ref OnQueryOutputDevicesInformationCallbackInfo data)
        {
            ModEntry.Log($"Result -> {data.ResultCode}");
            if (data.ResultCode != Result.Success)
            {
                return;
            }

            var getOutputDevicesCountOptions = new GetOutputDevicesCountOptions() { };
            var outputDevicesCount = rtcInterface.GetAudioInterface().GetOutputDevicesCount(ref getOutputDevicesCountOptions);
            for (uint i = 0; i < outputDevicesCount; i++)
            {
                var copyOutputDeviceInformationByIndexOptions = new CopyOutputDeviceInformationByIndexOptions() { DeviceIndex = i };
                var result = rtcInterface.GetAudioInterface().CopyOutputDeviceInformationByIndex(ref copyOutputDeviceInformationByIndexOptions, out var outOutputDeviceInformation);
                if (result == Result.Success)
                {
                    if (outOutputDeviceInformation.HasValue)
                    {
                        OutputDevices.Add(outOutputDeviceInformation.Value);

                        if(currentAudioOutputDevice == null && outOutputDeviceInformation.Value.DefaultDevice)
                        {
                            currentAudioOutputDevice = outOutputDeviceInformation.Value.DeviceId;
                        }
                    }
                }
                else
                {
                    ModEntry.Log($"CopyOutputDeviceInformationByIndex {i} -> {data.ResultCode}");
                }
            }
        }

        private void OnInputDevicesInformation(ref OnQueryInputDevicesInformationCallbackInfo data)
        {
            ModEntry.Log($"Result -> {data.ResultCode}");
            if (data.ResultCode != Result.Success)
            {
                return;
            }

            var getInputDevicesCountOptions = new GetInputDevicesCountOptions() { };
            var inputDevicesCount = rtcInterface.GetAudioInterface().GetInputDevicesCount(ref getInputDevicesCountOptions);
            for (uint i = 0; i < inputDevicesCount; i++)
            {
                var copyInputDeviceInformationByIndexOptions = new CopyInputDeviceInformationByIndexOptions() { DeviceIndex = i };
                var result = rtcInterface.GetAudioInterface().CopyInputDeviceInformationByIndex(ref copyInputDeviceInformationByIndexOptions, out var outInputDeviceInformation);
                if(result == Result.Success)
                {
                    if(outInputDeviceInformation.HasValue)
                    {
                        InputDevices.Add(outInputDeviceInformation.Value);

                        if(currentAudioInputDevice == null && outInputDeviceInformation.Value.DefaultDevice)
                        {
                            currentAudioInputDevice = outInputDeviceInformation.Value.DeviceId;
                        }
                    }
                }
                else
                {
                    ModEntry.Log($"CopyInputDeviceInformationByIndex {i} -> {data.ResultCode}");
                }
            }
        }

        public void ChangeInputDevice(Utf8String realDeviceId)
        {
            var setInputDeviceSettingsOptions = new SetInputDeviceSettingsOptions()
            {
                PlatformAEC = true,
                LocalUserId = EpicApplication.Instance.Authentication.ProductUserId,
                RealDeviceId = realDeviceId
            };
            rtcInterface.GetAudioInterface().SetInputDeviceSettings(ref setInputDeviceSettingsOptions, null, OnSetInputDeviceSettings);
        }

        private void OnSetInputDeviceSettings(ref OnSetInputDeviceSettingsCallbackInfo data)
        {
            ModEntry.Log($"Result -> {data.ResultCode}");
            if (data.ResultCode == Result.Success)
            {
                currentAudioInputDevice = data.RealDeviceId;
            }
        }

        public void ChangeOutputDevice(Utf8String realDeviceId)
        {
            var setOutputDeviceSettingsOptions = new SetOutputDeviceSettingsOptions()
            {
                LocalUserId = EpicApplication.Instance.Authentication.ProductUserId,
                RealDeviceId = realDeviceId
            };
            rtcInterface.GetAudioInterface().SetOutputDeviceSettings(ref setOutputDeviceSettingsOptions, null, OnSetOutputDeviceSettings);
        }

        private void OnSetOutputDeviceSettings(ref OnSetOutputDeviceSettingsCallbackInfo data)
        {
            ModEntry.Log($"Result -> {data.ResultCode}");
            if(data.ResultCode == Result.Success)
            {
                currentAudioOutputDevice = data.RealDeviceId;
            }
        }

        public void UpdateSending(RTCAudioStatus status)
        {
            var updateSendingOptions = new UpdateSendingOptions()
            {
                AudioStatus = status,
                LocalUserId = EpicApplication.Instance.Authentication.ProductUserId,
                RoomName = currentRoomName
            };
            rtcInterface.GetAudioInterface().UpdateSending(ref updateSendingOptions, null, OnUpdateSending);
        }

        private void OnUpdateSending(ref UpdateSendingCallbackInfo data)
        {
            ModEntry.Log($"Result -> {data.ResultCode}");
            if (data.ResultCode == Result.Success)
            {
                AudioStatus = data.AudioStatus;
            }
        }

        private void OnAudioBeforeRender(ref AudioBeforeRenderCallbackInfo data)
        {
            // This call is not thread safe.
            // Push everything into a queue and process it on the Unity Main Thread
            //Console.WriteLine($"{data.ParticipantId} {(data.Buffer.HasValue ? $"{data.Buffer.Value.SampleRate} {data.Buffer.Value.Channels} {data.Buffer.Value.Frames.Length}" : "No Buffer")}");
            if(data.Buffer.HasValue)
            {
                OnAudioReceived?.Invoke(data.ParticipantId, data.Buffer.Value);
            }
        }
    }
}
*/