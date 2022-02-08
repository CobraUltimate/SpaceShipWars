using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class SpritesEngine : MonoBehaviour
{

    public Image SpriteSelector;
    public Button NextButton;
    public Button PreviousButton;
    private int SpriteIndex;
    private List<Sprite> Sprites = new List<Sprite>();
    private Dictionary<string, ImageInfo> ImageInfoByPath = new Dictionary<string, ImageInfo>();

    [DllImport("__Internal")]
    private static extern void FireUpdateSprite();

    [DllImport("__Internal")]
    private static extern void ListenToUserImages();

    public void FireUpdateSpriteFromJS() => SpritesEngine.FireUpdateSprite();

    // Start is called before the first frame update
    void Start()
    {
        SpritesEngine.ListenToUserImages();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnUserImagesCollectionChanged(string args)
    {
        ImageInfo imageInfo = JsonUtility.FromJson<ImageInfo>(args);
        Texture2D texture = new Texture2D(1, 1);
        texture.LoadImage(Convert.FromBase64String(imageInfo.ImageBase64));
        texture.Apply();
        imageInfo.Sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(.5f, .5f));
        if (this.ImageInfoByPath.ContainsKey(imageInfo.StoragePath))
        {
            this.OnUserImagesCollectionItemChanged(imageInfo);
            return;
        }
        this.OnUserImagesCollectionItemAdded(imageInfo);
        if (this.SpriteSelector.sprite is null)
        {
            this.SpriteIndex = 0;
            this.UpdateCurrentSprite();

        }
        this.UpdateButtons();
    }

    public void OnUserImagesCollectionItemAdded(ImageInfo newImage)
    {
        this.ImageInfoByPath[newImage.RealtimeDatabaseId] = newImage;
        this.Sprites.Add(newImage.Sprite);
    }

    public void OnUserImagesCollectionItemChanged(ImageInfo newImage)
    {
        ImageInfo oldImage = this.ImageInfoByPath[newImage.RealtimeDatabaseId];
        int oldImageIndex = this.Sprites.IndexOf(oldImage.Sprite);
        this.Sprites[oldImageIndex] = newImage.Sprite;
        UpdateCurrentSprite();
    }

    public void OnUserImagesCollectionItemDeleted(string imagePath)
    {
        Console.WriteLine(imagePath);
    }

    private void UpdateButtons()
    {
        this.NextButton.interactable = this.SpriteIndex < this.Sprites.Count - 1 && this.Sprites.Count > 0;
        this.PreviousButton.interactable = this.SpriteIndex > 0 && this.Sprites.Count > 0;
    }

    public void OnNextButtonClicked()
    {
        this.SpriteIndex++;
        this.UpdateCurrentSprite();
        this.UpdateButtons();
    }

    public void OnPreviousButtonClicked()
    {
        this.SpriteIndex--;
        this.UpdateCurrentSprite();
        this.UpdateButtons();
    }

    public void UpdateCurrentSprite()
    {
        this.SpriteSelector.sprite = this.Sprites[this.SpriteIndex];
        MultiplayerEngine.CloudStoragePlayerImagePath = this.ImageInfoByPath.Values.First(imageInfo => imageInfo.Sprite == this.SpriteSelector.sprite).StoragePath;
    }

    [Serializable]
    public class ImageInfo
    {
        [SerializeField]
        public string StoragePath;
        [SerializeField]
        public string ImageBase64;
        [SerializeField]
        public string RealtimeDatabaseId;
        public Sprite Sprite;

        public ImageInfo() { }

    }

}
