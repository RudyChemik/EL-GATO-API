namespace ElGato_API.ModelsMongo.Diet
{
    public class MealPlan
    {
        public string Name { get; set; }
        public int PublicId { get; set; }
        public List<Ingridient> Ingridient { get; set; } = new List<Ingridient>();
    }
}
