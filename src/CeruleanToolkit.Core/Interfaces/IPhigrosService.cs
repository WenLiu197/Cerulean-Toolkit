using PhiLibrary197.CloudSave;

namespace CeruleanToolkit.Core.Interfaces;

public interface IPhigrosService
{
    Task<int> BindWithSessionTokenAsync();
    Task<int> BindWithTaptapAsync();
    int Unbind(AppConfig config);

    Task<(string NickName, string Rks, string Intro, string Data, string Challenge)> GetUserInfoAsync();
}