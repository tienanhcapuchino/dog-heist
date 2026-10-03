using System.Linq;
using System.Text.RegularExpressions;
using DogHeist.EditorTools.Greybox;
using DogHeist.Gameplay.Audio;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace DogHeist.Tests.EditMode
{
    public sealed class GreyboxFeedbackAssetsTests
    {
        private const string TempRoot = "Assets/_Project/Tests/_GreyboxFeedbackTemp";
        private const string LiberationTtf = "Assets/TextMesh Pro/Fonts/LiberationSans.ttf";

        [TearDown]
        public void DeleteTemp() => AssetDatabase.DeleteAsset(TempRoot);

        [Test]
        public void LoadOrCreate_CreatesEmptyCuesAndStyle()
        {
            LogAssert.Expect(LogType.Warning, new Regex(@"\[Greybox\].*font"));

            var assets = GreyboxAssets.LoadOrCreate(TempRoot);

            Assert.IsNotNull(assets.Sounds.FootstepWalk);
            Assert.IsNotNull(assets.Sounds.StingerLose);
            Assert.IsFalse(assets.Sounds.DogBark.HasClips);
            Assert.AreEqual(0f, assets.Sounds.StingerWin.SpatialBlend);
            Assert.IsNotNull(assets.IndicatorStyle);
            Assert.IsNotNull(assets.LurePrefab.GetComponent<ImpactSound>());
        }

        [Test]
        public void LoadOrCreate_KeepsClipsUserAssigned()
        {
            var first = GreyboxAssets.LoadOrCreate(TempRoot);
            var clip = AudioClip.Create("bark", 100, 1, 44100, false);
            AssetDatabase.AddObjectToAsset(clip, first.Sounds.DogBark);
            first.Sounds.DogBark.SetClips(new[] { clip });
            EditorUtility.SetDirty(first.Sounds.DogBark);
            AssetDatabase.SaveAssets();

            var second = GreyboxAssets.LoadOrCreate(TempRoot);

            Assert.AreSame(first.Sounds.DogBark, second.Sounds.DogBark);
            Assert.IsTrue(second.Sounds.DogBark.HasClips);
        }

        [Test]
        public void LoadOrCreate_WithVietnameseFont_CreatesFontAssetWithFallback()
        {
            GreyboxAssets.EnsureFolder($"{TempRoot}/Art/Fonts");
            Assert.IsTrue(AssetDatabase.CopyAsset(LiberationTtf, $"{TempRoot}/Art/Fonts/BeVietnamPro-Regular.ttf"));

            var assets = GreyboxAssets.LoadOrCreate(TempRoot);

            Assert.IsNotNull(assets.UiFont);
            Assert.IsTrue(assets.UiFont.fallbackFontAssetTable.Any(f => f != null));
        }
    }
}
