namespace study4_be.Services.Response
{
    public class TimeRemainingRespone
    {
        public int Days { get; set; }
        public int Hours { get; set; }
        public int Minutes { get; set; }

        public TimeRemainingRespone(int days, int hours, int minutes)
        {
            Days = days;
            Hours = hours;
            Minutes = minutes;
        }
    }
}
