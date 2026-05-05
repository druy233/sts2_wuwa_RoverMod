using Godot;
using System.Collections.Generic;

namespace Rover.Tools;

public static class RoverAudioHelper
{
    private struct ActiveSound
    {
        public AudioStreamPlayer Player;
        public Callable FinishedCb;
    }

    private static readonly StringName SfxBus = new("SFX");
    private static readonly List<AudioStreamPlayer> FreePool = new();
    private static readonly List<ActiveSound> Playing = new();

    public static void PlayOneShot(string resPath, float volumeLinear = 1f)
    {
        // 加载音频资源（GD.Load 自带缓存）
        var stream = GD.Load<AudioStream>(resPath);
        if (stream == null)
        {
            GD.PrintErr($"[RoverAudioHelper] Failed to load audio: {resPath}");
            return;
        }

        // 从池中获取或创建 AudioStreamPlayer
        AudioStreamPlayer player;
        if (FreePool.Count > 0)
        {
            player = FreePool[^1];
            FreePool.RemoveAt(FreePool.Count - 1);
        }
        else
        {
            player = new AudioStreamPlayer();
            player.Bus = SfxBus;
            var tree = Engine.GetMainLoop() as SceneTree;
            tree?.Root.AddChild(player);
        }

        player.Stream = stream;
        player.VolumeDb = Mathf.LinearToDb(Mathf.Pow(volumeLinear, 2f));

        // 使用 Callable 连接 Finished 信号
        Callable finishedCb = Callable.From(() => OnFinished(player));
        player.Connect(AudioStreamPlayer.SignalName.Finished, finishedCb);

        Playing.Add(new ActiveSound { Player = player, FinishedCb = finishedCb });
        player.Play();
    }

    private static void OnFinished(AudioStreamPlayer player)
    {
        for (int i = 0; i < Playing.Count; i++)
        {
            if (Playing[i].Player == player)
            {
                player.Disconnect(AudioStreamPlayer.SignalName.Finished, Playing[i].FinishedCb);
                Playing.RemoveAt(i);
                player.Stop();
                FreePool.Add(player);
                break;
            }
        }
    }
}