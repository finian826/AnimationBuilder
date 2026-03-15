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
        private SpriteLibraryAsset horseSpriteLibraryAsset = null;
        private SpriteResolver horseResolver = null;

        private static float keyTimerModifier = 100f / 60f;
        private static float pixelOffset = 1f / 16f;

        [SerializeField] private SO_FarmerHorseAnimationSettings horseSettings = null;

        [SerializeField]
        private List<string> animationLayers = new List<string>() { "00undr", "01body", "02sock", "03fot1",
            "04lwr1", "05shrt", "06lwr2", "07fot2", "08lwr3", "09hand","10outr","11neck","12face","13hair","14head","15over" };


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
            GUILayout.Label("Horse Sprite Library", EditorStyles.largeLabel);
            GUILayout.Label("Found as Sprite Library Asset");

            horseSpriteLibraryAsset = (SpriteLibraryAsset)EditorGUILayout.ObjectField("Horse Prefab",
                horseSpriteLibraryAsset, typeof(SpriteLibraryAsset), false);
            if (horseSpriteLibraryAsset != null)
            {
                EditorGUILayout.HelpBox(
                    "Sprite Library Asset: " + horseSpriteLibraryAsset.name.ToString(),
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

            /*if (horsePrefab != null)
            {
                horseSpriteResolver = horsePrefab.GetComponent<SpriteResolver>();
                horseSpriteLibrary = horsePrefab.GetComponent<SpriteLibrary>();
            }
            if (horseSpriteResolver == null && horsePrefab != null)
            {
                horseSpriteResolver = horsePrefab.GetComponentInChildren<SpriteResolver>();
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
            }*/


            GUILayout.Space(_space);
            GUILayout.Label("Directory to save Horse Animations", EditorStyles.largeLabel);
            GUILayout.Label("Must be in a resources directory");

            animationHorseFolder = (DefaultAsset)EditorGUILayout.ObjectField("Save Path for Horse Animations",
                animationHorseFolder, typeof(DefaultAsset), false);

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

            animationPlayerFolder = (DefaultAsset)EditorGUILayout.ObjectField("Save Path for Player on Horse Animations",
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
            //Sprites
            GUILayout.Space(_space);
            GUILayout.Label("Folder Paths (they need to be nested in: Assets/Resources)");
            GUILayout.Label("Folder of sliced sprites the character should use");

            spriteFolder = (DefaultAsset)EditorGUILayout.ObjectField(
                "Character Spritesheets",
                spriteFolder,
                typeof(DefaultAsset),
                false);
            if (spriteFolder != null)
            {
                spriteSavePath = AssetDatabase.GetAssetPath(spriteFolder);
                spritePath = AssetDatabase.GetAssetPath(spriteFolder).Replace("Assets/Resources/", "").Replace("Assets/MSC Animation Creator/Resources/", "");
                EditorGUILayout.HelpBox(
                    "Sprites located at: " + spritePath,
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
            GUILayout.Label("Animations Layer subfolders in the save path for animations");
            ScriptableObject target = this;
            SerializedObject so = new SerializedObject(target);
            SerializedProperty animationLayersProperty = so.FindProperty("animationLayers");
            EditorGUILayout.PropertyField(animationLayersProperty, true); // True means show children
            so.ApplyModifiedProperties(); // Remember to apply modified properties


            //Create Button
            if (GUILayout.Button("Create Animations"))
            {
                CreateBaseAnimations();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

            }

        }

        private void CreateBaseAnimations()
        {
            foreach (MSCFarmerHorseAnimation anim in horseSettings.list)
            {
                //Debug.Log("Create Horse Bottom Animations");
                string animSavePath = animationHorseSavePath + "/";
                string animSaveName = "horse" + anim.animationType + anim.animationName + "bottom";
                if (!AssetDatabase.IsValidFolder(animationHorseSavePath + "/" + anim.animationType.ToString()))
                {
                    AssetDatabase.CreateFolder(System.IO.Path.GetDirectoryName(animationHorseSavePath + "/"), anim.animationType.ToString());
                }

                //Debug.Log($"Building clip: {animSaveName} at {animSavePath}");
                CreateHorseAnimationClip(anim.bottomCategory, anim.bottomHorseKeyLabels, anim.bottomHorseKeyXFlips,
                    anim.bottomHorsekeyTimer, animSaveName, animSavePath, anim.animationType, anim.horseXFlip);

                animSaveName = "horse" + anim.animationType + anim.animationName + "top";
                //Debug.Log($"Building clip: {animSaveName} at {animSavePath}");
                CreateHorseAnimationClip(anim.topCategory, anim.topHorseKeyLabels, anim.topHorseKeyXFlips, anim.topHorsekeyTimer,
                    animSaveName, animSavePath, anim.animationType, anim.horseXFlip);
                if (anim.buildFarmer)
                {
                    string thisSpritePath = spritePath + "/";
                    Sprite[] spriteSheet = null;
                    foreach (var layer in animationLayers)
                    {
                        spriteSheet = null;
                        spriteSheet = Resources.LoadAll<Sprite>(thisSpritePath + layer);
                        //Debug.Log($"loaded {spriteSheet.Length.ToString()}");
                            string playerAnimSavePath = animationPlayerSavePath + "/" + layer.ToString() + "/";
                            if (!AssetDatabase.IsValidFolder(animationPlayerSavePath + "/" + layer.ToString()))
                            {
                            AssetDatabase.CreateFolder(System.IO.Path.GetDirectoryName(animationPlayerSavePath + "/"), layer.ToString());
                            }

                        //string playerAnimSaveName = layer + anim.animationType + anim.animationName;
                        string playerAnimSaveName = "horse" + anim.animationType + anim.animationName;
                            //Debug.Log($"Building clip: {animSaveName} at {animSavePath}");
                            CreateFarmerAnimationClip(spriteSheet, anim.farmerKeys, anim.farmerKeyXFlips, anim.farmerKeyTimer, anim.farmerFrameOffset, playerAnimSaveName, playerAnimSavePath, 
                                anim.animationType, layer, anim.farmerXFlip);
                        
                    }

                }
            }

        }

        /*public void BadCreateHorseAnimationClip(string category, string[] keys, bool[] keyxFlips, float[] keyTimer, string animName, string savePathParent, string savePathAdd, bool xFlip)
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
            EditorCurveBinding catagoryBinding = new EditorCurveBinding
            {
                path = "",
                type = typeof(SpriteResolver),
                propertyName = "m_CategoryHash"
            };
            

            EditorCurveBinding labelBinding = new EditorCurveBinding
            {
                path = "",
                type = typeof(SpriteResolver),
                propertyName = "m_labelHash"                
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

            //build label hash curve
            AnimationCurve labelCurve = new AnimationCurve();
            AnimationCurve categoryCurve=new AnimationCurve();
            for(int j = 0; j < keys.Length; j++)
            {
                Keyframe resolverKey = new Keyframe(keyTimer[j] * keyTimerModifier, (float)GetHash(keys[j]));
                Keyframe categoryKey = new Keyframe(keyTimer[j] * keyTimerModifier, (float)GetHash(category));
                resolverKey.weightedMode = WeightedMode.None;
                resolverKey.inTangent = 0f;
                resolverKey.outTangent = 0f;
                categoryKey.weightedMode= WeightedMode.None;
                categoryKey.inTangent = 0f;
                categoryKey.outTangent = 0f;

                int index = labelCurve.AddKey(resolverKey);
                int index2 = categoryCurve.AddKey(categoryKey);
                Debug.Log($"Hash for category: {category}: {GetHash(category)}");
                Debug.Log($"Hash for label: {keys[j]}: {GetHash(keys[j])}");
                //AnimationUtility.SetKeyLeftTangentMode(labelCurve, index, AnimationUtility.TangentMode.Constant);
                //AnimationUtility.SetKeyRightTangentMode(labelCurve, index, AnimationUtility.TangentMode.Constant);
                //AnimationUtility.SetKeyLeftTangentMode(categoryCurve, index2, AnimationUtility.TangentMode.Constant);
                //AnimationUtility.SetKeyRightTangentMode(categoryCurve, index2, AnimationUtility.TangentMode.Constant);

            }

            //bind the bindings and curves to the animation
            AnimationUtility.SetEditorCurve(newClip, catagoryBinding, categoryCurve);
            AnimationUtility.SetEditorCurve(newClip, enableResolver, resolverEnabled);
            AnimationUtility.SetEditorCurve(newClip, labelBinding, labelCurve);

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
        }*/

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
            //create the clip and name, setting the frame rate to 60
            AnimationClip newClip = new AnimationClip();
            newClip.name = animName;
            newClip.frameRate = 60f;
            

            //define the sprite curve binding properties for the animation
            EditorCurveBinding spriteBinding = EditorCurveBinding.FloatCurve("", typeof(SpriteResolver), "m_SpriteHash");
            
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
            
            //IEnumerable<string> categoryList = horseSpriteLibraryAsset.GetCategoryNames();

            //loop through all the label keys for the clip
            for (int j = 0; j < keys.Length; j++)
            {
                //generate the hash based on the passed category and the key label
                int resolverHash = GetSpriteHash(category, keys[j]);
                Debug.Log($"Hash for: {category}_{keys[j]} is {resolverHash}");
                Debug.Log($"Hash for: {category}_{keys[j]} is {(float)resolverHash}");
                Debug.Log($"Hash for category: {category}: {GetHash(category)}");
                Debug.Log($"Hash for label: {keys[j]}: {GetHash(keys[j])}");
                float curveFloat = GetAnimationFloatFromHash(resolverHash);
                //build the keyframe based on the calculated time and generated hash
                Keyframe resolverKey = new Keyframe();
                resolverKey.time = keyTimer[j] * keyTimerModifier;
                resolverKey.value = curveFloat;
                resolverKey.weightedMode = WeightedMode.None;
                resolverKey.inTangent = 0f;
                resolverKey.outTangent = 0f;

                int index = resolverCurve.AddKey(resolverKey);
                //AnimationUtility.SetKeyLeftTangentMode(resolverCurve, index, AnimationUtility.TangentMode.Constant);
                //AnimationUtility.SetKeyRightTangentMode(resolverCurve, index, AnimationUtility.TangentMode.Constant);

            }

            AnimationUtility.SetEditorCurve(newClip, enableResolver, resolverEnabled);
            AnimationUtility.SetEditorCurve(newClip, spriteBinding, resolverCurve);

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

        public static void CreateFarmerAnimationClip(Sprite[] sprites, int[] keys, bool[] keyxFlips, float[] keyTimer, Vector2Int[] frameOffset, string animName, string savePathParent, string savePathAdd, string layer, bool xFlip)
        {
            bool spriteFlips = false;
            if (keyxFlips.Length > 0)
            {
                for (int i = 0; i < keyxFlips.Length; i++)
                {
                    if (keyxFlips[i])
                    {
                        spriteFlips = true;
                    }
                }
            }
            AnimationClip newClip = new AnimationClip();
            newClip.name = animName;
            newClip.frameRate = 60f;

            EditorCurveBinding spriteBinding = EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite");

            AnimationClipSettings animClipSett = new AnimationClipSettings();
            animClipSett.loopTime = true;

            AnimationUtility.SetAnimationClipSettings(newClip, animClipSett);
            //Debug.Log($"Key Frames: {keys.ToString()}, Key Timers: {keyTimer.ToString()}");
            AnimationUtility.SetObjectReferenceCurve(newClip, spriteBinding, CreateSpriteKeyframes(sprites, keys, keyTimer));
            CreateSpriteOffsetKeyframes(newClip, frameOffset, keyTimer);
            if (spriteFlips)
            {
                EditorCurveBinding flipSpriteX = EditorCurveBinding.FloatCurve("", typeof(SpriteRenderer), "m_FlipX");
                AnimationCurve flipac = new AnimationCurve(CreateSpriteFlipKeyframes(keyxFlips, keyTimer));
                newClip.SetCurve("", typeof(SpriteRenderer), "m_FlipX", flipac);
            }
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
            if (!AssetDatabase.IsValidFolder(savePathParent + savePathAdd))
            {
                AssetDatabase.CreateFolder(System.IO.Path.GetDirectoryName(savePathParent), savePathAdd);
            }
            AssetDatabase.CreateAsset(newClip, savePathParent + savePathAdd + "/" + newClip.name + ".anim");
        }

        public static ObjectReferenceKeyframe[] CreateSpriteKeyframes(Sprite[] sprites, int[] spritesNumbers, float[] keyTimer)
        {
            //Debug.Log(modifier);
            ObjectReferenceKeyframe[] keyFrames = new ObjectReferenceKeyframe[spritesNumbers.Length];
            if (sprites.Length > 0)
            {
                for (int i = 0; i < spritesNumbers.Length; i++)
                {
                    keyFrames[i] = new ObjectReferenceKeyframe
                    {
                        time = keyTimer[i] * keyTimerModifier,
                        value = sprites[spritesNumbers[i]]
                    };
                }
            }

            return keyFrames;
        }

        public static void CreateSpriteOffsetKeyframes(AnimationClip clip, Vector2Int[] frameOffset, float[] keyTimer)
        {
            //Debug.Log(modifier);
            EditorCurveBinding offsetX = EditorCurveBinding.FloatCurve("", typeof(Transform), "localposition.x");

            EditorCurveBinding offsetY = EditorCurveBinding.FloatCurve("", typeof(Transform), "localposition.y");

            AnimationCurve xOffsetCurve = new AnimationCurve();
            AnimationCurve yOffsetCurve = new AnimationCurve();

            if (frameOffset.Length > 0)
            {
                for (int i = 0; i < frameOffset.Length; i++)
                {
                    xOffsetCurve.AddKey(keyTimer[i] * keyTimerModifier, frameOffset[i].x * pixelOffset);
                    yOffsetCurve.AddKey(keyTimer[i] * keyTimerModifier, frameOffset[i].y * pixelOffset);
                }
            }
            AnimationUtility.SetEditorCurve(clip, offsetX, xOffsetCurve);
            AnimationUtility.SetEditorCurve(clip, offsetY, yOffsetCurve);

        }


        // This mimics Unity's internal Bit30Hash hashing for Sprite Resolver
        private static int GetSpriteHash(string category, string label)
        {
            string combined = category + "_" + label;
            int hash = Animator.StringToHash(combined);
            hash = Preserve30Bits(hash);
            return hash; // Preserves first 30 bits as required by Unity 6
        }

        private static int Preserve30Bits(int input)
        {
            const int mask = 0x3FFFFFFF;
            return input & mask;
        }
        private static int GetHash(string category)
        {

            int hash = Animator.StringToHash($"{category}");
            hash = Preserve30Bits(hash);
            return hash; // Preserves first 30 bits as required by Unity 6

        }

        public static float GetAnimationFloatFromHash(int spriteHash)
        {
            // This takes the integer bits and puts them into a float 
            // without performing a mathematical conversion.
            byte[] bytes = BitConverter.GetBytes(spriteHash);
            return BitConverter.ToSingle(bytes, 0);
        }

    }
}
