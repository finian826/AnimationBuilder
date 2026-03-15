using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Rendering;


namespace ManaSeedTools.CharacterAnimator
{
    public class MSCFarmerUtilityEditor : EditorWindow
    {
        private float _space = 5f;

        private string spritePath = "Sprites/Character/Spritesheets/";
        private string spriteSavePath = "Sprites/Character/Spritesheets/";
        private string spritePathTexture = "Sprites/Character/Output Texture/";
        private string spriteSaveTexture = "Sprites/Character/Output Texture/";
        private string animationControllerPath = "Animations/Player/AnimationController/";
        private string animationControllerSavePath = "Animations/Player/AnimationController/";
        private string animationPath = "Animations/Player/Animations/";
        private string animationSavePath = "Animations/Player/Animations/";

        private string preSlicedSpriteBase;

        private DefaultAsset spriteFolder = null;
        private DefaultAsset spriteOutputFolder = null;
        private DefaultAsset animationFolder = null;
        private DefaultAsset animationControllerFolder = null;
        private GameObject playerPrefab = null;
        private AnimatorController baseAnimController = null;
        private static List<string> clipSubFolders = new List<string>();

        private static float keyTimerModifier = 100f / 60f;
        [SerializeField] private SO_FarmerAnimationSettings farmerBaseAnimations = null;

        [SerializeField]
        private List<string> animationLayers = new List<string>() { "00undr", "01body", "02sock", "03fot1",
            "04lwr1", "05shrt", "06lwr2", "07fot2", "08lwr3", "09hand","10outr","11neck","12face","13hair","14head","15over" };

        [MenuItem("Tools/MSC Farmer Animator")]
        public static void MSCFarmerEditorWindow()
        {
            GetWindow<MSCFarmerUtilityEditor>("ManaSeed Farmer Animator");

        }

        private void OnGUI()
        {
            preSlicedSpriteBase = AssetDatabase.GetAssetPath((Texture2D)Resources.Load("farmer_pre_sliced_sprites/empty_texture"));

            GUILayout.Label("The ManaSeed Farmer Animator", EditorStyles.largeLabel);
            GUILayout.Label("Definitions of the animations to create", EditorStyles.largeLabel);
            GUILayout.Label("Found as Scriptable Object in the AnimationSettings Folder");

            farmerBaseAnimations = (SO_FarmerAnimationSettings)EditorGUILayout.ObjectField("CharacterBase Animation Settings",
                farmerBaseAnimations, typeof(SO_FarmerAnimationSettings), false);

            GUILayout.Label("Animations Layer subfolders in the save path for animations");
            ScriptableObject target = this;
            SerializedObject so = new SerializedObject(target);
            SerializedProperty animationLayersProperty = so.FindProperty("animationLayers");
            EditorGUILayout.PropertyField(animationLayersProperty, true); // True means show children
            so.ApplyModifiedProperties(); // Remember to apply modified properties
            GUILayout.Space(_space);
            GUILayout.Label("Folder Paths (they need to be nested in: Assets/Resources)");
            GUILayout.Label("Folder of sliced sprites the character should use");

            //Sprites
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
            GUILayout.Label("Create Animations", EditorStyles.largeLabel);
            GUILayout.Label($"Use sprites from folder: {spritePath}");
            GUILayout.Label("Please be aware that this will take some time");
            //Animations
            animationFolder = (DefaultAsset)EditorGUILayout.ObjectField(
                "Save Path for Animations",
                animationFolder,
                typeof(DefaultAsset),
                false);

            if (animationFolder != null)
            {
                animationSavePath = AssetDatabase.GetAssetPath(animationFolder);
                animationPath = animationSavePath.Replace("Assets/Resources/", "").Replace("Assets/MSC Animation Creator/Resources/", "");
                EditorGUILayout.HelpBox(
                    "Saving the animations to: " + animationPath,
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
            GUILayout.Space(_space);
            GUILayout.Label("Rework/Create AnimController", EditorStyles.largeLabel);
            //AnimationController
            animationControllerFolder = (DefaultAsset)EditorGUILayout.ObjectField(
                "AnimationController Path",
                animationControllerFolder,
                typeof(DefaultAsset),
                false);

            if (animationControllerFolder != null)
            {
                animationControllerSavePath = AssetDatabase.GetAssetPath(animationControllerFolder);
                animationControllerPath = animationControllerSavePath.Replace("Assets/Resources/", "").Replace("Assets/MSC Animation Creator/Resources/", "");
                EditorGUILayout.HelpBox(
                    "Saving/Updating the AnimationControllers in: " + animationControllerPath,
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
            GUILayout.Label("Set the base AnimController provided in Resources/AnimationController");
            baseAnimController = (AnimatorController)EditorGUILayout.ObjectField(
                "Base AnimController",
                baseAnimController,
                typeof(AnimatorController),
                false);
            GUILayout.Space(_space);
            GUILayout.Label("If you have a player prefab you want to associate the anim controller to:");
            playerPrefab = (GameObject)EditorGUILayout.ObjectField(
                "Player Prefab",
                playerPrefab,
                typeof(GameObject),
                false);
            GUILayout.Space(_space);
            //Refactor Controller Button
            if (GUILayout.Button("Rework Animation Controller"))
            {
                ReworkAnimationController();
            }


        }

        private void CreateBaseAnimations()
        {
            Debug.Log("CreateBaseAnimations");
            string thisSpritePath = spritePath + "/";
            Sprite[] spriteSheet = null;
            foreach (var layer in animationLayers)
            {
                spriteSheet = null;
                spriteSheet = Resources.LoadAll<Sprite>(thisSpritePath + layer);
                Debug.Log($"loaded {spriteSheet.Length.ToString()}");
                foreach (MSCFarmerAnimation anim in farmerBaseAnimations.list)
                {
                    string animSavePath = animationSavePath + "/" + layer.ToString() + "/";
                    if (!AssetDatabase.IsValidFolder(animationSavePath + "/" + layer.ToString()))
                    {
                        AssetDatabase.CreateFolder(System.IO.Path.GetDirectoryName(animationSavePath + "/"), layer.ToString());
                    }
                    //string animSaveName = layer + anim.animationType + anim.animationName;
                    string animSaveName = anim.animationType + anim.animationName;
                    Debug.Log($"Building clip: {animSaveName} at {animSavePath}");
                    CreateAnimationClip(spriteSheet, anim.keys, anim.keyXFlips, anim.keyTimer, animSaveName, animSavePath, anim.animationType, layer, anim.xFlip);
                }
            }
        }

        public static void CreateAnimationClip(Sprite[] sprites, int[] keys, bool[] keyxFlips, float[] keyTimer, string animName, string savePathParent, string savePathAdd, string layer, bool xFlip)
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
                clipSubFolders.Add(savePathAdd);
            }
            AssetDatabase.CreateAsset(newClip, savePathParent + savePathAdd + "/" + newClip.name + ".anim");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
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

        private void ReworkAnimationController()
        {
            if (clipSubFolders.Count == 0 && farmerBaseAnimations != null)
            {
                string prevType = "";
                foreach (MSCFarmerAnimation anim in farmerBaseAnimations.list)
                {
                    if (anim.animationType != prevType)
                    {
                        clipSubFolders.Add(anim.animationType);
                        prevType = anim.animationType;
                    }
                }
                Debug.Log($"Clip sub folder length: {clipSubFolders.Count.ToString()}");
            }
            else
            {
                Debug.Log("Need animation SO object");
                return;
            }
            Debug.Log("ReworkAnimationController ");
            foreach (var layer in animationLayers)
            {
                AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(baseAnimController), animationControllerSavePath + "/" + layer + ".controller");
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            foreach (var layer in animationLayers)
            {
                AnimatorController animController = Resources.Load<AnimatorController>(animationControllerPath + "/" + layer);
                //need to add subfolders for clips
                foreach (var subFolder in clipSubFolders)
                {
                    Debug.Log($"Clip Folder: {animationPath}/{layer}/{subFolder}/");
                    AnimationClip[] animClips = Resources.LoadAll<AnimationClip>(animationPath + "/" + layer + "/" + subFolder + "/");
                    Debug.Log(animationPath + "/" + layer.ToString() + "/" + subFolder + " => " + animClips.Length);
                    if (animClips.Length == 0)
                        break;
                    if (animController != null)
                    {
                        AnimatorControllerLayer[] layers = animController.layers;
                        AnimatorControllerLayer workingLayer = layers[0];
                        List<AnimatorState> stateList = _ExpandStatesInLayer(workingLayer.stateMachine);
                        foreach (var state in stateList)
                        {
                            AnimationClip toUse = null;
                            //string animName = layer + state.name.ToLower();
                            string animName = state.name.ToLower();
                            foreach (AnimationClip reworkedClip in animClips)
                            {
                                Debug.Log($"State: {animName.ToString()} : Clip: {reworkedClip.name.ToString()}");
                                if (animName == reworkedClip.name)
                                {
                                    Debug.Log($"**Match** State: {animName.ToString()} : Clip: {reworkedClip.name.ToString()}");
                                    toUse = reworkedClip;
                                    state.motion = toUse;
                                }
                            }

                        }
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
                        //zuordnung Player prefab
                        if (playerPrefab != null)
                        {
                            //newPrefab.GetComponent<Animator>().runtimeAnimatorController = Resources.Load<AnimatorController>(animPath.Replace("Assets/Resources/", "") + "/" + saveName);
                            foreach (Animator pl_animator in playerPrefab.GetComponentsInChildren<Animator>())
                            {
                                if (pl_animator.name == layer.ToString()) pl_animator.runtimeAnimatorController = animController;
                            }
                        }
                    }
                    else
                    {
                        AnimatorController animator = AnimatorController.CreateAnimatorControllerAtPath(animationControllerSavePath + "/" + layer.ToString() + ".controller");
                        animator.name = layer;
                        AnimatorControllerLayer clayer = animator.layers[0];
                        foreach (AnimationClip reworkedClip in animClips)
                        {
                            AnimatorState state = clayer.stateMachine.AddState(reworkedClip.name);
                            state.motion = reworkedClip;
                        }
                    }
                }
            }
        }

        public static List<AnimatorState> _ExpandStatesInLayer(AnimatorStateMachine sm, List<AnimatorState> collector = null)
        {
            if (collector == null)
                collector = new List<AnimatorState>();

            foreach (var subSm in sm.stateMachines) // Jump into nested state machine
                _ExpandStatesInLayer(subSm.stateMachine, collector);

            foreach (var state in sm.states)
            {
                collector.Add(state.state);

                foreach (var subSm in sm.stateMachines) // Jump into nested state machine
                    _ExpandStatesInLayer(subSm.stateMachine, collector);
            }
            return collector;
        }


    }
}
