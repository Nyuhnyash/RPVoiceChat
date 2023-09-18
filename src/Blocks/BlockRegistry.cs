﻿using Vintagestory.API.Common;

namespace RPVoiceChat.Blocks
{
    public class BlockRegistry
    {
        public static void RegisterBlocks(ICoreAPI api)
        {
            api.RegisterBlockClass("soundemittingblock", typeof(SoundEmittingBlock));
            api.RegisterBlockClass("churchbellpart", typeof(ChurchBellPartBlock));
            api.RegisterBlockClass("churchbelllayer", typeof(ChurchBellLayerBlock));
        }
    }
}
