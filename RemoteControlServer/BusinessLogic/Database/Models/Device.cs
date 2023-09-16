namespace RemoteControlServer.Data.Models
{
    public class Device
    {
        public int Id { get; set; }        

        public string Hwid { get; set; }

        public int UserId { get; set; }

        public virtual User User { get; set; }
    }
}
