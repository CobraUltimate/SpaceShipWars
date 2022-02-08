using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{

    private int BlinckCounter;
    private double LastBlinckTime;
    public double BlinckTime;
    public Sprite BulletAdviceSprite;
    public Sprite BulletSprite;
    private SpaceShip Shooter;

    public static Bullet CreateComponent(SpaceShip spaceShip, Bullet prefabBullet)
    {
        Vector3 rotationEuler = spaceShip.transform.rotation.eulerAngles;
        Quaternion bulletRotation = Quaternion.Euler(rotationEuler.x, rotationEuler.y, rotationEuler.z - 90);
        Vector3 playerDirection = spaceShip.transform.rotation * Vector3.up;
        float bulletOffset = spaceShip.PrefabBullet.GetComponent<SpriteRenderer>().size.x * 2.5f;
        Vector3 bulletPosition = spaceShip.transform.position + (playerDirection * bulletOffset);
        Bullet newBullet = Instantiate(prefabBullet, bulletPosition, bulletRotation, spaceShip.transform);
        newBullet.Shooter = spaceShip;
        newBullet.GetComponent<Bullet>().enabled = true;
        return newBullet;
    }

    // Start is called before the first frame update
    void Start()
    {
        this.BlinckCounter = 0;
        this.LastBlinckTime = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time - this.LastBlinckTime > this.BlinckTime)
        {
            this.BlinckCounter++;
            this.LastBlinckTime = Time.time;
        }
        if (BlinckCounter == 6)
        {
            this.GetComponent<SpriteRenderer>().sprite = BulletSprite;
            this.GetComponent<BoxCollider2D>().enabled = true;
        }
        this.GetComponent<Renderer>().enabled = this.BlinckCounter % 2 == 0;
        if (BlinckCounter == 7)
        {
            Destroy(this.gameObject);
            Destroy(this);
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.name == "SpaceShipPlayer" && collision.gameObject != this.Shooter.gameObject)
        {
            collision.gameObject.GetComponent<SpaceShip>().OnBulletCollision();
        }
    }

}
