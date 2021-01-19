using BepInEx;
using R2API;
using R2API.Utils;
using RoR2;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using UnityEngine;

namespace RenamonArtificer
{
    [R2APISubmoduleDependency(nameof(LoadoutAPI), nameof(LanguageAPI))]
    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync)]
    [BepInDependency("com.bepis.r2api")]
    [BepInPlugin("com.RuneFox237.RenamonArtificer","RenamonArtificer","1.0.0")]
    public partial class RenamonArtificerPlugin : BaseUnityPlugin
    {
        private static AssetBundle assetBundle;
        private static readonly List<Material> materialsWithRoRShader = new List<Material>();
        private void Awake()
        {
            BeforeAwake();
            using (var assetStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("RenamonArtificer.runefox237renamonartificer"))
            {
                assetBundle = AssetBundle.LoadFromStream(assetStream);
            }

            On.RoR2.BodyCatalog.Init += BodyCatalogInit;

            ReplaceShaders();
            AddLanguageTokens();

            AfterAwake();
        }

        partial void BeforeAwake();
        partial void AfterAwake();
        static partial void BeforeBodyCatalogInit();
        static partial void AfterBodyCatalogInit();

        private static void ReplaceShaders()
        {
            materialsWithRoRShader.Add(LoadMaterialWithReplacedShader(@"Assets/Resources/RenamonMat.mat", @"Hopoo Games/Deferred/Standard"));
            materialsWithRoRShader.Add(LoadMaterialWithReplacedShader(@"Assets/Resources/TestMat.mat", @"Hopoo Games/Deferred/Standard"));
        }

        private static Material LoadMaterialWithReplacedShader(string materialPath, string shaderName)
        {
            var material = assetBundle.LoadAsset<Material>(materialPath);
            material.shader = Shader.Find(shaderName);

            return material;
        }

        private static void AddLanguageTokens()
        {
            LanguageAPI.Add("RUNEFOX237_SKIN_RENAMONSKIN_NAME", "Renamon");
            LanguageAPI.Add("RUNEFOX237_SKIN_RENAMONSKIN_NAME", "Renamon", "EN");
        }

        private static void BodyCatalogInit(On.RoR2.BodyCatalog.orig_Init orig)
        {
            orig();

            BeforeBodyCatalogInit();

            AddMageBodyRenamonSkinSkin();

            AfterBodyCatalogInit();
        }

        static partial void MageBodyRenamonSkinSkinAdded(SkinDef skinDef, GameObject bodyPrefab);

        private static void AddMageBodyRenamonSkinSkin()
        {
            var bodyName = "MageBody";
            var skinName = "RenamonSkin";
            try
            {
                var bodyPrefab = BodyCatalog.FindBodyPrefab(bodyName);

                var renderers = bodyPrefab.GetComponentsInChildren<Renderer>(true);
                var skinController = bodyPrefab.GetComponentInChildren<ModelSkinController>();
                var mdl = skinController.gameObject;

                var skin = new LoadoutAPI.SkinDefInfo
                {
                    Icon = assetBundle.LoadAsset<Sprite>(@"Assets\SkinMods\RenamonArtificer\Icons\RenamonSkinIcon.png"),
                    Name = skinName,
                    NameToken = "RUNEFOX237_SKIN_RENAMONSKIN_NAME",
                    RootObject = mdl,
                    BaseSkins = Array.Empty<SkinDef>(),
                    UnlockableName = "",
                    GameObjectActivations = new SkinDef.GameObjectActivation[]
                    {
                        new SkinDef.GameObjectActivation
                        {
                            gameObject = renderers[6].gameObject,
                            shouldActivate = false
                        },
                        new SkinDef.GameObjectActivation
                        {
                            gameObject = renderers[0].gameObject,
                            shouldActivate = false
                        },
                        new SkinDef.GameObjectActivation
                        {
                            gameObject = renderers[1].gameObject,
                            shouldActivate = false
                        },
                    },
                    RendererInfos = new CharacterModel.RendererInfo[]
                    {
                        new CharacterModel.RendererInfo
                        {
                            defaultMaterial = assetBundle.LoadAsset<Material>(@"Assets/Resources/RenamonMat.mat"),
                            defaultShadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On,
                            ignoreOverlays = false,
                            renderer = renderers.First(r => r.name == "MageMesh")
                        },
                        new CharacterModel.RendererInfo
                        {
                            defaultMaterial = assetBundle.LoadAsset<Material>(@"Assets/Resources/TestMat.mat"),
                            defaultShadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off,
                            ignoreOverlays = true,
                            renderer = renderers[3]
                        },
                        new CharacterModel.RendererInfo
                        {
                            defaultMaterial = assetBundle.LoadAsset<Material>(@"Assets/Resources/TestMat.mat"),
                            defaultShadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off,
                            ignoreOverlays = true,
                            renderer = renderers[4]
                        },
                        new CharacterModel.RendererInfo
                        {
                            defaultMaterial = assetBundle.LoadAsset<Material>(@"Assets/Resources/TestMat.mat"),
                            defaultShadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off,
                            ignoreOverlays = true,
                            renderer = renderers[5]
                        },
                        new CharacterModel.RendererInfo
                        {
                            defaultMaterial = assetBundle.LoadAsset<Material>(@"Assets/Resources/TestMat.mat"),
                            defaultShadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off,
                            ignoreOverlays = true,
                            renderer = renderers[2]
                        },
                    },
                    MeshReplacements = new SkinDef.MeshReplacement[]
                    {
                        new SkinDef.MeshReplacement
                        {
                            mesh = assetBundle.LoadAsset<Mesh>(@"Assets\SkinMods\RenamonArtificer\Meshes\Renamon.mesh"),
                            renderer = renderers.First(r => r.name == "MageMesh")
                        },
                    },
                    MinionSkinReplacements = Array.Empty<SkinDef.MinionSkinReplacement>(),
                    ProjectileGhostReplacements = Array.Empty<SkinDef.ProjectileGhostReplacement>()
                };

                Array.Resize(ref skinController.skins, skinController.skins.Length + 1);
                skinController.skins[skinController.skins.Length - 1] = LoadoutAPI.CreateNewSkinDef(skin);

                var skinsField = typeof(BodyCatalog).GetFieldValue<SkinDef[][]>("skins");
                skinsField[BodyCatalog.FindBodyIndex(bodyPrefab)] = skinController.skins;
                MageBodyRenamonSkinSkinAdded(skinController.skins[skinController.skins.Length - 1], bodyPrefab);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Failed to add \"{skinName}\" skin to \"{bodyName}\"");
                Debug.LogError(e);
            }
        }
    }
}
