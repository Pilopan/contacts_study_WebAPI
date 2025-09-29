using contacts.Model;
using human_resources.Model;
using System.Collections.Concurrent;

namespace contacts.Services
{
    public static class IdGenereationService
    {
        public static int GenerateNextIdValue(ConcurrentDictionary<int, Contact> _contact)
        {
            return _contact.Any() ? _contact.Keys.Max() + 1 : 1;
        }
    }
}
