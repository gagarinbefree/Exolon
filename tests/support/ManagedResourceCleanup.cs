using Godot;
using System;

public partial class ManagedResourceCleanup : RefCounted
{
	public void CollectReleasedResources()
	{
		GC.Collect();
		GC.WaitForPendingFinalizers();
	}
}
