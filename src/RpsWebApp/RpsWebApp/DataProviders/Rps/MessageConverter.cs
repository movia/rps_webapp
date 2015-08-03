using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using RpsPositionWebApp.Data;
using RpsPositionWebApp.ViewModels;

namespace RpsPositionWebApp.DataProviders.Rps
{
    public class MessageConverter
    {
        internal static string ByteArrayToHexViaLookup(byte[] bytes)
        {
            /* Best performance convertion by http://stackoverflow.com/questions/311165/how-do-you-convert-byte-array-to-hexadecimal-string-and-vice-versa */
            string[] hexStringTable = new string[] {
                "00", "01", "02", "03", "04", "05", "06", "07", "08", "09", "0A", "0B", "0C", "0D", "0E", "0F",
                "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "1A", "1B", "1C", "1D", "1E", "1F",
                "20", "21", "22", "23", "24", "25", "26", "27", "28", "29", "2A", "2B", "2C", "2D", "2E", "2F",
                "30", "31", "32", "33", "34", "35", "36", "37", "38", "39", "3A", "3B", "3C", "3D", "3E", "3F",
                "40", "41", "42", "43", "44", "45", "46", "47", "48", "49", "4A", "4B", "4C", "4D", "4E", "4F",
                "50", "51", "52", "53", "54", "55", "56", "57", "58", "59", "5A", "5B", "5C", "5D", "5E", "5F",
                "60", "61", "62", "63", "64", "65", "66", "67", "68", "69", "6A", "6B", "6C", "6D", "6E", "6F",
                "70", "71", "72", "73", "74", "75", "76", "77", "78", "79", "7A", "7B", "7C", "7D", "7E", "7F",
                "80", "81", "82", "83", "84", "85", "86", "87", "88", "89", "8A", "8B", "8C", "8D", "8E", "8F",
                "90", "91", "92", "93", "94", "95", "96", "97", "98", "99", "9A", "9B", "9C", "9D", "9E", "9F",
                "A0", "A1", "A2", "A3", "A4", "A5", "A6", "A7", "A8", "A9", "AA", "AB", "AC", "AD", "AE", "AF",
                "B0", "B1", "B2", "B3", "B4", "B5", "B6", "B7", "B8", "B9", "BA", "BB", "BC", "BD", "BE", "BF",
                "C0", "C1", "C2", "C3", "C4", "C5", "C6", "C7", "C8", "C9", "CA", "CB", "CC", "CD", "CE", "CF",
                "D0", "D1", "D2", "D3", "D4", "D5", "D6", "D7", "D8", "D9", "DA", "DB", "DC", "DD", "DE", "DF",
                "E0", "E1", "E2", "E3", "E4", "E5", "E6", "E7", "E8", "E9", "EA", "EB", "EC", "ED", "EE", "EF",
                "F0", "F1", "F2", "F3", "F4", "F5", "F6", "F7", "F8", "F9", "FA", "FB", "FC", "FD", "FE", "FF",
            };
            StringBuilder result = new StringBuilder(bytes.Length * 2);
            foreach (byte b in bytes)
            {
                result.Append(hexStringTable[b]);
            }
            return result.ToString();
        }

        private ConfigurationManager config;
        internal ushort sequenceNumber = 0;

        public MessageConverter()
        {
            config = ConfigurationManager.Instance;
        }

        public byte[] VehiclePositionReport(VehiclePositionEvent vehiclePositionEvent)
        {
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write((byte)2);
                writer.Write((byte)127);

                var deviceId = config.Configuration.Get("vehicles:" + vehiclePositionEvent.VehicleId + ":deviceId");
                var deviceIdParts = deviceId.Split('-');

                if (deviceIdParts.Length != 8)
                    throw new FormatException($"Invalid Device Id '{deviceId}'.");
                
                // 3	2	Yes	Unit identity	Byte(8)	Fixed identity of sending unit	E.g. MAC-address or similar.
                for (var i = 0; i < 8; i++)
                {
                    writer.Write(Convert.ToByte(deviceIdParts[i], 16));
                }               

                // 4	10	Yes	Sequence number	UInt16	0-65535	See 4.2.1.
                if (sequenceNumber == ushort.MaxValue)
                    sequenceNumber = 1;

                writer.Write(sequenceNumber++);

                // 5	12	Yes	Timestamp	UInt32	Time of fix.	Milliseconds since UTC midnight.
                writer.Write((UInt32)DateTime.UtcNow.TimeOfDay.TotalMilliseconds);

                // 6	16	Yes	Latitude	Single	-90,00000° - +90,00000°	IEEE 32-bits floating point number with single precision.
                writer.Write((Single)vehiclePositionEvent.Latitude);

                // 7	20	Yes	Longitude	Single	-180,00000° - +180,00000°	
                writer.Write((Single)vehiclePositionEvent.Longitude);

                // 8	24	Yes	Speed	UInt16	0,00 – 655.36 m/s	Speed in steps of 0,01 m/s
                writer.Write((UInt16)(vehiclePositionEvent.Speed * 100f));

                // 9	26	Yes	Direction	UInt16	0,00 – 359.99° 	Direction in steps of 0.01°
                writer.Write((UInt16)(vehiclePositionEvent.Direction * 100f));

                // 10	28	Yes	GPS-quality	Byte	Quality for GPS.	See 4.2.6.
                writer.Write((Byte)(0x01 | (0x02 << 4)));

                // 11	29	Some	Signals	Byte	4 signals of 2 bits each	See 4.2.7.
                writer.Write((Byte)((0x03 << 6) | (0x00 << 4) | (0x00 << 2) | (0x03 << 0)));

                // 12	30	No	Distance	UInt32	Vehicle distance in steps of 5 meters	See 4.2.8
                writer.Write((UInt32)0);

                var vehicleId = Encoding.ASCII.GetBytes(vehiclePositionEvent.VehicleId);
                // L13	34	Yes	Length of field 13	Byte	0-255	Number of bytes.
                writer.Write((Byte)vehicleId.Length);

                // 13		Yes	Vehicle id	String	Identity of vehicle.	Must be a vehicle fixed unique value, e.g. BUS FMS VI field.
                writer.Write(vehicleId);

                // L14		No	Length of field 14	Byte	0-255	Number of bytes.
                writer.Write((Byte)0);

                // 14		No	Driver id	String	Identity of current driver.	Optional. Could be BUS FMS DI, or unique id from other available source in vehicle.
                //writer.Write();

                var serviceJourneyId = Encoding.ASCII.GetBytes(VehicleAssignmentManager.Instance.GetJourneyId(vehiclePositionEvent.VehicleId) ?? string.Empty);
                // L15		Yes	Length of field 15	Byte	0-255	Number of bytes.
                writer.Write((Byte)serviceJourneyId.Length);

                // 15		Yes	Service Journey Id	String	See 4.2.11.
                writer.Write(serviceJourneyId);

                // L16		No	Length of field 16	Byte	0-255	Number of bytes.
                writer.Write((Byte)0);

                // 16		No	Account Id	String	Identity of operator.	Optional.

                return stream.ToArray();
            }
        }
    }
}