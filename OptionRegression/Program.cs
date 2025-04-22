using CsvHelper;
using CsvHelper.Configuration.Attributes;
using System.Diagnostics;
using System.Globalization;

namespace OptionRegression;

class Record
{
    [Name("日期")]
    public DateOnly Date { get; set; }
    [Name("收盘")]
    public float ClosingPrice { get; set; }
    [Name("开盘")]
    public float OpeningPrice { get; set; }
    [Name("高")]
    public float HighPrice { get; set; }
    [Name("低")]
    public float LowPrice { get; set; }
    [Name("交易量")]
    public string Volume { get; set; }
    [Name("涨跌幅")]
    public string Percent { get; set; }
}

internal class Program
{
    static double OptionPrice = 0.005;
    static int EachBuyVolumn = 300;
    static double StartMoney = 0;
    static int CurrentHoldVolumn = 0;
    static double CurrentBalance = 0;

    static double LastPrice = 0;
    static double OptionEarned = 0;
    static double StockEarned = 0;

    static void Main(string[] args)
    {
        Console.WriteLine("Option Regression!");

        if (args.Length == 0)
        {
            Console.WriteLine("Usage: <csv file>");
            return;
        }

        if (Path.GetExtension(args[0]).ToLower() != ".csv" || !File.Exists(args[0]))
        {
            Console.WriteLine("Invalid Input File");
            return;
        }

        using (var reader = new StreamReader(args[0]))
        using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
        {
            var records = csv.GetRecords<Record>().ToList();
            records.Reverse();

            StartMoney = EachBuyVolumn * records[0].OpeningPrice;

            TwoDirection(records);

            WeeklyTwoDirection(records);

            LongOnly(records);

            WeeklyLongOnly(records);

            MonthlyLongOnly(records);
        }

    }

    static void TwoDirection(List<Record> records)
    {
        CurrentBalance = 0;
        OptionEarned = 0;
        StockEarned = 0;
        CurrentHoldVolumn = 0;

        Console.WriteLine("====TwoDirection====");

        foreach (var record in records)
        {

            if (CurrentHoldVolumn == 0)
            {
                var diff = record.ClosingPrice - record.OpeningPrice;
                if (diff < -OptionPrice * record.OpeningPrice)
                {
                    // Buy
                    CurrentHoldVolumn = EachBuyVolumn;
                    double price = record.OpeningPrice * (1.0 - OptionPrice);
                    LastPrice = price;
                    CurrentBalance -= EachBuyVolumn * price;
                    Console.WriteLine($"Buy {EachBuyVolumn} at {price}");

                    CurrentBalance += OptionPrice * record.OpeningPrice * EachBuyVolumn; // add call earning
                    OptionEarned += OptionPrice * record.OpeningPrice * EachBuyVolumn;
                }
                else if (diff > OptionPrice * record.OpeningPrice)
                {
                    // Sell
                    CurrentHoldVolumn = -EachBuyVolumn;
                    double price = record.OpeningPrice * (1.0 + OptionPrice);
                    LastPrice = price;
                    CurrentBalance += EachBuyVolumn * price;
                    Console.WriteLine($"Sell {EachBuyVolumn} at {price}");

                    CurrentBalance += OptionPrice * record.OpeningPrice * EachBuyVolumn; // add put earning
                    OptionEarned += OptionPrice * record.OpeningPrice * EachBuyVolumn;
                }
                else
                {
                    CurrentBalance += 2 * OptionPrice * record.OpeningPrice * EachBuyVolumn; // add call and put earning
                    OptionEarned += 2 * OptionPrice * record.OpeningPrice * EachBuyVolumn;
                }

                continue;
            }

            if (CurrentHoldVolumn > 0)
            {
                Debug.Assert(CurrentHoldVolumn == EachBuyVolumn);

                // sell call only
                var diff = record.ClosingPrice - record.OpeningPrice;

                // call selled
                if (diff > OptionPrice * record.OpeningPrice)
                {
                    // Sell
                    CurrentHoldVolumn -= EachBuyVolumn;
                    double price = record.OpeningPrice * (1.0 + OptionPrice);
                    CurrentBalance += EachBuyVolumn * price;
                    double earned = (price - LastPrice) * EachBuyVolumn;
                    StockEarned += earned;
                    Console.WriteLine($"Sell {EachBuyVolumn} at {price} earned {earned}");
                }
                else
                {
                    // don't sell, earn call option
                    CurrentBalance += OptionPrice * record.OpeningPrice * EachBuyVolumn; // add call and put earning
                    OptionEarned += OptionPrice * record.OpeningPrice * EachBuyVolumn;
                }
                continue;
            }

            if (CurrentHoldVolumn < 0)
            {
                Debug.Assert(CurrentHoldVolumn == -EachBuyVolumn);

                // sell put only
                var diff = record.ClosingPrice - record.OpeningPrice;

                // put selled
                if (diff < -OptionPrice * record.OpeningPrice)
                {
                    // buy
                    CurrentHoldVolumn += EachBuyVolumn;
                    double price = record.OpeningPrice * (1.0 - OptionPrice);
                    CurrentBalance -= EachBuyVolumn * price;
                    double earned = (LastPrice - price) * EachBuyVolumn;
                    StockEarned += earned;
                    Console.WriteLine($"Buy {EachBuyVolumn} at {price} earned {earned}");
                }
                else
                {
                    // don't sell, earn put option
                    CurrentBalance += OptionPrice * record.OpeningPrice * EachBuyVolumn; // add call and put earning
                    OptionEarned += OptionPrice * record.OpeningPrice * EachBuyVolumn;
                }
                continue;
            }
        }
        Console.WriteLine($"Start Money: {StartMoney}");
        Console.WriteLine($"Balance: {CurrentBalance}");
        Console.WriteLine($"Option Earned: {OptionEarned}");
        Console.WriteLine($"Stock Earned: {StockEarned}");
        Console.WriteLine($"Current Hold Volumn: {CurrentHoldVolumn}");
    }

    static void LongOnly(List<Record> records)
    {
        CurrentBalance = 0;
        OptionEarned = 0;
        StockEarned = 0;
        CurrentHoldVolumn = 0;

        Console.WriteLine("====LongOnly====");
        foreach (var record in records)
        {

            if (CurrentHoldVolumn == 0)
            {
                var diff = record.ClosingPrice - record.OpeningPrice;
                if (diff < -OptionPrice * record.OpeningPrice)
                {
                    // Buy
                    CurrentHoldVolumn = EachBuyVolumn;
                    double price = record.OpeningPrice * (1.0 - OptionPrice);
                    LastPrice = price;
                    CurrentBalance -= EachBuyVolumn * price;
                    Console.WriteLine($"Buy {EachBuyVolumn} at {price}");

                    //CurrentBalance += OptionPrice * record.OpeningPrice * EachBuyVolumn; // add call earning
                    //OptionEarned += OptionPrice * record.OpeningPrice * EachBuyVolumn;
                }
#if false
                else if (diff > OptionPrice * record.OpeningPrice)
                {
                    // Sell
                    CurrentHoldVolumn = -EachBuyVolumn;
                    double price = record.OpeningPrice * (1.0 + OptionPrice);
                    LastPrice = price;
                    CurrentBalance += EachBuyVolumn * price;
                    Console.WriteLine($"Sell {EachBuyVolumn} at {price}");

                    CurrentBalance += OptionPrice * record.OpeningPrice * EachBuyVolumn; // add put earning
                    OptionEarned += OptionPrice * record.OpeningPrice * EachBuyVolumn;
                }
#endif
                else
                {
                    CurrentBalance += 1 * OptionPrice * record.OpeningPrice * EachBuyVolumn; // add call and put earning
                    OptionEarned += 1 * OptionPrice * record.OpeningPrice * EachBuyVolumn;
                }

                continue;
            }

            if (CurrentHoldVolumn > 0)
            {
                Debug.Assert(CurrentHoldVolumn == EachBuyVolumn);

                // sell call only
                var diff = record.ClosingPrice - record.OpeningPrice;

                // call selled
                if (diff > OptionPrice * record.OpeningPrice)
                {
                    // Sell
                    double price = record.OpeningPrice * (1.0 + OptionPrice);
                    double earned = (price - LastPrice) * EachBuyVolumn;

                    if (false && earned < 0)
                        continue; // if lose money, do nothing

                    CurrentHoldVolumn -= EachBuyVolumn;
                    CurrentBalance += EachBuyVolumn * price;
                    StockEarned += earned;
                    Console.WriteLine($"Sell {EachBuyVolumn} at {price} earned {earned}");
                }
                else
                {
                    // don't sell, earn call option
                    CurrentBalance += OptionPrice * record.OpeningPrice * EachBuyVolumn; // add call and put earning
                    OptionEarned += OptionPrice * record.OpeningPrice * EachBuyVolumn;
                }
                continue;
            }

            if (CurrentHoldVolumn < 0)
            {
                Debug.Assert(CurrentHoldVolumn == -EachBuyVolumn);

                // sell put only
                var diff = record.ClosingPrice - record.OpeningPrice;

                // put selled
                if (diff < -OptionPrice * record.OpeningPrice)
                {
                    // buy
                    CurrentHoldVolumn += EachBuyVolumn;
                    double price = record.OpeningPrice * (1.0 - OptionPrice);
                    CurrentBalance -= EachBuyVolumn * price;
                    double earned = (LastPrice - price) * EachBuyVolumn;
                    StockEarned += earned;
                    Console.WriteLine($"Buy {EachBuyVolumn} at {price} earned {earned}");
                }
                else
                {
                    // don't sell, earn put option
                    CurrentBalance += OptionPrice * record.OpeningPrice * EachBuyVolumn; // add call and put earning
                    OptionEarned += OptionPrice * record.OpeningPrice * EachBuyVolumn;
                }
                continue;
            }
        }
        Console.WriteLine($"Start Money: {StartMoney}");
        Console.WriteLine($"Balance: {CurrentBalance}");
        Console.WriteLine($"Option Earned: {OptionEarned}");
        Console.WriteLine($"Stock Earned: {StockEarned}");
        Console.WriteLine($"Current Hold Volumn: {CurrentHoldVolumn}");
    }

    static void WeeklyTwoDirection(List<Record> records)
    {
        CurrentBalance = 0;
        OptionEarned = 0;
        StockEarned = 0;
        CurrentHoldVolumn = 0;
        OptionPrice *= 4;

        Console.WriteLine("====WeeklyTwoDirection====");
        for (int i = 0; i < records.Count - 5; i += 5)
        {
            var rec1 = records[i];
            var rec2 = records[i + 5];
            if (CurrentHoldVolumn == 0)
            {
                var diff = rec2.ClosingPrice - rec1.OpeningPrice;
                if (diff < -OptionPrice * rec1.OpeningPrice)
                {
                    // Buy
                    CurrentHoldVolumn = EachBuyVolumn;
                    double price = rec1.OpeningPrice * (1.0 - OptionPrice);
                    LastPrice = price;
                    CurrentBalance -= EachBuyVolumn * price;
                    Console.WriteLine($"Buy {EachBuyVolumn} at {price}");

                    CurrentBalance += OptionPrice * rec1.OpeningPrice * EachBuyVolumn; // add call earning
                    OptionEarned += OptionPrice * rec1.OpeningPrice * EachBuyVolumn;
                }
#if true
                else if (diff > OptionPrice * rec1.OpeningPrice)
                {
                    // Sell
                    CurrentHoldVolumn = -EachBuyVolumn;
                    double price = rec1.OpeningPrice * (1.0 + OptionPrice);
                    LastPrice = price;
                    CurrentBalance += EachBuyVolumn * price;
                    Console.WriteLine($"Sell {EachBuyVolumn} at {price}");

                    CurrentBalance += OptionPrice * rec1.OpeningPrice * EachBuyVolumn; // add put earning
                    OptionEarned += OptionPrice * rec1.OpeningPrice * EachBuyVolumn;
                }
#endif
                else
                {
                    CurrentBalance += 1 * OptionPrice * rec1.OpeningPrice * EachBuyVolumn; // add call and put earning
                    OptionEarned += 1 * OptionPrice * rec1.OpeningPrice * EachBuyVolumn;
                }

                continue;
            }

            if (CurrentHoldVolumn > 0)
            {
                Debug.Assert(CurrentHoldVolumn == EachBuyVolumn);

                // sell call only
                var diff = rec2.ClosingPrice - rec1.OpeningPrice;

                // call selled
                if (diff > OptionPrice * rec1.OpeningPrice)
                {
                    // Sell
                    double price = rec1.OpeningPrice * (1.0 + OptionPrice);
                    double earned = (price - LastPrice) * EachBuyVolumn;

                    if (false && earned < 0)
                        continue; // if lose money, do nothing

                    CurrentHoldVolumn -= EachBuyVolumn;
                    CurrentBalance += EachBuyVolumn * price;
                    StockEarned += earned;
                    Console.WriteLine($"Sell {EachBuyVolumn} at {price} earned {earned}");
                }
                else
                {
                    // don't sell, earn call option
                    CurrentBalance += OptionPrice * rec1.OpeningPrice * EachBuyVolumn; // add call and put earning
                    OptionEarned += OptionPrice * rec1.OpeningPrice * EachBuyVolumn;
                }
                continue;
            }

            if (CurrentHoldVolumn < 0)
            {
                Debug.Assert(CurrentHoldVolumn == -EachBuyVolumn);

                // sell put only
                var diff = rec2.ClosingPrice - rec1.OpeningPrice;

                // put selled
                if (diff < -OptionPrice * rec1.OpeningPrice)
                {
                    // buy
                    CurrentHoldVolumn += EachBuyVolumn;
                    double price = rec1.OpeningPrice * (1.0 - OptionPrice);
                    CurrentBalance -= EachBuyVolumn * price;
                    double earned = (LastPrice - price) * EachBuyVolumn;
                    StockEarned += earned;
                    Console.WriteLine($"Buy {EachBuyVolumn} at {price} earned {earned}");
                }
                else
                {
                    // don't sell, earn put option
                    CurrentBalance += OptionPrice * rec1.OpeningPrice * EachBuyVolumn; // add call and put earning
                    OptionEarned += OptionPrice * rec1.OpeningPrice * EachBuyVolumn;
                }
                continue;
            }
        }
        Console.WriteLine($"Start Money: {StartMoney}");
        Console.WriteLine($"Balance: {CurrentBalance}");
        Console.WriteLine($"Option Earned: {OptionEarned}");
        Console.WriteLine($"Stock Earned: {StockEarned}");
        Console.WriteLine($"Current Hold Volumn: {CurrentHoldVolumn}");

        OptionPrice /= 4; // reset option price
    }

    static void WeeklyLongOnly(List<Record> records)
    {
        CurrentBalance = 0;
        OptionEarned = 0;
        StockEarned = 0;
        CurrentHoldVolumn = 0;
        OptionPrice *= 4;

        Console.WriteLine("====WeeklyLongOnly====");
        for (int i = 0; i < records.Count - 5; i+=5)
        {
            var rec1 = records[i];
            var rec2 = records[i + 5];
            if (CurrentHoldVolumn == 0)
            {
                var diff = rec2.ClosingPrice - rec1.OpeningPrice;
                if (diff < -OptionPrice * rec1.OpeningPrice)
                {
                    // Buy
                    CurrentHoldVolumn = EachBuyVolumn;
                    double price = rec1.OpeningPrice * (1.0 - OptionPrice);
                    LastPrice = price;
                    CurrentBalance -= EachBuyVolumn * price;
                    Console.WriteLine($"Buy {EachBuyVolumn} at {price}");

                    //CurrentBalance += OptionPrice * rec1.OpeningPrice * EachBuyVolumn; // add call earning
                    //OptionEarned += OptionPrice * rec1.OpeningPrice * EachBuyVolumn;
                }
#if false
                else if (diff > OptionPrice * record.OpeningPrice)
                {
                    // Sell
                    CurrentHoldVolumn = -EachBuyVolumn;
                    double price = record.OpeningPrice * (1.0 + OptionPrice);
                    LastPrice = price;
                    CurrentBalance += EachBuyVolumn * price;
                    Console.WriteLine($"Sell {EachBuyVolumn} at {price}");

                    CurrentBalance += OptionPrice * record.OpeningPrice * EachBuyVolumn; // add put earning
                    OptionEarned += OptionPrice * record.OpeningPrice * EachBuyVolumn;
                }
#endif
                else
                {
                    CurrentBalance += 1 * OptionPrice * rec1.OpeningPrice * EachBuyVolumn; // add call and put earning
                    OptionEarned += 1 * OptionPrice * rec1.OpeningPrice * EachBuyVolumn;
                }

                continue;
            }

            if (CurrentHoldVolumn > 0)
            {
                Debug.Assert(CurrentHoldVolumn == EachBuyVolumn);

                // sell call only
                var diff = rec2.ClosingPrice - rec1.OpeningPrice;

                // call selled
                if (diff > OptionPrice * rec1.OpeningPrice)
                {
                    // Sell
                    double price = rec1.OpeningPrice * (1.0 + OptionPrice);
                    double earned = (price - LastPrice) * EachBuyVolumn;

                    if (false && earned < 0)
                        continue; // if lose money, do nothing

                    CurrentHoldVolumn -= EachBuyVolumn;
                    CurrentBalance += EachBuyVolumn * price;
                    StockEarned += earned;
                    Console.WriteLine($"Sell {EachBuyVolumn} at {price} earned {earned}");
                }
                else
                {
                    // don't sell, earn call option
                    CurrentBalance += OptionPrice * rec1.OpeningPrice * EachBuyVolumn; // add call and put earning
                    OptionEarned += OptionPrice * rec1.OpeningPrice * EachBuyVolumn;
                }
                continue;
            }

            if (CurrentHoldVolumn < 0)
            {
                Debug.Assert(CurrentHoldVolumn == -EachBuyVolumn);

                // sell put only
                var diff = rec2.ClosingPrice - rec1.OpeningPrice;

                // put selled
                if (diff < -OptionPrice * rec1.OpeningPrice)
                {
                    // buy
                    CurrentHoldVolumn += EachBuyVolumn;
                    double price = rec1.OpeningPrice * (1.0 - OptionPrice);
                    CurrentBalance -= EachBuyVolumn * price;
                    double earned = (LastPrice - price) * EachBuyVolumn;
                    StockEarned += earned;
                    Console.WriteLine($"Buy {EachBuyVolumn} at {price} earned {earned}");
                }
                else
                {
                    // don't sell, earn put option
                    CurrentBalance += OptionPrice * rec1.OpeningPrice * EachBuyVolumn; // add call and put earning
                    OptionEarned += OptionPrice * rec1.OpeningPrice * EachBuyVolumn;
                }
                continue;
            }
        }
        Console.WriteLine($"Start Money: {StartMoney}");
        Console.WriteLine($"Balance: {CurrentBalance}");
        Console.WriteLine($"Option Earned: {OptionEarned}");
        Console.WriteLine($"Stock Earned: {StockEarned}");
        Console.WriteLine($"Current Hold Volumn: {CurrentHoldVolumn}");

        OptionPrice /= 4; // reset option price
    }

    static void MonthlyLongOnly(List<Record> records)
    {
        CurrentBalance = 0;
        OptionEarned = 0;
        StockEarned = 0;
        CurrentHoldVolumn = 0;
        OptionPrice *= 8;

        Console.WriteLine("====MonthlyLongOnly====");
        for (int i = 0; i < records.Count - 20; i += 20)
        {
            var rec1 = records[i];
            var rec2 = records[i + 20];
            if (CurrentHoldVolumn == 0)
            {
                var diff = rec2.ClosingPrice - rec1.OpeningPrice;
                if (diff < -OptionPrice * rec1.OpeningPrice)
                {
                    // Buy
                    CurrentHoldVolumn = EachBuyVolumn;
                    double price = rec1.OpeningPrice * (1.0 - OptionPrice);
                    LastPrice = price;
                    CurrentBalance -= EachBuyVolumn * price;
                    Console.WriteLine($"Buy {EachBuyVolumn} at {price}");

                    //CurrentBalance += OptionPrice * rec1.OpeningPrice * EachBuyVolumn; // add call earning
                    //OptionEarned += OptionPrice * rec1.OpeningPrice * EachBuyVolumn;
                }
#if false
                else if (diff > OptionPrice * record.OpeningPrice)
                {
                    // Sell
                    CurrentHoldVolumn = -EachBuyVolumn;
                    double price = record.OpeningPrice * (1.0 + OptionPrice);
                    LastPrice = price;
                    CurrentBalance += EachBuyVolumn * price;
                    Console.WriteLine($"Sell {EachBuyVolumn} at {price}");

                    CurrentBalance += OptionPrice * record.OpeningPrice * EachBuyVolumn; // add put earning
                    OptionEarned += OptionPrice * record.OpeningPrice * EachBuyVolumn;
                }
#endif
                else
                {
                    CurrentBalance += 1 * OptionPrice * rec1.OpeningPrice * EachBuyVolumn; // add call and put earning
                    OptionEarned += 1 * OptionPrice * rec1.OpeningPrice * EachBuyVolumn;
                }

                continue;
            }

            if (CurrentHoldVolumn > 0)
            {
                Debug.Assert(CurrentHoldVolumn == EachBuyVolumn);

                // sell call only
                var diff = rec2.ClosingPrice - rec1.OpeningPrice;

                // call selled
                if (diff > OptionPrice * rec1.OpeningPrice)
                {
                    // Sell
                    double price = rec1.OpeningPrice * (1.0 + OptionPrice);
                    double earned = (price - LastPrice) * EachBuyVolumn;

                    if (false && earned < 0)
                        continue; // if lose money, do nothing

                    CurrentHoldVolumn -= EachBuyVolumn;
                    CurrentBalance += EachBuyVolumn * price;
                    StockEarned += earned;
                    Console.WriteLine($"Sell {EachBuyVolumn} at {price} earned {earned}");
                }
                else
                {
                    // don't sell, earn call option
                    CurrentBalance += OptionPrice * rec1.OpeningPrice * EachBuyVolumn; // add call and put earning
                    OptionEarned += OptionPrice * rec1.OpeningPrice * EachBuyVolumn;
                }
                continue;
            }

            if (CurrentHoldVolumn < 0)
            {
                Debug.Assert(CurrentHoldVolumn == -EachBuyVolumn);

                // sell put only
                var diff = rec2.ClosingPrice - rec1.OpeningPrice;

                // put selled
                if (diff < -OptionPrice * rec1.OpeningPrice)
                {
                    // buy
                    CurrentHoldVolumn += EachBuyVolumn;
                    double price = rec1.OpeningPrice * (1.0 - OptionPrice);
                    CurrentBalance -= EachBuyVolumn * price;
                    double earned = (LastPrice - price) * EachBuyVolumn;
                    StockEarned += earned;
                    Console.WriteLine($"Buy {EachBuyVolumn} at {price} earned {earned}");
                }
                else
                {
                    // don't sell, earn put option
                    CurrentBalance += OptionPrice * rec1.OpeningPrice * EachBuyVolumn; // add call and put earning
                    OptionEarned += OptionPrice * rec1.OpeningPrice * EachBuyVolumn;
                }
                continue;
            }
        }
        Console.WriteLine($"Start Money: {StartMoney}");
        Console.WriteLine($"Balance: {CurrentBalance}");
        Console.WriteLine($"Option Earned: {OptionEarned}");
        Console.WriteLine($"Stock Earned: {StockEarned}");
        Console.WriteLine($"Current Hold Volumn: {CurrentHoldVolumn}");

        OptionPrice /= 8; // reset option price
    }
}
