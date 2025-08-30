using System.Windows.Forms;

namespace HaynesProParser.DataBase.Elements
{
    public static class dbFuelTypes
    {
        public static int GetFuelTypeId(string fuelType)
        {
            return fuelType.ToLower() switch
            {
                "petrol" => 0,
                "diesel" => 1,
                "cng" => 2,
                "hydrogen" => 3,
                "electric" => 4,
                "hybrid" => 5,
                _ => ErrorType(fuelType)
            };
        }

        private static int ErrorType(string fuelType)
        {
            MessageBox.Show(fuelType, "Error");
            return 6;
        }
    }
}
