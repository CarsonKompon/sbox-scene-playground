using Sandbox;

namespace Home;

public class Movement : BaseComponent
{

	// References
	[Property] public HomePlayer Player { get; set; }
	[Property] public CharacterController characterController { get; set; }

	public Vector3 WishVelocity { get; protected set; } = Vector3.Zero;

}
