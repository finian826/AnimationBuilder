using System;

namespace ManaSeedTools.CharacterAnimator
{
    [Serializable]
    public class MSCFarmerAnimation 
    {
        public string animationType;

        //body,outfit, etc..
        public string animationName;

        //animation key ints
        public int[] keys;

        //animation key xflips
        public bool[] keyXFlips;

        //animation timers floats
        public float[] keyTimer;

        //xflip of sprite
        public bool xFlip = false;
    }
}
