using DogHeist.EditorTools.Greybox;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace DogHeist.Tests.EditMode
{
    public sealed class GreyboxAssetsTests
    {
        private const string TempRoot = "Assets/_Project/Tests/_GreyboxAssetsTemp";

        [TearDown]
        public void DeleteTempAssets() => AssetDatabase.DeleteAsset(TempRoot);

        [Test]
        public void LoadOrCreate_CreatesConfigsPrefabAndMaterials()
        {
            var assets = GreyboxAssets.LoadOrCreate(TempRoot);

            Assert.IsNotNull(AssetDatabase.LoadAssetAtPath<ScriptableObject>($"{TempRoot}/Settings/Configs/ThiefConfig.asset"));
            Assert.IsNotNull(assets.DogConfig);
            Assert.IsNotNull(assets.OwnerConfig);
            Assert.IsNotNull(assets.LurePrefab);
            Assert.IsNotNull(assets.LurePrefab.GetComponent<Rigidbody>());
            Assert.IsNotNull(assets.GroundMaterial.shader);
        }

        [Test]
        public void LoadOrCreate_DoesNotOverwriteExistingConfig()
        {
            var first = GreyboxAssets.LoadOrCreate(TempRoot);
            var serialized = new SerializedObject(first.ThiefConfig);
            serialized.FindProperty("<WalkSpeed>k__BackingField").floatValue = 9f;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.SaveAssets();

            var second = GreyboxAssets.LoadOrCreate(TempRoot);

            Assert.AreSame(first.ThiefConfig, second.ThiefConfig);
            Assert.AreEqual(9f, second.ThiefConfig.WalkSpeed);
        }

        [Test]
        public void EnsureLayer_ReturnsSameIndexOnSecondCall()
        {
            var first = GreyboxAssets.EnsureLayer(GreyboxAssets.CharactersLayerName);
            var second = GreyboxAssets.EnsureLayer(GreyboxAssets.CharactersLayerName);

            Assert.AreEqual(first, second);
            Assert.AreEqual(first, LayerMask.NameToLayer(GreyboxAssets.CharactersLayerName));
        }
    }
}
