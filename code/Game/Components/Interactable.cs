using System.Text.Json;
using Home.UI;
using Sandbox;

namespace Home;

[Title( "Interactable" )]
[Category( "Home" )]
[Icon( "touch_app", "red", "white" )]
public sealed class Interactable : BaseComponent, INetworkSerializable
{
	[Property] public GameObject InteractPrompt { get; set; }
	[Property] public string InteractPromptText { get; set; } = "Interact";
	[Property] public bool CanInteract { get; private set; } = true;

	string originalPromptText = "";

	public delegate void OnInteractDelegate( HomePlayer player );
	public OnInteractDelegate OnInteract { get; set; }

	public GameObject CreateUIPrompt()
	{
		GameObject obj;
		if ( InteractPrompt is null )
		{
			var defaultPrefab = ResourceLibrary.Get<PrefabFile>( "prefabs/ui/basicinteractprompt.object" );
			var prompt = new GameObject();
			prompt.Deserialize( defaultPrefab.RootObject );
			obj = prompt;
		}
		else
		{
			obj = SceneUtility.Instantiate( InteractPrompt );
		}

		if ( obj.GetComponent<InteractPrompt>() is InteractPrompt promptScript )
		{
			promptScript.Message = InteractPromptText;
		}
		return obj;
	}

	[Broadcast]
	public void SetCanInteract()
	{
		CanInteract = true;
	}

	[Broadcast]
	public void SetCantInteract()
	{
		CanInteract = false;
	}

	[Broadcast]
	public void ChangeText( string text )
	{
		originalPromptText = InteractPromptText;
		InteractPromptText = text;

		if ( InteractPrompt is not null )
		{
			if ( InteractPrompt.GetComponent<InteractPrompt>() is InteractPrompt promptScript )
			{
				promptScript.Message = InteractPromptText;
			}
		}
	}

	[Broadcast]
	public void RestoreText()
	{
		if ( string.IsNullOrEmpty( originalPromptText ) ) return;

		InteractPromptText = originalPromptText;

		if ( InteractPrompt is not null )
		{
			if ( InteractPrompt.GetComponent<InteractPrompt>() is InteractPrompt promptScript )
			{
				promptScript.Message = InteractPromptText;
			}
		}
	}

	public void Interact( HomePlayer player )
	{
		if ( !CanInteract ) return;

		OnInteract?.Invoke( player );
	}

	public void Write( ref ByteStream stream )
	{
		stream.Write( CanInteract );
		stream.Write( InteractPromptText );
	}

	public void Read( ByteStream stream )
	{
		CanInteract = stream.Read<bool>();
		InteractPromptText = stream.Read<string>();
	}
}
