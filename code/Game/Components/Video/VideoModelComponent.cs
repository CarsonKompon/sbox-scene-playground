using Sandbox;

namespace Home;

public class VideoModelComponent : DynamicTextureComponent
{
    [Property] public ModelComponent Model { get; set; }
    [Property] public string MaterialName { get; set; } = "Screen";

    Material ScreenMaterial;

    public override void OnStart()
    {
        ScreenMaterial = Cloud.Material( "carsonk.screen_tv" ).CreateCopy();
    }

    public override void OnPostEffect()
    {
        if ( ScreenMaterial.GetTexture( "Color" ) != OutputTexture )
        {
            ScreenMaterial.Set( "Color", OutputTexture );
            if ( string.IsNullOrEmpty( MaterialName ) )
            {
                Model.SceneObject.SetMaterialOverride( ScreenMaterial );
            }
            else
            {
                Model.SceneObject.SetMaterialOverride( ScreenMaterial, MaterialName );
            }
        }
    }
}