using UnityEngine;
using System.Collections.Generic;

namespace ManaSeedTools.CharacterAnimator
{
    [CreateAssetMenu(fileName = "so_FarmerAnimationSettings", menuName = "Scriptable Objects/MSC Farmer Animation Settings")]
    public class SO_FarmerAnimationSettings : ScriptableObject
    {
        [SerializeField]
        public List<MSCFarmerAnimation> list;
    }
}
