// SPDX-License-Identifier: MIT

using LobotomyCorporationMods.CrowdControl.Enums;

namespace LobotomyCorporationMods.CrowdControl
{
    public class Timed
    {
        private static int frames;
        private float old;
        public TimedType type;

        public Timed(TimedType t)
        {
            type = t;
        }

        public void addEffect()
        {
            switch (type)
            {
                case TimedType.EXAMPLE:
                    {
                        Harmony_Patch.ActionQueue.Enqueue(() =>
                        {
                            //add game code to run when the timed effect starts here
                        });
                        break;
                    }
            }
        }

        public void removeEffect()
        {
            switch (type)
            {
                case TimedType.EXAMPLE:
                    {
                        TestMod.ActionQueue.Enqueue(() =>
                        {
                            //add game code to run when the timed effect ends here
                        });
                        break;
                    }
            }
        }

        public void tick()
        {
            frames++;
            var playerRef = StartOfRound.Instance.localPlayerController;

            switch (type)
            {
                case TimedType.EXAMPLE:
                    //add game code to run every frame while the effect is active if needed
                    break;
            }
        }
    }
}
