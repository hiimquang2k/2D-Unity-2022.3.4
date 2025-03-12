// See https://aka.ms/new-console-template for more information
using Microsoft.VisualBasic.FileIO;

internal class Date{
    public int day, month, year, oriDay;
    public int Jan = 31, Feb = 28, Mar = 31, Apr = 30, May = 31, Jun = 30, Jul =31, Aug = 31, Sep = 30, Oct = 31, Nov = 30, Dec = 31 ;
    static void Main()
    {
        Date date = new Date();
        date.Input();
        Console.WriteLine(date.IsLeapYear());
        if (date.IsLeapYear())
        {
            date.Feb = 29;
        }
        Console.WriteLine(date.ExceedDayInMonth());
        date.Substract();
        date.Reconstruct();
        date.Output();
    }
    public void Input()
    {
        Console.WriteLine("Enter day: ");
        this.day = int.Parse(Console.ReadLine());
        Console.WriteLine("Enter month: ");
        this.month = int.Parse(Console.ReadLine());
        Console.WriteLine("Enter year: ");
        this.year = int.Parse(Console.ReadLine());
    }
    public void Output()
    {
        Console.WriteLine($"{this.day}/{this.month}/{this.year}");
    }
    public bool IsLeapYear()
    {
        return ((this.year % 4 == 0 || this.year % 400 == 0) && this.year % 100 != 0);
    }
    public bool ExceedDayInMonth()
    {
        return this.day > CurrentMonth();
    }
    public bool TooFarBack()
    {
        return this.day <= 0;
    }
    public int CurrentMonth()
    {
        switch (this.month)
        {
        case 01:
            return  Jan;
        case 02:         
            return  Feb;
        case 03:
            return  Mar;
        case 04:
            return  Apr;
        case 05:
            return  Mar;
        case 06:
            return  Jun;
        case 07:
            return  Jul;
        case 08:
            return  Aug;
        case 09:
            return  Sep;
        case 10:
            return  Oct;
        case 11:
            return  Nov;
        case 12:
            return  Dec; 
        }
        return 0;
    }
    public void Add()
    {
        oriDay = this.day;
        this.day += 35;  
    }
    public void Substract()
    {
        this.day -= 5;
    }
    public void Reconstruct()
    {
        while (ExceedDayInMonth())
        {
            this.day -= oriDay;
            this.month += 1;
            if (this.month > 12)
            {
                this.year += 1;
                this.month = 1;
            } 
        }
        while (TooFarBack())
        {
        this.month -=1;
        if (this.day == 0)
        this.day = CurrentMonth() - Math.Abs(this.day);
        else if (this.day < 0)
        this.day = CurrentMonth() - Math.Abs(this.day+1);
        if (this.month < 1)
        {
            this.year -=1;
            this.month = 12;
        }
        }
    }
}
