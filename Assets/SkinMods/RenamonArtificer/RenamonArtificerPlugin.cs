using BepInEx;
using BepInEx.Logging;
using RoR2;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using UnityEngine;
using System.Security;
using System.Security.Permissions;
using MonoMod.RuntimeDetour;

#pragma warning disable CS0618 // Type or member is obsolete
[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete
[assembly: R2API.Utils.ManualNetworkRegistration]
[assembly: EnigmaticThunder.Util.ManualNetworkRegistration]
namespace RenamonArtificer
{
    
    [BepInPlugin("com.RuneFox237.RenamonArtificer","RenamonArtificer","1.0.0")]
    public partial class RenamonArtificerPlugin : BaseUnityPlugin
    {
        internal static RenamonArtificerPlugin Instance { get; private set; }
        internal static ManualLogSource InstanceLogger { get; private set; }
        
        private static AssetBundle assetBundle;
        private static readonly List<Material> materialsWithRoRShader = new List<Material>();
        private void Awake()
        {
            Instance = this;
            BeforeAwake();
            using (var assetStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("RenamonArtificer.runefox237renamonartificer"))
            {
                assetBundle = AssetBundle.LoadFromStream(assetStream);
            }

            BodyCatalog.availability.CallWhenAvailable(BodyCatalogInit);
            new Hook(typeof(Language).GetMethod(nameof(Language.LoadStrings)), (Action<Action<Language>, Language>)LanguageLoadStrings).Apply();

            ReplaceShaders();

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

        private static void LanguageLoadStrings(Action<Language> orig, Language self)
        {
            orig(self);

            switch(self.name.ToLower())
            {
                case "en":
                    self.SetStringByToken("RUNEFOX237_SKIN_RENAMONSKIN_NAME", "Renamon");
                    break;
                default:
                    self.SetStringByToken("RUNEFOX237_SKIN_RENAMONSKIN_NAME", "Renamon");
                    break;
            }
        }

        private static void Nothing(Action<SkinDef> orig, SkinDef self)
        {

        }

        private static void BodyCatalogInit()
        {
            BeforeBodyCatalogInit();

            var hook = new Hook(typeof(SkinDef).GetMethod(nameof(SkinDef.Awake), BindingFlags.NonPublic | BindingFlags.Instance), (Action<Action<SkinDef>, SkinDef>)Nothing);
            hook.Apply();

            AddMageBodyRenamonSkinSkin();

            hook.Undo();

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
                var modelLocator = bodyPrefab.GetComponent<ModelLocator>();
                var mdl = modelLocator.modelTransform.gameObject;
                var skinController = mdl.GetComponent<ModelSkinController>();

                var renderers = mdl.GetComponentsInChildren<Renderer>(true);

                var skin = ScriptableObject.CreateInstance<SkinDef>();
                skin.icon = assetBundle.LoadAsset<Sprite>(@"Assets\SkinMods\RenamonArtificer\Icons\RenamonSkinIcon.png");
                skin.name = skinName;
                skin.nameToken = "RUNEFOX237_SKIN_RENAMONSKIN_NAME";
                skin.rootObject = mdl;
                skin.baseSkins = Array.Empty<SkinDef>();
                skin.unlockableDef = null;
                skin.gameObjectActivations = new SkinDef.GameObjectActivation[]
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
                };
                skin.rendererInfos = new CharacterModel.RendererInfo[]
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
                };
                skin.meshReplacements = new SkinDef.MeshReplacement[]
                {
                    new SkinDef.MeshReplacement
                    {
                        mesh = assetBundle.LoadAsset<Mesh>(@"Assets\SkinMods\RenamonArtificer\Meshes\Renamon.mesh"),
                        renderer = renderers.First(r => r.name == "MageMesh")
                    },
                };
                skin.minionSkinReplacements = Array.Empty<SkinDef.MinionSkinReplacement>();
                skin.projectileGhostReplacements = Array.Empty<SkinDef.ProjectileGhostReplacement>();

                Array.Resize(ref skinController.skins, skinController.skins.Length + 1);
                skinController.skins[skinController.skins.Length - 1] = skin;

                BodyCatalog.skins[(int)BodyCatalog.FindBodyIndex(bodyPrefab)] = skinController.skins;
                MageBodyRenamonSkinSkinAdded(skin, bodyPrefab);
            }
            catch (Exception e)
            {
                InstanceLogger.LogWarning($"Failed to add \"{skinName}\" skin to \"{bodyName}\"");
                InstanceLogger.LogError(e);
            }
        }
    }

}

namespace R2API.Utils
{
    [AttributeUsage(AttributeTargets.Assembly)]
    public class ManualNetworkRegistrationAttribute : Attribute { }
}

namespace EnigmaticThunder.Util
{
    [AttributeUsage(AttributeTargets.Assembly)]
    public class ManualNetworkRegistrationAttribute : Attribute { }
}