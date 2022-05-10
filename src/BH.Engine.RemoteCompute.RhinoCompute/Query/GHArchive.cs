/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2022, the respective contributors. All rights reserved.
 *
 * Each contributor holds copyright over their respective contributions.
 * The project versioning (Git) records all such contribution source information.
 *                                           
 *                                                                              
 * The BHoM is free software: you can redistribute it and/or modify         
 * it under the terms of the GNU Lesser General Public License as published by  
 * the Free Software Foundation, either version 3.0 of the License, or          
 * (at your option) any later version.                                          
 *                                                                              
 * The BHoM is distributed in the hope that it will be useful,              
 * but WITHOUT ANY WARRANTY; without even the implied warranty of               
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the                 
 * GNU Lesser General Public License for more details.                          
 *                                                                            
 * You should have received a copy of the GNU Lesser General Public License     
 * along with this code. If not, see <https://www.gnu.org/licenses/lgpl-3.0.html>.      
 */

using System;
using System.IO;
using GH_IO.Serialization;

namespace BH.Engine.RemoteCompute.RhinoCompute
{
    public static partial class Query
    {
        public static GH_Archive GHArchive(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new Exception($"Missing {nameof(filePath)} input.");
            }

            if (!File.Exists(filePath))
            {
                throw new Exception($"File at input filepath does not exist: {filePath}");
            }

            byte[] byteArray = File.ReadAllBytes(filePath);
            return byteArray.GHArchive();
        }

        public static GH_Archive GHArchive(this byte[] byteArray)
        {
            try
            {
                var byteArchive = new GH_Archive();
                if (byteArchive.Deserialize_Binary(byteArray))
                    return byteArchive;
            }
            catch (Exception) { }

            string Xml = System.Text.Encoding.UTF8.GetString(byteArray).StripBom();

            var xmlArchive = new GH_Archive();
            if (!xmlArchive.Deserialize_Xml(Xml))
                throw new Exception($"Could not deserialize GH_Archive from input ByteArray.");

            return xmlArchive;
        }
    }
}

