
namespace AdventureWorks.Services.Entities
{
    public partial class CurrencyRate
    {
        public string RateString => $"$1 = {EndOfDayRate} {ToCurrencyCodeObject.CurrencyCode}";
    }
}