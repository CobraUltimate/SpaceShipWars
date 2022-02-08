using Assets.Scenes.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpaceShip : MonoBehaviour, IColdownActionListener
{

    public float RadiansRotationSpeed;
    public float UnitsMovementSpeed;

    public List<ISpaceShipListener> Listeners = new List<ISpaceShipListener>();

    public Explosion PrefabExplosion;

    #region Dash

    public Slider DashProgressBar;
    public double DashColdownTime;
    public double DashDuration;
    public float DashSpeedMultiplier;
    private ColdownAction DashColdownAction;

    #endregion

    #region Fire

    public double FireColdownTime;
    public double FireDuration;
    private ColdownAction FireColdownAction;
    public Bullet PrefabBullet;
    public Bullet CurrentBullet;
    public bool Firing => this.FireColdownAction.DoingAction;
    #endregion


    // Start is called before the first frame update
    void Start()
    {
        this.DashColdownAction = ColdownAction.CreateComponent(this.gameObject, this.DashColdownTime, this.DashDuration, KeyCode.Space, this.DashProgressBar);
        this.FireColdownAction = ColdownAction.CreateComponent(this.gameObject, this.FireColdownTime, this.FireDuration, KeyCode.Mouse0, null);
        this.FireColdownAction.Listeners.Add(this);
        Console.WriteLine("0 ----->" + this.PrefabExplosion);
    }

    // Update is called once per frame
    void Update()
    {
        this.UpdateRotation();
        this.UpdatePosition();
        this.UpdateFire();
    }

    private void UpdateRotation()
    {
        if (this.DashColdownAction.DoingAction) return;
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 mousePositionRelativeToPlayer = new Vector2(mousePosition.x - this.transform.position.x, mousePosition.y - this.transform.position.y);
        float mouseRotation = Vector2.SignedAngle(Vector2.up, mousePositionRelativeToPlayer.normalized);
        if (mousePositionRelativeToPlayer.x > 0) mouseRotation += 360;
        float playerRotation = this.transform.rotation.eulerAngles.z;

        float mouseRotationBoundToOrigin = mouseRotation - playerRotation;
        if (mouseRotationBoundToOrigin < 0) mouseRotationBoundToOrigin += 360;

        if (Math.Abs(mouseRotationBoundToOrigin) < 5) return;
        float rotationDirection = mouseRotationBoundToOrigin < 180 ? 1 : -1;

        this.transform.Rotate(Vector3.forward, Time.deltaTime * this.RadiansRotationSpeed * rotationDirection);
    }

    private void UpdatePosition()
    {
        Vector3 movement = Vector3.up * this.UnitsMovementSpeed * Time.deltaTime;
        if (this.DashColdownAction.DoingAction) movement *= DashSpeedMultiplier;
        this.transform.Translate(movement, Space.Self);
    }

    private void UpdateFire()
    {
        if (this.FireColdownAction.DoingAction)
        {
            this.CurrentBullet = Bullet.CreateComponent(this, this.PrefabBullet);
        }
    }

    public void OnBulletCollision()
    {
        Explosion explosion = GameObject.Instantiate(this.PrefabExplosion, this.transform.position, this.transform.rotation);
        explosion.GetComponent<Animator>().enabled = true;
        this.Listeners.ForEach(listener => listener.OnSpaceShipDestroyed());
        GameObject.Destroy(this.gameObject);
    }

    public void OnActionStart()
    {
        this.Listeners.ForEach(listener => listener.OnSpaceShipFireStart());
    }

}
public interface ISpaceShipListener
{

    void OnSpaceShipDestroyed();
    void OnSpaceShipFireStart();

}
