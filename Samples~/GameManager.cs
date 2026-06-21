using Singletons;

namespace Singletons.Samples
{
	// To configure the behaviour for this type, add this script to any object in the scene,
	// then go to Edit -> Project Settings -> Singletons.
	public class GameManager : Singleton<GameManager>
	{
		// Initialize() is called when this object is assigned as the main instance.
		// Typically only called once per instance during Awake(),
		// but may be called again if an existing instance is assigned as main.
		protected override void Initialize()
		{

		}
	}
}
