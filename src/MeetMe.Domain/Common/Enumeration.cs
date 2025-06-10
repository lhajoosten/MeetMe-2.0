namespace MeetMe.Domain.Common
{
    public abstract class Enumeration : IComparable
    {
        public int Id { get; private set; }
        public string Name { get; private set; }

        protected Enumeration(int value, string name)
        {
            Id = value;
            Name = name;
        }

        public override string ToString() => Name;

        public static IEnumerable<T> GetAll<T>() where T : Enumeration
        {
            return typeof(T).GetFields(System.Reflection.BindingFlags.Public |
                                       System.Reflection.BindingFlags.Static |
                                       System.Reflection.BindingFlags.DeclaredOnly)
                            .Select(f => f.GetValue(null))
                            .Cast<T>();
        }

        public static T FromValue<T>(int value) where T : Enumeration
        {
            var matchingItem = GetAll<T>().FirstOrDefault(item => item.Id == value);
            if (matchingItem == null)
                throw new ArgumentException($"No enumeration found with value {value} in {typeof(T).Name}");
            return matchingItem;
        }

        public static T FromName<T>(string name) where T : Enumeration
        {
            var matchingItem = GetAll<T>().FirstOrDefault(item => item.Name == name);
            if (matchingItem == null)
                throw new ArgumentException($"No enumeration found with name {name} in {typeof(T).Name}");
            return matchingItem;
        }


        public override bool Equals(object obj)
        {
            if (obj is not Enumeration otherValue)
                return false;

            return GetType().Equals(obj.GetType()) && Id.Equals(otherValue.Id);
        }

        public override int GetHashCode() => Id.GetHashCode();

        public int CompareTo(object other) => Id.CompareTo(((Enumeration)other).Id);
    }
}