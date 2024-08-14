namespace ilyvion.Laboratory;

public static partial class CustomBackCompatibility
{
    private class ExposableDummy : IExposable
    {
        public ExposableDummy()
        {
        }
        public ExposableDummy(object one)
        {
        }
        public ExposableDummy(object one, object two)
        {
        }
        public ExposableDummy(object one, object two, object three)
        {
        }
        public ExposableDummy(object one, object two, object three, object four)
        {
        }
        public ExposableDummy(object one, object two, object three, object four, object five)
        {
        }
        public ExposableDummy(
            object one, object two, object three, object four, object five, object six)
        {
        }
        public ExposableDummy(
            object one,
            object two,
            object three,
             object four,
             object five,
             object six,
             object seven)
        {
        }

        public void ExposeData()
        {
        }
    }
}
