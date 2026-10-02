using DogHeist.EditorTools.Greybox;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace DogHeist.Tests.EditMode
{
    /// <summary>
    /// Mỗi test dựng màn vào một scene trống mới (Test Runner tự khôi phục scene của người dùng sau khi chạy xong).
    /// Asset tạo ra nằm trong thư mục tạm và bị xóa sau mỗi test.
    /// </summary>
    public abstract class GreyboxSceneTestBase
    {
        protected const string TempAssetRoot = "Assets/_Project/Tests/_GreyboxSceneTemp";

        protected Scene Scene { get; private set; }

        [SetUp]
        public void CreateEmptyScene() =>
            Scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        [TearDown]
        public void DeleteTempAssets() => AssetDatabase.DeleteAsset(TempAssetRoot);

        internal GreyboxBuildResult Build() =>
            GreyboxLevelBuilder.BuildInto(Scene, new GreyboxBuildOptions { AssetRoot = TempAssetRoot });

        protected static Object ReadReference(Object target, string propertyPath) =>
            new SerializedObject(target).FindProperty(propertyPath).objectReferenceValue;
    }
}
