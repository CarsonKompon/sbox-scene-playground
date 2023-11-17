using System.Text.Json;
using Home.UI;
using Sandbox;

namespace Home;

public sealed class Interactable : BaseComponent
{
	[Property] public GameObject InteractPrompt { get; set; }
	[Property] public string InteractPromptText { get; set; } = "Interact";

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

	public void Interact( HomePlayer player )
	{
		OnInteract?.Invoke( player );
	}
}
