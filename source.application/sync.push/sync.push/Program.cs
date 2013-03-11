namespace sync.push
{
    internal class Program
    {
        private static void Main(string[] args) {
            new Integration(args[0])
                .Push();
        }
    }
}