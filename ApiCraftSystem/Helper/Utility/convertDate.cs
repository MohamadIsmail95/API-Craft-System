namespace ApiCraftSystem.Helper.Utility
{
    public static class convertDate
    {
        public static DateTime convertDateToArabStandardDate(DateTime dateTime)
        {
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById("Arab Standard Time");
            dateTime = TimeZoneInfo.ConvertTimeFromUtc(dateTime, timeZone);

            return dateTime;
        }

    }
}
