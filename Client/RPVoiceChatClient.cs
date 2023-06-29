﻿using NAudio.Wave;
using rpvoicechat.Client.Utils;
using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using Vintagestory.API.Client;

namespace rpvoicechat
{
    public class RPVoiceChatClient : RPVoiceChatCommon
    {
        IClientNetworkChannel clientChannel;
        ICoreClientAPI clientApi;
        RPAudioInputManager audioInputManager;
        RPAudioOutputManager audioOutputManager;
        RPVoiceChatSocketClient socketClient;



        public override void StartClientSide(ICoreClientAPI api)
        {
            clientApi = api;

            // Register game tick listener
            //listenerId = api.Event.RegisterGameTickListener(OnGameTick, 20);

            // Register clientside connection to the network channel
            clientChannel = clientApi.Network.GetChannel("rpvoicechat")
                .SetMessageHandler<ConnectionPacket>(OnHandshakeRecieved);

            // Custom keybindings
            GlobalKeyboardHook keyboardHook = new GlobalKeyboardHook();
            keyboardHook.KeyDown += OnKeyDown;
            keyboardHook.KeyUp += OnKeyUp;

            // Add a new keybinding
            clientApi.Input.RegisterHotKey("voiceVolume", "Change voice volume", GlKeys.N, HotkeyType.GUIOrOtherControls);
            clientApi.Input.RegisterHotKey("changeMic", "Cycle microphone", GlKeys.N, HotkeyType.GUIOrOtherControls, true);
            clientApi.Input.RegisterHotKey("toggleMute", "Toggle mute", GlKeys.M, HotkeyType.GUIOrOtherControls);

            // Add hotkey handler
            clientApi.Input.SetHotKeyHandler("voiceVolume", ChangeVoiceVolume);
            clientApi.Input.SetHotKeyHandler("changeMic", ChangeMic);
            clientApi.Input.SetHotKeyHandler("toggleMute", ToggleMute);

            // Initialize the socket client
            socketClient = new RPVoiceChatSocketClient(clientApi);
            socketClient.OnAudioPacketReceived += OnAudioReceived;

            // Initialize the audio output manager
            audioOutputManager = new RPAudioOutputManager(clientApi);

            clientApi.Logger.Debug("[RPVoiceChat] Client started");

        }

        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.CapsLock)
            {
                audioInputManager.TogglePushToTalk(false);
            }
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.CapsLock)
            {
                audioInputManager.TogglePushToTalk(true);
            }
        }


        private void OnHandshakeRecieved(ConnectionPacket packet)
        {
            socketClient.ConnectToServer(packet.serverIp);

            // Initialize the audio input manager
            audioInputManager = new RPAudioInputManager(socketClient, clientApi);
            audioInputManager.StartRecording();

            clientApi.Logger.Debug("[RPVoiceChat] Handshake recieved");
        }

        private bool ToggleMute(KeyCombination t1)
        {
            audioInputManager.ToggleMute();
            return true;
        }

        // Function to change the voice volume
        private bool ChangeVoiceVolume(KeyCombination t1)
        {
            VoiceLevel voiceLevel = audioInputManager.CycleVoiceLevel();
            clientApi.ShowChatMessage("Voice volume set to " + Enum.GetName(typeof(VoiceLevel), voiceLevel).ToLower());

            return true;
        }

        private bool ChangeMic(KeyCombination t1)
        {
            WaveInCapabilities device = audioInputManager.CycleInputDevice();
            clientApi.ShowChatMessage("Microphone set to " + device.ProductName);

            return true;
        }

        private void OnGameTick(float dt)
        {
            // If the player is in singleplayer or the player is null, return
            if (clientApi.IsSinglePlayer || clientApi.World.Player == null) return;

            // Check if the player is in a world
            if (clientApi.World == null) return;

            // If the player is recording audio show the audio icon
            if (audioInputManager.isRecording)
            {
                // Get the player characters voice type (Trumpet, Tuba, etc.)

                



                string sound = clientApi.World.Player.Entity.talkUtil.soundName.GetName();
                clientApi.ShowChatMessage("Sound is: " + sound);
            }

        }

        // When client recieves audio data from the server
        public void OnAudioReceived(PlayerAudioPacket packet)
        {
            clientApi.Logger.Debug("[RPVoiceChat - Client] Audio recieved");
            audioOutputManager.HandleAudioPacket(packet);
        }
    }
}
