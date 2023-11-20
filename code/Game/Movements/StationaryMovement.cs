using Sandbox;

namespace Home;

public class StationaryMovement : Movement
{
	[Property] public SIT_TYPE SitAnim { get; set; }
	[Property] public SIT_POSE SitPoseAnim { get; set; }
	[Property] public GameObject TargetObject { get; set; }

	public enum SIT_TYPE
	{
		STANDING,
		SITTING_CHAIR,
		SITTING_GROUND
	}

	public enum SIT_POSE
	{
		NORMAL,
		LEANING,
		SCRUNCHED,
		CROSSED
	}

	public override void Update()
	{
		base.Update();

		Player ??= GetComponentInParent<HomePlayer>( true, true );
		characterController ??= GetComponentInParent<CharacterController>( true, true );

		if ( characterController == null ) return;

		if ( TargetObject is not null )
		{
			characterController.Transform.Position = TargetObject.Transform.Position;
		}

		characterController.Velocity = Vector3.Zero;
		characterController.Move();

	}

	public override void UpdateAnimations( CitizenAnimation helper )
	{
		helper.WithWishVelocity( WishVelocity );
		if ( SitAnim != SIT_TYPE.STANDING )
		{
			helper.Target.Set( "sit", (int)SitAnim );
			helper.Target.Set( "sit_pose", (int)SitPoseAnim );
		}
	}

	public void SetTarget( GameObject target, SIT_TYPE sitType = SIT_TYPE.STANDING, SIT_POSE sitPose = SIT_POSE.NORMAL )
	{
		TargetObject = target;
		SitAnim = sitType;
		SitPoseAnim = sitPose;
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
		if ( SitAnim != SIT_TYPE.STANDING )
		{
			Player.AnimationHelper.Target.Set( "sit", 0 );
			Player.AnimationHelper.Target.Set( "sit_pose", 0 );
		}
	}

}
