using Sandbox;

public sealed class DestroyAfter : BaseComponent
{
	[Property] public float DestroyTime { get; set; } = 1f;

	float _timer;

	public override void Update()
	{
		_timer += Time.Delta;
		if ( _timer >= DestroyTime )
		{
			GameObject.Destroy();
		}
	}
}
