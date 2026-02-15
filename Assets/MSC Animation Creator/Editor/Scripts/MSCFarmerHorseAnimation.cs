using System;
using UnityEngine;

namespace ManaSeedTools.CharacterAnimator
{

    [Serializable]
    public class MSCFarmerHorseAnimation 
    {
        public string animationType;

        //body,outfit, etc..
        public string animationName;
        [Header("Bottom Layer Horse Details")]
        public string bottomCategory;

        //animation key ints
        public string[] bottomHorseKeyLabels;

        //animation key xflips
        public bool[] bottomHorseKeyXFlips;

        //animation timers floats
        public float[] bottomHorsekeyTimer;

        [Header("Top Layer Horse Details")]
        public string topCategory;

        //animation key ints
        public string[] topHorseKeyLabels;

        //animation key xflips
        public bool[] topHorseKeyXFlips;

        //animation timers floats
        public float[] topHorsekeyTimer;

        public bool horseXFlip = false;

        [Header("Farmer Sprite Info")]
        public bool buildFarmer;
        //animation key ints
        public int[] farmerKeys;
        [Header("Sprite offset in pixels")]
        public Vector2Int farmerFrameOffset;

        //animation key xflips
        public bool[] farmerKeyXFlips;

        //animation timers floats
        public float[] farmerKeyTimer;

        //xflip of sprite
        public bool farmerXFlip = false;


    }
}