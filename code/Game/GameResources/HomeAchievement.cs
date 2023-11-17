using Sandbox;
using System.Collections.Generic;

namespace Home;

[GameResource( "Home Achievement", "achieve", "A achievement for Home", Category = "Home" )]
public partial class HomeAchievement : GameResource
{
    public string Name { get; set; } = "New Achievement";
    public int Goal { get; set; } = 1;
    public string[] Rewards { get; set; }
}