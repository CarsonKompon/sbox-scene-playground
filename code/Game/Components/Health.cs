
using System.Collections.Generic;
using Sandbox;

[Category( "Quest" )]
[Title( "Health Component" )]
[Icon( "local_hospital", "red", "white" )]
public sealed class HealthComponent : BaseComponent, BaseComponent.ITriggerListener
{
    [Property] public GameObject ParentObject { get; set; }
    [Property] public float Health { get; set; } = 100f;
    [Property] public float MaxHealth { get; set; } = 100f;
    [Property] public float HealRate { get; set; } = 1f;
    [Property] public float HealDelay { get; set; } = 1f;
    [Property] public float ContactDamage { get; set; } = 10f;
    [Property] public float ContactDamageDelay { get; set; } = 1f;

    public delegate void DeathEvent();
    public event DeathEvent OnDeath;

    public delegate void DamageEvent();
    public event DamageEvent OnDamage;

    TimeSince lastHeal = 0f;
    Dictionary<GameObject, TimeSince> contactDamageTimers = new();

    public override void Update()
    {
        if ( lastHeal > HealDelay )
        {
            Heal( HealRate * Time.Delta );
        }
    }

    public void Heal( float amount )
    {
        Health = Math.Clamp( Health + amount, 0f, MaxHealth );
        lastHeal = 0f;
    }

    public void Damage( float amount )
    {
        Health = Math.Clamp( Health - amount, 0f, MaxHealth );

        if ( Health <= 0f )
        {
            OnDeath?.Invoke();
            ParentObject?.Destroy();
        }
    }

    void ITriggerListener.OnTriggerEnter( Collider other )
    {
        //        Log.Info( $"{other.GameObject.Name} entered {GameObject.Name}" );
        var health = other.GameObject.GetComponent<HealthComponent>();
        if ( health != null && !other.GameObject.Tags.HasAny( GameObject.Tags.TryGetAll().ToHashSet() ) )
        {
            if ( contactDamageTimers.ContainsKey( other.GameObject ) && contactDamageTimers[other.GameObject] < ContactDamageDelay )
            {
                return;
            }
            Log.Info( $"{other.GameObject.Name} damaged {GameObject.Name} for {ContactDamage} damage" );
            health.Damage( ContactDamage );
            contactDamageTimers[other.GameObject] = 0f;
        }
    }

    void ITriggerListener.OnTriggerExit( Collider other )
    {

    }
}