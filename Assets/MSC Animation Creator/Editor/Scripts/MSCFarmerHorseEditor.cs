using System;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.U2D.Animation;

namespace ManaSeedTools.CharacterAnimator
{

    public class MSCFarmerHorseEditor : EditorWindow
    {
        private float _space = 5f;

        private string spritePath = "Sprites/Character/Spritesheets/";
        private string spriteSavePath = "Sprites/Character/Spritesheets/";
        private string spritePathTexture = "Sprites/Character/Output Texture/";
        private string spriteSaveTexture = "Sprites/Character/Output Texture/";
        private string animationControllerPath = "Animations/Player/AnimationController/";
        private string animationControllerSavePath = "Animations/Player/AnimationController/";
        private string animationPlayerPath = "Animations/Player/Animations/";
        private string animationPlayerSavePath = "Animations/Player/Animations/";
        private string animationHorsePath = "Animations/Player/Animations/";
        private string animationHorseSavePath = "Animations/Player/Animations/";

        private DefaultAsset spriteFolder = null;
        private DefaultAsset spriteOutputFolder = null;
        private DefaultAsset animationHorseFolder = null;
        private DefaultAsset animationPlayerFolder = null;

        private DefaultAsset animationControllerFolder = null;
        private GameObject horsePrefab = null;
        private AnimatorController baseAnimController = null;
        private SpriteResolver horseSpriteResolver = null;
        private SpriteLibrary horseSpriteLibrary = null;
        private SpriteResolver horseResolver = null;

        private static float keyTimerModifier = 100f / 60f;
        [SerializeField] private SO_FarmerHorseAnimationSettings horseSettings = null;

        [MenuItem("Tools/MSC Horse Animator")]
        public static void MSCFarmerHorseEditorWindow()
        {
            GetWindow<MSCFarmerHorseEditor>("ManaSeed Horse Animator");

        }

        private void OnGUI()
        {
            GUILayout.Label("The ManaSeed Horse Animator", EditorStyles.largeLabel);
            GUILayout.Label("Definitions of the animations to create", EditorStyles.largeLabel);
            GUILayout.Label("Found as Scriptable Object in the AnimationSettings Folder");

            horseSettings = (SO_FarmerHorseAnimationSettings)EditorGUILayout.ObjectField("Horse and Player Animation Settings",
                horseSettings, typeof(SO_FarmerHorseAnimationSettings), false);
            
            GUILayout.Space(_space);
            GUILayout.Label("Horse Prefab with Sprite Library and Resolver", EditorStyles.largeLabel);
            GUILayout.Label("Found as Game Object or Prefab in the Prefab Folder");

            horsePrefab = (GameObject)EditorGUILayout.ObjectField("Horse and Player Animation Settings",
                horsePrefab, typeof(GameObject), false);
            if (horsePrefab != null)
            {
                horseSpriteResolver = horsePrefab.GetComponent<SpriteResolver>();
                horseSpriteLibrary = horsePrefab.GetComponent<SpriteLibrary>();
            }
            if (horseSpriteResolver == null && horsePrefab!=null)
            {
                horseSpriteResolver=horsePrefab.GetComponentInChildren<SpriteResolver>();
                if (horseSpriteResolver != null)
                {
                    EditorGUILayout.HelpBox("Sprite Resolver found.", MessageType.Info, true);

                }
                else
                {
                    EditorGUILayout.HelpBox("No Sprite Resolver Found!", MessageType.Warning, true);

                }
            }
            if (horseSpriteLibrary == null && horsePrefab != null)
            {
                horseSpriteLibrary = horsePrefab.GetComponentInChildren<SpriteLibrary>();
                if (horseSpriteLibrary != null)
                {
                    EditorGUILayout.HelpBox("Sprite Library found.", MessageType.Info, true);

                }
                else
                {
                    EditorGUILayout.HelpBox("No Sprite Library Found!", MessageType.Warning, true);

                }
            }


            GUILayout.Space(_space);
            GUILayout.Label("Directory to save Horse Animations", EditorStyles.largeLabel);
            GUILayout.Label("Must be in a resources directory");

            animationHorseFolder = (DefaultAsset)EditorGUILayout.ObjectField("Save Path for Animations",
                animationHorseFolder, typeof(DefaultAsset),false);

            if (animationHorseFolder != null)
            {
                animationHorseSavePath = AssetDatabase.GetAssetPath(animationHorseFolder);
                animationHorsePath = animationHorseSavePath.Replace("Assets/Resources/", "").Replace("Assets/MSC Animation Creator/Resources/", "");
                EditorGUILayout.HelpBox(
                    "Saving the animations to: " + animationHorsePath,
                    MessageType.Info,
                    true);
            }
            else
            {
                EditorGUILayout.HelpBox(
                    "Not valid!",
                    MessageType.Warning,
                    true);
            }

            GUILayout.Space(_space);
            GUILayout.Label("Directory to save Player Horse Animations", EditorStyles.largeLabel);
            GUILayout.Label("Must be in a resources directory");

            animationPlayerFolder = (DefaultAsset)EditorGUILayout.ObjectField("Save Path for Animations",
                animationPlayerFolder, typeof(DefaultAsset), false);

            if (animationPlayerFolder != null)
            {
                animationPlayerSavePath = AssetDatabase.GetAssetPath(animationPlayerFolder);
                animationPlayerPath = animationPlayerSavePath.Replace("Assets/Resources/", "").Replace("Assets/MSC Animation Creator/Resources/", "");
                EditorGUILayout.HelpBox(
                    "Saving the animations to: " + animationPlayerPath,
                    MessageType.Info,
                    true);
            }
            else
            {
                EditorGUILayout.HelpBox(
                    "Not valid!",
                    MessageType.Warning,
                    true);
            }

            //Create Button
            if (GUILayout.Button("Create Animations"))
            {
                CreateBaseAnimations();
            }

        }

        private void CreateBaseAnimations()
        {
            foreach (MSCFarmerHorseAnimation anim in horseSettings.list)
            {
                Debug.Log("Create Horse Bottom Animations");
                string animSavePath = animationHorseSavePath + "/";
                string animSaveName = anim.animationType + anim.animationName + "bottom";
                if (!AssetDatabase.IsValidFolder(animationHorseSavePath + "/" + anim.animationType.ToString()))
                {
                    AssetDatabase.CreateFolder(System.IO.Path.GetDirectoryName(animationHorseSavePath + "/"), anim.animationType.ToString());
                }

                Debug.Log($"Building clip: {animSaveName} at {animSavePath}");
                CreateHorseAnimationClip(anim.bottomCategory, anim.bottomHorseKeyLabels, anim.bottomHorseKeyXFlips, 
                    anim.bottomHorsekeyTimer, animSaveName, animSavePath, anim.animationType, anim.horseXFlip);

                animSaveName = anim.animationType + anim.animationName + "top";
                Debug.Log($"Building clip: {animSaveName} at {animSavePath}");
                CreateHorseAnimationClip(anim.topCategory, anim.topHorseKeyLabels, anim.topHorseKeyXFlips, anim.topHorsekeyTimer, 
                    animSaveName, animSavePath, anim.animationType, anim.horseXFlip);

            }
            
        }

        public void CreateHorseAnimationClip(string category, string[] keys, bool[] keyxFlips, float[] keyTimer, string animName, string savePathParent, string savePathAdd, bool xFlip)
        {
            
            //Test for any frames that have to be flipped instead of the whole clip flipped on the x axis
            bool spriteFlips = false;
            if (keyxFlips.Length > 0)
            {
                for (int ij = 0; ij < keyxFlips.Length; ij++)
                {
                    if (keyxFlips[ij])
                    {
                        spriteFlips = true;
                    }
                }
            }
            //create the clip and name if, setting the frame rate to 60
            AnimationClip newClip = new AnimationClip();
            newClip.name = animName;
            newClip.frameRate = 60f;

            //define the sprite curve binding properties for the animation
            EditorCurveBinding spriteBinding = new EditorCurveBinding
            {
                path = "",
                type = typeof(SpriteResolver),
                propertyName = "m_SpriteHash"
            };

            //define the SpriteResolver Enable curve binding property for the animation
            EditorCurveBinding enableResolver = new EditorCurveBinding
            {
                path = "",
                type = typeof(SpriteResolver),
                propertyName = "m_Enabled"
            };

            //create the SpriteResolver Enable curve
            AnimationCurve resolverEnabled = new AnimationCurve();
            resolverEnabled.AddKey(0f, 1f);

            AnimationClipSettings animClipSett = new AnimationClipSettings();
            animClipSett.loopTime = true;

            AnimationUtility.SetAnimationClipSettings(newClip, animClipSett);

            //create the animation curve for the clip
            AnimationCurve resolverCurve = new AnimationCurve();
            
            //loop through all the label keys for the clip
            for(int j=0;j<keys.Length; j++)
            {
                //generate the hash based on the passed category and the key label
                int resolverHash = GetSpriteHash(category, keys[j]);
                Debug.Log($"Has for: {category}_{keys[j]} is {resolverHash}");
                Keyframe resolverKey= new Keyframe(keyTimer[j] * keyTimerModifier, (resolverHash));

                int index=resolverCurve.AddKey(resolverKey);
                AnimationUtility.SetKeyLeftTangentMode(resolverCurve, index, AnimationUtility.TangentMode.Constant);
                AnimationUtility.SetKeyRightTangentMode(resolverCurve, index, AnimationUtility.TangentMode.Constant);
                
            }

            AnimationUtility.SetEditorCurve(newClip, enableResolver, resolverEnabled);
            AnimationUtility.SetEditorCurve(newClip,spriteBinding, resolverCurve);
            
            //do the individual sprite flips if it is required
            if (spriteFlips)
            {
                EditorCurveBinding flipSpriteX = EditorCurveBinding.FloatCurve("", typeof(SpriteRenderer), "m_FlipX");
                AnimationCurve flipac = new AnimationCurve(CreateSpriteFlipKeyframes(keyxFlips, keyTimer));
                newClip.SetCurve("", typeof(SpriteRenderer), "m_FlipX", flipac);
            }

            //do the full clip flip if it is required
            if (xFlip)
            {
                EditorCurveBinding flipX = new EditorCurveBinding();
                flipX.type = typeof(SpriteRenderer);
                flipX.path = "";
                flipX.propertyName = "m_FlipX";
                AnimationCurve ac = new AnimationCurve();
                ac.AddKey(0f, 1f);
                AnimationUtility.SetEditorCurve(newClip, flipX, ac);
            }

            //Debug.Log(savePathParent);
            //Debug.Log(savePathAdd);
            //save the clip
            if (!AssetDatabase.IsValidFolder(savePathParent + savePathAdd))
            {
                AssetDatabase.CreateFolder(System.IO.Path.GetDirectoryName(savePathParent), savePathAdd);
            }
            AssetDatabase.CreateAsset(newClip, savePathParent + savePathAdd + "/" + newClip.name + ".anim");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        public static ObjectReferenceKeyframe[] CreateSpriteHorseKeyframes(string[] resolverKeys, float[] keyTimer,string category)
        {
            //Debug.Log(modifier);
            ObjectReferenceKeyframe[] keyFrames = new ObjectReferenceKeyframe[resolverKeys.Length];
            if (resolverKeys.Length > 0)
            {
                for (int i = 0; i < resolverKeys.Length; i++)
                {
                    
                    string categoryAndLabel = category + "/" + resolverKeys[i];

                    //keyFrames[i] = new ObjectReferenceKeyframe
                   // {
                   //     time = keyTimer[i] * keyTimerModifier,
                   //     value = categoryAndLabel
                   // };
                    
                    
                }
            }

            return keyFrames;
        }

        public static Keyframe[] CreateSpriteFlipKeyframes(bool[] flipXSprite, float[] keyTimer)
        {
            //Debug.Log(modifier);
            Keyframe[] keyFrames = new Keyframe[flipXSprite.Length];
            if (flipXSprite.Length > 0)
            {
                
                for (int i = 0; i < flipXSprite.Length; i++)
                {
                    
                    if (flipXSprite[i])
                    {
                        keyFrames[i] = new Keyframe(keyTimer[i] * keyTimerModifier, 1f, float.PositiveInfinity, float.PositiveInfinity);
                    }
                    else
                    {
                        keyFrames[i] = new Keyframe(keyTimer[i] * keyTimerModifier, 0f, float.PositiveInfinity, float.PositiveInfinity);
                    }
                }
            }

            return keyFrames;
        }

        // This mimics Unity's internal Bit30Hash hashing for Sprite Resolver
        private static int GetSpriteHash(string category, string label)
        {
            
            int hash = Animator.StringToHash($"{category}_{label}");
            hash = Preserve30Bits(hash);
            return hash ; // Preserves first 30 bits as required by Unity 6
        }

        private static int Preserve30Bits(int input)
        {
            const int mask = 0x3FFFFFFF;
            return input & mask;
        }
    }
}
