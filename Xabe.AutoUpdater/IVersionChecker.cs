using System.Threading.Tasks;

namespace Xabe.AutoUpdater
{
    public interface IVersionChecker
    {
        Task<string> GetInstalledVersionNumber();
    }
}