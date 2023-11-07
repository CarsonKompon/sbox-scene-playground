
using Sandbox;

[Category( "Quest" )]
[Title( "Health Component" )]
[Icon( "local_hospital", "red", "white" )]
public sealed class HealthComponent : BaseComponent
{
    [Property] public float Health { get; set; } = 100f;
    [Property] public float MaxHealth { get; set; } = 100f;
    [Property] public float HealRate { get; set; } = 1f;
    [Property] public float HealDelay { get; set; } = 1f;

    public delegate void DeathEvent();
    public event DeathEvent OnDeath;

    public delegate void DamageEvent();
    public event DamageEvent OnDamage;

    TimeSince lastHeal = 0f;

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
    }
}