using Sandbox;

namespace Home;

public class StationaryMovement : Movement
{
	[Property] public bool IsSitting { get; set; } = false;
	[Property, Range( 0, 2 )] public int SitAnim { get; set; }
	[Property, Range( 0, 3 )] public int SitPoseAnim { get; set; }

	Model citizenModel;

	public override void Update()
	{
		Player ??= GetComponentInParent<HomePlayer>( true, true );
		characterController ??= GetComponentInParent<CharacterController>( true, true );

		if ( characterController == null && GameObject.Transform.LocalPosition == Vector3.Zero ) return;

		characterController.Transform.Position = GameObject.Transform.Position;
		GameObject.Transform.LocalPosition = Vector3.Zero;
		characterController.Velocity = Vector3.Zero;
	}

	public override void UpdateAnimations( CitizenAnimation helper )
	{
		helper.WithWishVelocity( WishVelocity );
		if ( IsSitting )
		{
			helper.Target.Set( "sit", SitAnim );
			helper.Target.Set( "sit_pose", SitPoseAnim );
		}
	}

	public override void OnDisabled()
	{
		CleanUp();
	}

	public override void OnDestroy()
	{
		CleanUp();
	}

	void CleanUp()
	{
		if ( IsSitting )
		{
			Player.AnimationHelper.Target.Set( "sit", 0 );
			Player.AnimationHelper.Target.Set( "sit_pose", 0 );
		}
	}

}
