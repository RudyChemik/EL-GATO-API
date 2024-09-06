namespace ElGato_API.VM.Diet
{
    public class RemoveIngridientVM
    {
        public int MealPublicId { get; set; }
        public string IngridientId { get; set; }
        public string IngridientName { get; set; }
        public double WeightValue { get; set; }
        public DateTime Date { get; set; }
    }
}
