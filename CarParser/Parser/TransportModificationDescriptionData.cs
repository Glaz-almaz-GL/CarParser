using OpenQA.Selenium;
using System;
using System.Collections.Generic;

namespace CarParser
{
    public class TransportModificationDescriptionData
    {
        string ModificationId { get; }
        string Type { get; }
        string EngineCode { get; }
        string FuelType { get; }
        string Volume { get; }
        string Power { get; }
        string ModelYear { get; }

        public TransportModificationDescriptionData(string modificationId, string type, string engineCode, string fuelType, string volume, string power, string modelYear)
        {
            ModificationId = modificationId;
            Type = type;
            EngineCode = engineCode;
            FuelType = fuelType;
            Volume = volume;
            Power = power;
            ModelYear = modelYear;
        }

        public void GetModificationData(ref string modificationId, ref string modificationType, ref string modificationEngineCode, ref string modificationFuelType, ref string modificationVolume, ref string modificationPower, ref string modificationModelYear)
        {
            modificationId = ModificationId;
            modificationType = Type;
            modificationEngineCode = EngineCode;
            modificationFuelType = FuelType;
            modificationVolume = Volume;
            modificationPower = Power;
            modificationModelYear = ModelYear;

            Console.WriteLine($"Modification Id: {ModificationId}, Modification Type: {Type}, Modification Engine Code: {EngineCode}, Modification Volume: {Volume}, Modification Fuel Type: {FuelType}, Modification Power: {Power}, Modification Model Year: {ModelYear}");
        }
    }
}