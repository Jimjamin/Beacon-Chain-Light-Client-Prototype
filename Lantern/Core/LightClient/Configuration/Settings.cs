
namespace Lantern
{
    /// <summary>
    /// Relevant configurable settings for the light client.
    /// </summary>
    public class Settings
    {
        private string serverUrl;
        private string lightClientApiUrl = "http://localhost:5001";
        private int network = 0;
        
        public Settings(string server)
        {
            serverUrl = server;
        }

        public string ServerUrl { get; set; }
        public string LightClientApiUrl { get; set; }
        public int Network { get; set; }
    }
}
