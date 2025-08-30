using System.Diagnostics;

namespace HaynesProParser.DataBase.Elements
{
    public static class dbTransportTypes
    {
        public static int GetTransportTypeId(string transportType)
        {
            return transportType.ToLower().Trim() switch
            {
                "car" => 1,
                "motorcycle" => 2,
                "truck" => 3,
                "axle" => 4,
                "trailer" => 5,
                _ => ErrorType(transportType)
            };
        }

        public static string GetTransportTypeName(int transportId)
        {
            return transportId switch
            {
                0 => "car",
                1 => "motorcycle",
                2 => "truck",
                3 => "axle",
                4 => "trailer",
                _ => ErrorType(transportId)
            };
        }

        private static string ErrorType(int transportId)
        {
            Debug.WriteLine($"Error {transportId}");
            return "ErrorType";
        }

        private static int ErrorType(string transportName)
        {
            Debug.WriteLine($"Error {transportName}");
            return 5;
        }
    }
}
