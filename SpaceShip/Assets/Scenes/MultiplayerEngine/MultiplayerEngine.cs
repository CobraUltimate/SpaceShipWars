
using UnityEngine;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System;

public class MultiplayerEngine : MonoBehaviour, ISpaceShipListener
{

    public static string CloudStoragePlayerImagePath;
    public SpaceShip CurrentPlayerSpaceShip;
    public Bullet PrefabBullet;
    public Explosion PrefabExplosion;
    public Dictionary<string, SpaceShip> PlayersSpaceShips;
    string PlayerId;
    private bool CurrentPlayerFiring;

    [DllImport("__Internal")]
    private static extern void Save(string path, string dataJson);

    [DllImport("__Internal")]
    private static extern void Delete(string path);

    [DllImport("__Internal")]
    private static extern void Init();

    [DllImport("__Internal")]
    public static extern string GetUserId();

    [DllImport("__Internal")]
    public static extern string GetUserImage(string playerId, string cloudStorageImagePath);

    // Start is called before the first frame update
    void Start()
    {
        Init();
        this.PlayerId = MultiplayerEngine.GetUserId();
        this.PlayersSpaceShips = new Dictionary<string, SpaceShip>();
        this.PlayersSpaceShips[this.PlayerId] = this.CurrentPlayerSpaceShip;
        this.CurrentPlayerSpaceShip.Listeners.Add(this);
        MultiplayerEngine.GetUserImage(this.PlayerId, MultiplayerEngine.CloudStoragePlayerImagePath);
    }

    void OnPlayerAdded(string playerJson)
    {
        Player newPlayer = JsonUtility.FromJson<Player>(playerJson);
        if (newPlayer.Id == this.PlayerId) return;
        Vector2 newPlayerSpaceShipPosition = new Vector2(newPlayer.Position.X, newPlayer.Position.Y);
        Quaternion newPlayerSpaceShipRotation = new Quaternion(newPlayer.Rotation.X, newPlayer.Rotation.Y, newPlayer.Rotation.Z, newPlayer.Rotation.W);
        SpaceShip newPlayerSpaceShip = Instantiate(this.CurrentPlayerSpaceShip, newPlayerSpaceShipPosition, newPlayerSpaceShipRotation);
        newPlayerSpaceShip.GetComponent<SpaceShip>().enabled = false;
        newPlayerSpaceShip.gameObject.tag = "SpaceShip";
        newPlayerSpaceShip.PrefabExplosion = this.PrefabExplosion;
        this.PlayersSpaceShips[newPlayer.Id] = newPlayerSpaceShip;
        MultiplayerEngine.GetUserImage(newPlayer.Id, newPlayer.CloudStoragePlayerImagePath);
    }

    void OnPlayerImageDownloaded(string palyerImageInfoJson)
    {
        PlayerImageInfo playerImageInfo = JsonUtility.FromJson<PlayerImageInfo>(palyerImageInfoJson);
        Texture2D texture = new Texture2D(1, 1);
        texture.LoadImage(Convert.FromBase64String(playerImageInfo.ImageBase64));
        texture.Apply();
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(.5f, .5f));
        this.PlayersSpaceShips[playerImageInfo.PlayerId].GetComponent<SpriteRenderer>().sprite = sprite;
    }

    void OnPlayerChanged(string playerJson)
    {
        Player updatedPlayer = JsonUtility.FromJson<Player>(playerJson);
        if (updatedPlayer.Id == this.PlayerId) return;
        Position updatedPlayerPosition = updatedPlayer.Position;
        Rotation updatedPlayerRotation = updatedPlayer.Rotation;
        Vector3 positionVector = new Vector3(updatedPlayerPosition.X, updatedPlayerPosition.Y, -1);
        Quaternion rotationQuaternion = new Quaternion(updatedPlayerRotation.X, updatedPlayerRotation.Y, updatedPlayerRotation.Z, updatedPlayerRotation.W);
        this.PlayersSpaceShips[updatedPlayer.Id].transform.position = positionVector;
        this.PlayersSpaceShips[updatedPlayer.Id].transform.rotation = rotationQuaternion;
        if (updatedPlayer.Firing)
        {
            Bullet bullet = Bullet.CreateComponent(this.PlayersSpaceShips[updatedPlayer.Id], this.PrefabBullet);
            this.PlayersSpaceShips[updatedPlayer.Id].CurrentBullet = bullet;
        }
    }

    void OnPlayerRemoved(string playerJson)
    {
        Console.WriteLine("RemovedPlayer ---> " + playerJson);
        Player removedPlayer = JsonUtility.FromJson<Player>(playerJson);
        if (removedPlayer.Id == this.PlayerId) return;
        Console.WriteLine("RemovedPlayerId ---> " + removedPlayer.Id);
        SpaceShip RemovedSpaceShip = this.PlayersSpaceShips[removedPlayer.Id];
        Explosion explosion = GameObject.Instantiate(this.PrefabExplosion, RemovedSpaceShip.transform.position, RemovedSpaceShip.transform.rotation);
        explosion.GetComponent<Animator>().enabled = true;
        GameObject.Destroy(this.PlayersSpaceShips[removedPlayer.Id].gameObject);
        this.PlayersSpaceShips.Remove(removedPlayer.Id);
    }

    void FixedUpdate()
    {
        if (this.CurrentPlayerSpaceShip is null) return;
        Vector3 playerSpaceShipPosition = CurrentPlayerSpaceShip.transform.position;
        Quaternion playerSpaceShipRotation = CurrentPlayerSpaceShip.transform.rotation;
        Position playerPosition = new Position(playerSpaceShipPosition.x, playerSpaceShipPosition.y);
        Rotation playerRotation = new Rotation(playerSpaceShipRotation.x, playerSpaceShipRotation.y, playerSpaceShipRotation.z, playerSpaceShipRotation.w);
        Player player = new Player(this.PlayerId, playerPosition, playerRotation, this.CurrentPlayerFiring, MultiplayerEngine.CloudStoragePlayerImagePath);
        string playerJson = JsonUtility.ToJson(player);
        MultiplayerEngine.Save($"/players/{player.Id}", playerJson);
        if (this.CurrentPlayerFiring) this.CurrentPlayerFiring = false;
    }

    public void OnSpaceShipDestroyed()
    {
        MultiplayerEngine.Delete($"/players/{this.PlayerId}");
        this.CurrentPlayerSpaceShip = null;
    }

    public void OnSpaceShipFireStart()
    {
        this.CurrentPlayerFiring = true;
    }

    [Serializable]
    public class Player
    {
        [SerializeField]
        public string Id;
        [SerializeField]
        public Position Position;
        [SerializeField]
        public Rotation Rotation;
        [SerializeField]
        public bool Firing;
        [SerializeField]
        public string CloudStoragePlayerImagePath;

        public Player(string id, Position position, Rotation rotation, bool firing, string cloudStoragePlayerImagePath)
        {
            this.Id = id;
            this.Position = position;
            this.Rotation = rotation;
            this.Firing = firing;
            this.CloudStoragePlayerImagePath = cloudStoragePlayerImagePath;
        }
    }

    [Serializable]
    public class Position
    {
        [SerializeField]
        public float X;
        [SerializeField]
        public float Y;

        public Position() { }

        public Position(float x, float y)
        {
            this.X = x;
            this.Y = y;
        }
    }

    [Serializable]
    public class Rotation
    {
        [SerializeField]
        public float X;
        [SerializeField]
        public float Y;
        [SerializeField]
        public float Z;
        [SerializeField]
        public float W;

        public Rotation() { }

        public Rotation(float x, float y, float z, float w)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
            this.W = w;
        }
    }

    [Serializable]
    public class PlayerImageInfo
    {
        [SerializeField]
        public string PlayerId;
        [SerializeField]
        public string ImageBase64;

        public PlayerImageInfo() { }

    }

}