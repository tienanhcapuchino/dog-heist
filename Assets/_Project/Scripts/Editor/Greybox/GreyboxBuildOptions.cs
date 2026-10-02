namespace DogHeist.EditorTools.Greybox
{
    internal sealed class GreyboxBuildOptions
    {
        /// <summary>Thư mục chứa config, material, prefab. Test dùng thư mục tạm để không đụng asset thật.</summary>
        public string AssetRoot { get; set; } = GreyboxAssets.DefaultAssetRoot;
    }
}
