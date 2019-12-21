using MiniJSON;
using UnityEngine;

public class ParseJob : ThreadedJob
{
	public string InData;
	public object OutData;

	protected override void ThreadFunction()
	{
		OutData = Json.Deserialize (InData);
	}
//	protected override void OnFinished()
//	{
//		Debug.Log("Json data is ready");
//	}
}