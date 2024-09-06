namespace ElGato_API.VM.Diet
{
    public class UpdateIngridientVM
    {
        public string IngridientName { get; set; }
        public string IngridientId { get; set; }
        public double IngridientWeightOld { get; set; }
        public double IngridientWeightNew { get; set; }
        public DateTime Date {  get; set; }
        public int MealPublicId { get; set; }
    }
}
