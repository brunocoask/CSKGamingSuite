using System;
using System.Collections.Generic;
using System.Linq;
using NAudio.CoreAudioApi;

namespace CSKGamingSuite;

public sealed record AudioDeviceInfo(string Id, string Name, bool IsDefault);

/// <summary>
/// Gerenciador de Áudio Master e mixer de volume do Windows usando NAudio CoreAudioApi (WASAPI).
/// </summary>
public sealed class AudioManager : IDisposable
{
    private readonly MMDeviceEnumerator _enumerator = new();

    public MMDevice? GetDefaultRenderDevice()
    {
        try
        {
            return _enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
        }
        catch
        {
            return null;
        }
    }

    public List<AudioDeviceInfo> GetPlaybackDevices()
    {
        var list = new List<AudioDeviceInfo>();
        try
        {
            var defaultDev = GetDefaultRenderDevice();
            var devices = _enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);
            foreach (var d in devices)
            {
                list.Add(new AudioDeviceInfo(d.ID, d.FriendlyName, defaultDev?.ID == d.ID));
            }
        }
        catch { }
        return list;
    }

    public float GetMasterVolume()
    {
        try
        {
            var dev = GetDefaultRenderDevice();
            return dev?.AudioEndpointVolume?.MasterVolumeLevelScalar * 100f ?? 50f;
        }
        catch
        {
            return 50f;
        }
    }

    public void SetMasterVolume(float percent)
    {
        try
        {
            var dev = GetDefaultRenderDevice();
            if (dev?.AudioEndpointVolume != null)
            {
                float level = Math.Clamp(percent / 100f, 0f, 1f);
                dev.AudioEndpointVolume.MasterVolumeLevelScalar = level;
            }
        }
        catch { }
    }

    public bool IsMasterMuted()
    {
        try
        {
            var dev = GetDefaultRenderDevice();
            return dev?.AudioEndpointVolume?.Mute ?? false;
        }
        catch
        {
            return false;
        }
    }

    public void SetMasterMute(bool mute)
    {
        try
        {
            var dev = GetDefaultRenderDevice();
            if (dev?.AudioEndpointVolume != null)
            {
                dev.AudioEndpointVolume.Mute = mute;
            }
        }
        catch { }
    }

    public void ToggleMasterMute()
    {
        SetMasterMute(!IsMasterMuted());
    }

    public void Dispose()
    {
        _enumerator.Dispose();
    }
}
