namespace DeviceCirculationSystem.bean
{
    public class User
    {
        public User(string name)
        {
            this.name = name;
        }

        public string name { get; private set; }
    }
}