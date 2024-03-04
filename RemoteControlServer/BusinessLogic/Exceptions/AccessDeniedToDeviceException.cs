namespace RemoteControlServer.BusinessLogic.Exceptions
{
    public class AccessDeniedToDeviceException : Exception
    {
        public AccessDeniedToDeviceException() : base("User token is invalid for this device") { }
    }
}
