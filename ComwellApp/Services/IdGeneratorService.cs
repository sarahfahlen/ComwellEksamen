namespace ComwellApp.Services
{
    public class IdGeneratorService
    {
        // Generisk metode der virker på alle lister med integer-ID’er
        public int GenererNytId<T>(List<T> liste, Func<T, int> idSelector)
        {
            if (liste == null || liste.Count == 0)
                return 1; // hvis listen er tom, starter vi ved 1

            return liste.Max(idSelector) + 1; // ellers finder vi højeste ID og lægger 1 til
        }
    }
}