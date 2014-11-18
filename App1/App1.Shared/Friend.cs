
namespace Swooper
{
    class Friend
    {
        public string Name { get; set; }
        public int Id { get; set; }

        public int Online { get; set; }
        public Friend(string name,  int id,int online)
        {
            Name = name;
            Id = id;
            Online = online;
        }
        public Friend()
        {
        }

        public string getName()
        {
            return Name;
        }
    }
}
