using PhiLibrary197.CloudSave;

namespace CeruleanToolkit.Core.Interfaces;

public interface IPhigrosService
{
    Task<int> BindWithSessionTokenAsync();

    Task<int> BindWithTaptapAsync();

    int Unbind(AppConfig config);

    string ParseStatus(ScoreStatus status);

    string ParseDifficulty(Difficulty difficulty);

    Task<(string NickName, string Rks, string Intro, string Data, string Challenge, bool ShowIntro)> GetUserInfoAsync(bool showIntro = true);

    Task<int> GetB30Async();
}