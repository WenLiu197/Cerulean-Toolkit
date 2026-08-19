using CeruleanToolkit.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CeruleanToolkit.Core.Services;

public class ConfigService : IConfigService
{
    private const string _configFileName = "config.json";

    private readonly string _configDirectory;

    private readonly string _configFilePath;

    private Config _config = new();

    public ConfigService()
    {
        _configDirectory = GetConfigDirectory();
        _configFilePath = Path.Combine(_configDirectory, _configFileName);
        Init();
    }

    /// <summary>
    /// 顾名思义，就是获取配置的实例
    /// </summary>
    /// <returns></returns>
    public Config GetConfig() => _config;

    /// <summary>
    /// 初始化配置，
    /// </summary>
    private void Init()
    {
        if (System.IO.File.Exists(_configFilePath))
        {
            Load();
        }
        else
        {
            Directory.CreateDirectory(_configDirectory);
            Save();
        }
    }

    private void Load()
    {
        try
        {
            string json = File.ReadAllText(_configFilePath);
            Config? loaded = JsonSerializer.Deserialize(json, ConfigJsonContext.Default.Config);
            if (loaded is not null)
            {
                _config = loaded;
            }
        }
        catch (JsonException ex)
        {
            Console.Error.WriteLine($"配置文件损坏，将使用默认配置: {ex.Message}");
            _config = new Config();
            Save();
        }
        catch (IOException ex)
        {
            Console.Error.WriteLine($"无法读取配置文件: {ex.Message}");
        }
    }

    private void Save()
    {
        string json = JsonSerializer.Serialize(_config, ConfigJsonContext.Default.Config);
        File.WriteAllText(_configFilePath, json);
    }

    /// <summary>
    /// 可以根据不同平台获取不同的配置目录
    /// <para>
    /// <i>但只支持 <b>Windows</b> 和 <b>Linux</b></i>
    /// </para>
    /// </summary>
    /// <returns>配置目录的完整路径</returns>
    /// <exception cref="PlatformNotSupportedException">当前平台不是 Windows 或 Linux</exception>
    private static string GetConfigDirectory()
    {
        // 返回 %LOCALAPPDATA%\Cerulean Toolkit\
        if (OperatingSystem.IsWindows())
        {
            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            return Path.Combine(localAppData, "Cerulean Toolkit");
        }

        // 返回 ~/.config/cerulean-toolkit
        if (OperatingSystem.IsLinux())
        {
            string? configHome = Environment.GetEnvironmentVariable("XDG_CONFIG_HOME");
            string baseDirectory = string.IsNullOrEmpty(configHome)
                ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config")
                : configHome;

            return Path.Combine(baseDirectory, "cerulean-toolkit");
        }

        throw new PlatformNotSupportedException("仅支持 Windows 与 Linux 平台");
    }
}

/// <summary>
/// 提供 <see cref="Config"/> 类型的序列化上下文。
/// </summary>
[JsonSourceGenerationOptions(
    WriteIndented = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(Config))]
public sealed partial class ConfigJsonContext : JsonSerializerContext { }