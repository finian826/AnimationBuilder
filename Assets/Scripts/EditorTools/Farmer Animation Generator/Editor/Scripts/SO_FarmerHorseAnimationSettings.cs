using UnityEngine;
using System.Collections.Generic;

namespace ManaSeedTools.CharacterAnimator
{
    [CreateAssetMenu(fileName = "so_FarmerHorseAnimationSettings", menuName = "Scriptable Objects/Farmer and Horse Animation Settings")]
    public class SO_FarmerHorseAnimationSettings : ScriptableObject
    {
        [SerializeField]
        public List<MSCFarmerHorseAnimation> list;
    }
}
