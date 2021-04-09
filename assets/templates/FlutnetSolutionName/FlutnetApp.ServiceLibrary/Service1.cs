using Flutnet.ServiceModel;

namespace FlutnetApp.ServiceLibrary
{
    [PlatformService]
    public class Service1
    {
        [PlatformOperation]
        public string Hello(string name)
        {
            return $"Hi, {name}! How are you?";
        }
    }
}