using System.Text.Json.Serialization;
using System.Text.Json;

namespace WebApi
{
    public class Foo : IDisposable
    {
        private static int _instanceCounter = 0;

        public int InstanceNumber { get; }

        public Foo()
        {
            InstanceNumber = ++_instanceCounter;
            Console.WriteLine($"Created Foo# {InstanceNumber}");
        }
        public void Dispose()
        {
            Console.WriteLine($"Disposing Foo# {InstanceNumber}");
            --_instanceCounter;
        }
    }
}