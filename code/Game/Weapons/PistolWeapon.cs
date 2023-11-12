using System;
using Sandbox;

public class PistolWeapon : BaseWeapon
{
    public override Model Model { get; set; } = Model.Load( "weapons/rust_pistol/rust_pistol.vmdl" );
    public override int MaxAmmo => 12;
    public override float ReloadTime => 1.5f;
    public override float FireRate => 0.32f;
    public override float BaseDamage => 15f;
}