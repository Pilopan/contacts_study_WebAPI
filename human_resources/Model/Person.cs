namespace human_resources.Model
{
    public class Person
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string SurName { get; set; }
        public Person (int id, string firstName, string lastName, string surName)
        {
            Id = id;
            FirstName = firstName;
            LastName = lastName;
            SurName = surName;
        }
    }
}
