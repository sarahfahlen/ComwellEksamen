namespace ComwellApp.Services
{
    public class IdGeneratorService
    {
        // Generisk metode
        public int GenererNytId<T>(List<T> liste, Func<T, int> idSelector)
        {
            if (liste == null || liste.Count == 0)
                return 1;
            return liste.Max(idSelector) + 1;
        }

        // 🔧 Brug denne til unikt delmål-ID på tværs af hele planen
        public int GenererNytDelmaalId(Shared.Elevplan plan)
        {
            var eksisterendeIds = plan.ListPerioder
                .SelectMany(p => p.ListMaal)
                .SelectMany(m => m.ListDelmaal)
                .Select(d => d.Id)
                .Where(id => id > 0)
                .ToList();

            Console.WriteLine($"📊 Delmål ID'er i hele planen: [{string.Join(", ", eksisterendeIds)}]");

            if (!eksisterendeIds.Any())
                return 1;

            return eksisterendeIds.Max() + 1;
        }
    }
}