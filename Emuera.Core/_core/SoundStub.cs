namespace MinorShift.Emuera.Runtime.Utils;

// Sound.WMP.cs / Sound.NAudio.cs（COM/NAudio，平台相关）已从无头核心排除。
// 引擎（Instraction.Child 的 PLAYSOUND 等）按签名引用 Sound；分析不会执行其方法体。
internal sealed class Sound
{
	public void play(string filename, int repeat = 1) { }
	public void stop() { }
	public void close() { }
	public bool isPlaying() => false;
	public void setVolume(int volume) { }
}
