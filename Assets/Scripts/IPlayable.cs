
public interface IPausable {

	/// <summary>
	/// Pause the IPausable element until OnResumeGame is called
	/// </summary>
	void OnPauseGame();
	
	/// <summary>
	/// Resume the IPausable element 
	/// </summary>
	void OnResumeGame();
}
