using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WinformDevFramework.IServices.PLCBasic;
using WinformDevFramework.Models.PLCBasic;
using WinformDevFramework.IRepository.PLCBasic;
using PLCBasic.IRepository;

namespace WinformDevFramework.Services.PLCBasic.Handlers
{
    public static class PlcHandlerHelper
    {
        public static bool GetEventDataPoints(ILogger logger, IPLC_Event_Data_DetailRepository eventDetailRepository, int eventId, out List<PLC_Event_Data_Detail> readDataPoints, out List<PLC_Event_Data_Detail> writeDataPoints)
        {
            readDataPoints = new List<PLC_Event_Data_Detail>();
            writeDataPoints = new List<PLC_Event_Data_Detail>();

            try
            {
                var eventDetails = eventDetailRepository.QueryListByClauseAsync(p => p.EventId == eventId).Result;
                if (eventDetails == null || !eventDetails.Any())
                {
                    logger.LogError($"未找到事件数据点配置: EventId={eventId}");
                    return false;
                }

                readDataPoints = eventDetails.Where(d => d.IOOperation?.ToLower() == "read").ToList();
                writeDataPoints = eventDetails.Where(d => d.IOOperation?.ToLower() == "write").ToList();

                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"获取事件数据点配置异常: EventId={eventId}");
                return false;
            }
        }

        public static async Task<Dictionary<string, string>> ReadEventDataPointsAsync(IPlcCommunicationService plcCommunicationService, ILogger logger, string plcCode, List<PLC_Event_Data_Detail> readDataPoints)
        {
            var readDataPointsDict = new Dictionary<string, string>();

            foreach (var dataPoint in readDataPoints)
            {
                string value = await ReadPlcParamAsync(plcCommunicationService, logger, plcCode, dataPoint.DataAddress, dataPoint.DataType);
                readDataPointsDict[dataPoint.ParamName] = value;
            }

            return readDataPointsDict;
        }

        public static async Task<string> ReadPlcParamAsync(IPlcCommunicationService plcCommunicationService, ILogger logger, string plcCode, string address, string dataType)
        {
            return await Task.Run(() => ReadPlcData(plcCommunicationService, logger, plcCode, address, dataType));
        }

        public static string ReadPlcData(IPlcCommunicationService plcCommunicationService, ILogger logger, string plcCode, string address, string dataType)
        {
            try
            {
                if (string.IsNullOrEmpty(dataType))
                    return string.Empty;

                switch (dataType.ToLower())
                {
                    case "bool":
                        var boolResult = plcCommunicationService.ReadBool(plcCode, address);
                        return boolResult.IsSuccess ? boolResult.Content.ToString() : string.Empty;
                    case "int32":
                    case "int":
                        var intResult = plcCommunicationService.ReadInt32(plcCode, address);
                        return intResult.IsSuccess ? intResult.Content.ToString() : string.Empty;
                    case "float":
                    case "real":
                        var floatResult = plcCommunicationService.ReadFloat(plcCode, address);
                        return floatResult.IsSuccess ? floatResult.Content.ToString() : string.Empty;
                    case "string":
                        var stringResult = plcCommunicationService.ReadString(plcCode, address, 256);
                        return stringResult.IsSuccess ? stringResult.Content : string.Empty;
                    case "byte":
                        var byteResult = plcCommunicationService.ReadByte(plcCode, address);
                        return byteResult.IsSuccess ? byteResult.Content.ToString() : string.Empty;
                    case "double":
                        var doubleResult = plcCommunicationService.ReadDouble(plcCode, address);
                        return doubleResult.IsSuccess ? doubleResult.Content.ToString() : string.Empty;
                    default:
                        var defaultResult = plcCommunicationService.ReadInt32(plcCode, address);
                        return defaultResult.IsSuccess ? defaultResult.Content.ToString() : string.Empty;
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, $"读取PLC数据失败: PlcCode={plcCode}, Address={address}, DataType={dataType}");
                return string.Empty;
            }
        }

        public static async Task<bool> CollectAndWriteDataPointsAsync(IPlcCommunicationService plcCommunicationService, ILogger logger, string plcCode, List<PLC_Event_Data_Detail> writeDataPoints, Dictionary<string, object> dataToWrite)
        {
            bool allSuccess = true;

            foreach (var writeDataPoint in writeDataPoints)
            {
                if (dataToWrite.TryGetValue(writeDataPoint.ParamName, out var value))
                {
                    await WriteDataPointAsync(plcCommunicationService, logger, plcCode, writeDataPoint.DataAddress, writeDataPoint.DataType, value);
                }
            }

            return allSuccess;
        }

        public static async Task WriteDataPointAsync(IPlcCommunicationService plcCommunicationService, ILogger logger, string plcCode, string address, string dataType, object value)
        {
            await Task.Run(() => WriteDataPoint(plcCommunicationService, logger, plcCode, address, dataType, value));
        }

        public static void WriteDataPoint(IPlcCommunicationService plcCommunicationService, ILogger logger, string plcCode, string address, string dataType, object value)
        {
            try
            {
                if (string.IsNullOrEmpty(dataType))
                    return;

                switch (dataType.ToLower())
                {
                    case "bool":
                        bool boolValue = value is bool ? (bool)value : Convert.ToBoolean(value);
                        plcCommunicationService.Write(plcCode, address, boolValue);
                        break;
                    case "int32":
                    case "int":
                        int intValue = value is int ? (int)value : Convert.ToInt32(value);
                        plcCommunicationService.Write(plcCode, address, intValue);
                        break;
                    case "float":
                    case "real":
                        float floatValue = value is float ? (float)value : Convert.ToSingle(value);
                        plcCommunicationService.Write(plcCode, address, floatValue);
                        break;
                    case "string":
                        string stringValue = value?.ToString() ?? string.Empty;
                        plcCommunicationService.Write(plcCode, address, stringValue);
                        break;
                    case "byte":
                        byte byteValue = value is byte ? (byte)value : Convert.ToByte(value);
                        plcCommunicationService.Write(plcCode, address, byteValue);
                        break;
                    case "double":
                        double doubleValue = value is double ? (double)value : Convert.ToDouble(value);
                        plcCommunicationService.Write(plcCode, address, doubleValue);
                        break;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"写入PLC数据失败: PlcCode={plcCode}, Address={address}, DataType={dataType}, Value={value}");
            }
        }

        public static string CleanPlcString(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            return input.Trim().Replace("\0", "");
        }

        public static async Task ErrorMsg(IPlcCommunicationService plcCommunicationService, ILogger logger, IPLC_Event_Data_DetailRepository eventDetailRepository, string plcCode, string stationCode, string barcode, string msg, int errorCode, List<PLC_Event_Data_Detail> writeDataPoints, Dictionary<string, object> dataToWrite)
        {
            logger.LogError($"PLC事件处理错误: PlcCode={plcCode}, StationCode={stationCode}, Barcode={barcode}, ErrorCode={errorCode}, Message={msg}");

            dataToWrite["ErrorCode"] = errorCode;
            dataToWrite["ErrorMsg"] = msg;

            await CollectAndWriteDataPointsAsync(plcCommunicationService, logger, plcCode, writeDataPoints, dataToWrite);
        }

        public static async Task<int> CheckDeviceStatusAsync(IPlcCommunicationService plcCommunicationService, ILogger logger, IPLC_AddressRepository plcAddressRepository, string plcCode, string stationCode)
        {
            try
            {
                var deviceStatusAddress = await plcAddressRepository.QueryByClauseAsync(p => p.PlcCode == plcCode && p.StationCode == stationCode && p.Category == "DeviceStatus");
                if (deviceStatusAddress == null)
                {
                    logger.LogError($"未找到设备状态配置: PlcCode={plcCode}, StationCode={stationCode}, Category=DeviceStatus");
                    return -1;
                }

                string statusValue = ReadPlcData(plcCommunicationService, logger, plcCode, deviceStatusAddress.AddressCode, deviceStatusAddress.DataType);
                if (!int.TryParse(statusValue, out int deviceStatus))
                {
                    logger.LogError($"读取设备状态失败，无法解析为整数: PlcCode={plcCode}, StationCode={stationCode}, Value={statusValue}");
                    return -1;
                }

                logger.LogInformation($"设备状态检查: PlcCode={plcCode}, StationCode={stationCode}, Status={deviceStatus}");

                if (deviceStatus != 90 && deviceStatus != 83)
                {
                    logger.LogWarning($"设备状态不满足条件，跳过后续操作: PlcCode={plcCode}, StationCode={stationCode}, Status={deviceStatus}");
                    return -1;
                }

                return deviceStatus;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"检查设备状态异常: PlcCode={plcCode}, StationCode={stationCode}");
                return -1;
            }
        }

        public static async Task SendDeviceStatusErrorAsync(IPlcCommunicationService plcCommunicationService, ILogger logger, string plcCode, string stationCode, List<PLC_Event_Data_Detail> writeDataPoints, Dictionary<string, object> dataToWrite)
        {
            string msg = "当前设备状态不为83，90，不满足生产条件";
            logger.LogError($"设备状态错误: PlcCode={plcCode}, StationCode={stationCode}, Message={msg}");

            dataToWrite["PartNG"] = true;
            dataToWrite["PartIDReqDone"] = false;
            dataToWrite["MainCheckInErrorCode"] = 103;

            await CollectAndWriteDataPointsAsync(plcCommunicationService, logger, plcCode, writeDataPoints, dataToWrite);
        }
    }
}