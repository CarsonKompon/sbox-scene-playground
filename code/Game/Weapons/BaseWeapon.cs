using System;
using Sandbox;

public class BaseWeapon : BaseComponent
{
    [Property] QuestPlayer Holder { get; set; }

    public virtual Model Model { get; set; } = Model.Load( "models/weapons/usp/usp.vmdl" );
    public virtual int MaxAmmo => 12;
    public virtual float ReloadTime => 1f;
    public virtual float FireRate => 0.125f;
    public virtual float BaseDamage => 15f;

    public bool Reloading = false;
    public int CurrentClip = 0;

    TimeUntil reloadTimer = 0f;
    AnimatedModelComponent modelComponent;

    public virtual void OnFire() { }

    public override void OnAwake()
    {
        if ( Holder is not null )
        {
            Initialize( Holder );
        }
    }

    public override void OnEnabled()
    {
        if ( modelComponent is not null )
        {
            modelComponent.Enabled = true;
        }
    }

    public override void OnDisabled()
    {
        if ( modelComponent is not null )
        {
            modelComponent.Enabled = false;
        }
    }

    public void Initialize( QuestPlayer player )
    {
        modelComponent = GameObject.AddComponent<AnimatedModelComponent>( false );
        modelComponent.Model = Model;
        modelComponent.Enabled = true;
        modelComponent.BoneMergeTarget = player.Body.GetComponent<AnimatedModelComponent>( false );

        Holder = player;
        GameObject.Parent = player.Body;

        Log.Info( "initialized weapon" );
    }

    public override void Update()
    {
        if ( CurrentClip == 0 && !Reloading )
        {
            Reloading = true;
            reloadTimer = ReloadTime;
        }
        else if ( Reloading && reloadTimer )
        {
            CurrentClip = MaxAmmo;
            Reloading = false;
        }
    }

    public void Attack()
    {
        if ( CurrentClip == 0 )
            return;

        OnFire();

        CurrentClip--;
    }

}