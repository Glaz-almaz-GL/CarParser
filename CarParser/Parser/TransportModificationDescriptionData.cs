using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Office2010.PowerPoint;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;

namespace CarParser
{
    public class TransportModificationDescriptionData
    {
        string ModificationId { get; }
        string Group { get; }
        string Type { get; }
        string MakeName { get; }
        string EngineCode { get; }
        string Code { get; }
        string FuelType { get; }
        string Volume { get; }
        string Bridge { get; }
        string Power { get; }
        string ModelYear { get; }
        string ManufactureType { get; }
        string Description { get; }
        string Weight { get; }

        public TransportModificationDescriptionData(string modificationId, string group, string type, string makeName, string engineCode, string code, string fuelType, string volume, string bridge, string power, string modelYear, string manufactureType, string description, string weight)
        {
            ModificationId = modificationId;
            Group = group;
            Type = type;
            MakeName = makeName;
            EngineCode = engineCode;
            Code = code;
            FuelType = fuelType;
            Volume = volume;
            Bridge = bridge;
            Power = power;
            ModelYear = modelYear;
            ManufactureType = manufactureType;
            Description = description;
            Weight = weight;
        }

        public void GetModificationData(ref string modificationId, ref string modificationGroup, ref string modificationType, ref string modificationMakeName, ref string modificationEngineCode, ref string modificationCode, ref string modificationFuelType, ref string modificationVolume, ref string modificationBridge, ref string modificationPower, ref string modificationModelYear, ref string modificationManufactureType, ref string modificationDescription, ref string modificationWeight)
        {
            modificationId = ModificationId;
            modificationGroup = Group;
            modificationType = Type;
            modificationMakeName = MakeName;
            modificationEngineCode = EngineCode;
            modificationCode = Code;
            modificationFuelType = FuelType;
            modificationVolume = Volume;
            modificationBridge = Bridge;
            modificationPower = Power;
            modificationModelYear = ModelYear;
            modificationManufactureType = ManufactureType;
            modificationDescription = Description;
            modificationWeight = Weight;

            Console.WriteLine($"Modification Id: {ModificationId}, Modification Type: {Type}, Modification Engine Code: {EngineCode}, Modification Volume: {Volume}, Modification Fuel Type: {FuelType}, Modification Power: {Power}, Modification Model Year: {ModelYear}");
        }
    }
}