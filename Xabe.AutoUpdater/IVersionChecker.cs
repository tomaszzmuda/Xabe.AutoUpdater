using System.Threading.Tasks;

namespace Xabe.AutoUpdater
{
    public interface IVersionChecker
    {
        /// <summary>
        ///     Get installed version number
        /// </summary>
        /// <returns>Version number of current running application</returns>
        Task<string> GetInstalledVersionNumber();
    }
}