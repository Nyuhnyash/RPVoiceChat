﻿using RPVoiceChat.Networking;
using RPVoiceChat.Server;
using Vintagestory.API.Common;
using Vintagestory.API.Common.CommandAbbr;
using Vintagestory.API.Server;

namespace RPVoiceChat
{
    public class RPVoiceChatServer : RPVoiceChatMod
    {
        protected ICoreServerAPI sapi;
        private GameServer server;
        public override void StartServerSide(ICoreServerAPI api)
        {
            sapi = api;

            var mainServer = new UDPNetworkServer(ModConfig.Config.ServerPort, ModConfig.Config.ServerIP);
            if (ModConfig.Config.ManualPortForwarding) mainServer.TogglePortForwarding(false);
            var backupServer = new NativeNetworkServer(api);
            server = new GameServer(sapi, mainServer, backupServer);
            server.Launch();
            
            // Register/load world config
            sapi.World.Config.SetInt("rpvoicechat:distance-whisper", sapi.World.Config.GetInt("rpvoicechat:distance-whisper", (int)VoiceLevel.Whispering));
            sapi.World.Config.SetInt("rpvoicechat:distance-talk", sapi.World.Config.GetInt("rpvoicechat:distance-talk", (int)VoiceLevel.Talking));
            sapi.World.Config.SetInt("rpvoicechat:distance-shout", sapi.World.Config.GetInt("rpvoicechat:distance-shout", (int)VoiceLevel.Shouting));

            // Register commands
            registerCommands();
        }

        public override bool ShouldLoad(EnumAppSide forSide)
        {
            return forSide == EnumAppSide.Server;
        }

        private void registerCommands()
        {
            var parsers = sapi.ChatCommands.Parsers;

            sapi.ChatCommands
                .GetOrCreate("rpvc")
                .RequiresPrivilege(Privilege.controlserver)
                .BeginSub("shout")
                    .WithDesc("Sets the shout distance in blocks")
                    .WithArgs(parsers.Int("distance"))
                    .HandleWith(SetShoutHandler)
                .EndSub()
                .BeginSub("talk")
                    .WithDesc("Sets the talk distance in blocks")
                    .WithArgs(parsers.Int("distance"))
                    .HandleWith(SetTalkHandler)
                .EndSub()
                .BeginSub("whisper")
                    .WithDesc("Sets the whisper distance in blocks")
                    .WithArgs(parsers.Int("distance"))
                    .HandleWith(SetWhisperHandler)
                .EndSub()
                .BeginSub("info")
                    .WithDesc("Displays the current audio distances")
                    .HandleWith(DisplayInfoHandler)
                .EndSub()
                .BeginSub("reset")
                    .WithDesc("Resets the audio distances to their default settings")
                    .HandleWith(ResetDistanceHandler)
                .EndSub()
                .BeginSub("voiptoggle")
                    .WithDesc("Toggles voip functionality on the server")
                    .WithArgs(parsers.Bool("toggle"))
                    .HandleWith(ToggleVoip)
                .EndSub();
        }

        private TextCommandResult ToggleVoip(TextCommandCallingArgs args)
        {
            bool toggle = (bool)args[0];

            if (server.ServerToggle(toggle))
                return TextCommandResult.Success("Voip toggled " + (toggle ? "on" : "off"));
            else
                return TextCommandResult.Error("Voip is already " + (toggle ? "on" : "off") + "!");
        }
        private TextCommandResult ResetDistanceHandler(TextCommandCallingArgs args)
        {
            sapi.World.Config.SetInt("rpvoicechat:distance-whisper", (int)VoiceLevel.Whispering);
            sapi.World.Config.SetInt("rpvoicechat:distance-talk", (int)VoiceLevel.Talking);
            sapi.World.Config.SetInt("rpvoicechat:distance-shout", (int)VoiceLevel.Shouting);

            return TextCommandResult.Success("Audio distances reset to default");
        }
        
        private TextCommandResult DisplayInfoHandler(TextCommandCallingArgs args)
        {
            int whisper = sapi.World.Config.GetInt("rpvoicechat:distance-whisper", (int)VoiceLevel.Whispering);
            int talk = sapi.World.Config.GetInt("rpvoicechat:distance-talk", (int)VoiceLevel.Talking);
            int shout = sapi.World.Config.GetInt("rpvoicechat:distance-shout", (int)VoiceLevel.Shouting);

            return TextCommandResult.Success
                (
                    "Whisper distance: " + whisper + " blocks\n" +
                    "Talk distance: " + talk + " blocks\n" +
                    "Shout distance: " + shout + " blocks"
                );
        }

        private TextCommandResult SetWhisperHandler(TextCommandCallingArgs args)
        {
            int distance = (int)args[0];

            sapi.World.Config.SetInt("rpvoicechat:distance-whisper", distance);

            return TextCommandResult.Success("Whisper distance set to " + distance);
        }

        private TextCommandResult SetTalkHandler(TextCommandCallingArgs args)
        {
            int distance = (int)args[0];

            sapi.World.Config.SetInt("rpvoicechat:distance-talk", distance);

            return TextCommandResult.Success("Talking distance set to " + distance);
        }

        private TextCommandResult SetShoutHandler(TextCommandCallingArgs args)
        {
            int distance = (int)args[0];

            sapi.World.Config.SetInt("rpvoicechat:distance-shout", distance);

            return TextCommandResult.Success("Shout distance set to " + distance);
        }

        public override void Dispose()
        {
            server?.Dispose();
        }
    }
}
