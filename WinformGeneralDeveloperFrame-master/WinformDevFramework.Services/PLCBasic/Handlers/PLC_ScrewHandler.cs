using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinformDevFramework.IServices.PLCBasic;
using WinformDevFramework.Models.PLCBasic;
using WinformDevFramework.IRepository.PLCBasic;
using PLCBasic.IRepository;
using PLCBasic;

namespace WinformDevFramework.Services.PLCBasic.Handlers
{
    public class PLC_ScrewHandler : IPLC_ScrewHandler
    {
        private readonly ILogger<PLC_ScrewHandler> _logger;
        private readonly IPlcCommunicationService _plcCommunicationService;
        private readonly IPLC_Event_Data_DetailRepository _eventDetailRepository;
        private readonly IWipProcessDataRepository _wipProcessDataRepository;
        private readonly ISocketCommunicationService _socketCommunicationService;
        private readonly IPLC_AddressRepository _plcAddressRepository;

        public PLC_ScrewHandler(ILogger<PLC_ScrewHandler> logger,
                                  IPlcCommunicationService plcCommunicationService,
                                  IPLC_Event_Data_DetailRepository eventDetailRepository,
                                  IWipProcessDataRepository wipProcessDataRepository,
                                  ISocketCommunicationService socketCommunicationService,
                                  IPLC_AddressRepository plcAddressRepository)
        {
            _logger = logger;
            _plcCommunicationService = plcCommunicationService;
            _eventDetailRepository = eventDetailRepository;
            _wipProcessDataRepository = wipProcessDataRepository;
            _socketCommunicationService = socketCommunicationService;
            _plcAddressRepository = plcAddressRepository;
        }

        public async Task HandleAsync(PlcEventTriggeredEventArgs e, bool isRisingEdge)
        {
            if (isRisingEdge)
            {
                await HandleScrewExchangeAsync(e);
            }
            else
            {
                await HandleScrewDownExchangeAsync(e);
            }
        }

        private async Task HandleScrewExchangeAsync(PlcEventTriggeredEventArgs e)
        {
            try
            {
                _logger.LogInformation($"开始处理拧紧数据相关操作: EventId={e.EventId}, PlcCode={e.PlcCode}, StationCode={e.StationCode}");

                if (!PlcHandlerHelper.GetEventDataPoints(_logger, _eventDetailRepository, e.EventId, out var readDataPoints, out var writeDataPoints))
                {
                    return;
                }

                Dictionary<string, object> dataToWrite = new Dictionary<string, object>();

                int deviceStatus = await PlcHandlerHelper.CheckDeviceStatusAsync(_plcCommunicationService, _logger, _plcAddressRepository, e.PlcCode, e.StationCode);
                if (deviceStatus == -1)
                {
                    await PlcHandlerHelper.SendDeviceStatusErrorAsync(_plcCommunicationService, _logger, e.PlcCode, e.StationCode, writeDataPoints, dataToWrite);
                    return;
                }

                Dictionary<string, string> readDataPointsDict = await PlcHandlerHelper.ReadEventDataPointsAsync(_plcCommunicationService, _logger, e.PlcCode, readDataPoints);

                await TriggerSocketRisingEdgeAsync(e);

                _logger.LogInformation($"拧紧数据事件处理完成: EventId={e.EventId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"处理拧紧数据事件时发生错误: EventId={e.EventId}");
            }
        }

        private async Task HandleScrewDownExchangeAsync(PlcEventTriggeredEventArgs e)
        {
            try
            {
                _logger.LogInformation($"开始处理拧紧下降沿数据相关操作: EventId={e.EventId}, PlcCode={e.PlcCode}, StationCode={e.StationCode}");

                if (!PlcHandlerHelper.GetEventDataPoints(_logger, _eventDetailRepository, e.EventId, out var readDataPoints, out var writeDataPoints))
                {
                    return;
                }

                Dictionary<string, string> readDataPointsDict = await PlcHandlerHelper.ReadEventDataPointsAsync(_plcCommunicationService, _logger, e.PlcCode, readDataPoints);

                List<double> screwDataList = await TriggerSocketFallingEdgeAsync(e);

                if (screwDataList != null && screwDataList.Count > 0)
                {
                    await SaveScrewDataAsync(e, screwDataList, readDataPointsDict);
                }

                _logger.LogInformation($"拧紧下降沿数据事件处理完成: EventId={e.EventId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"处理拧紧下降沿数据事件时发生错误: EventId={e.EventId}");
            }
        }

        private async Task TriggerSocketRisingEdgeAsync(PlcEventTriggeredEventArgs e)
        {
            try
            {
                if (_socketCommunicationService == null)
                {
                    _logger.LogWarning("Socket通讯服务未初始化");
                    return;
                }

                if (!await EnsureSocketConnectedAsync())
                {
                    return;
                }

                byte[] aqCommand = Encoding.UTF8.GetBytes("AQ\r");
                _logger.LogInformation($"拧紧上升沿-发送AQ指令");
                byte[] aqResponse = await _socketCommunicationService.SendRawAndReceiveAsync(aqCommand, 4);
                if (aqResponse != null)
                {
                    string aqHex = BitConverter.ToString(aqResponse).Replace("-", "").ToLower();
                    _logger.LogInformation($"拧紧上升沿-AQ指令响应: {aqHex}");
                }
                else
                {
                    _logger.LogWarning("拧紧上升沿-AQ指令未收到响应");
                }

                byte[] asCommand = Encoding.UTF8.GetBytes("AS\r");
                _logger.LogInformation($"拧紧上升沿-发送AS指令");
                byte[] asResponse = await _socketCommunicationService.SendRawAndReceiveAsync(asCommand, 4);
                if (asResponse != null)
                {
                    string asHex = BitConverter.ToString(asResponse).Replace("-", "").ToLower();
                    _logger.LogInformation($"拧紧上升沿-AS指令响应: {asHex}");
                }
                else
                {
                    _logger.LogWarning("拧紧上升沿-AS指令未收到响应");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "拧紧上升沿Socket通讯失败");
            }
        }

        private async Task<List<double>> TriggerSocketFallingEdgeAsync(PlcEventTriggeredEventArgs e)
        {
            var resultList = new List<double>();

            try
            {
                if (_socketCommunicationService == null)
                {
                    _logger.LogWarning("Socket通讯服务未初始化");
                    return resultList;
                }

                if (!await EnsureSocketConnectedAsync())
                {
                    return resultList;
                }

                byte[] apCommand = Encoding.UTF8.GetBytes("AP\r");
                _logger.LogInformation($"拧紧下降沿-发送AP指令");
                byte[] apResponse = await _socketCommunicationService.SendRawAndReceiveAsync(apCommand, 4);
                if (apResponse != null)
                {
                    string apHex = BitConverter.ToString(apResponse).Replace("-", "").ToLower();
                    _logger.LogInformation($"拧紧下降沿-AP指令响应: {apHex}");
                }
                else
                {
                    _logger.LogWarning("拧紧下降沿-AP指令未收到响应");
                }

                byte[] aoCommand = Encoding.UTF8.GetBytes("AO,1\r");
                _logger.LogInformation($"拧紧下降沿-发送AO,1指令");
                string aoResponse = await _socketCommunicationService.SendRawAndReceiveLineAsync(aoCommand);
                if (!string.IsNullOrEmpty(aoResponse))
                {
                    _logger.LogInformation($"拧紧下降沿-AO,1指令响应: {aoResponse}");

                    string[] parts = aoResponse.Split(',');
                    for (int i = 1; i < parts.Length; i++)
                    {
                        resultList.Add(Convert.ToDouble(parts[i].Trim()));
                    }

                    _logger.LogInformation($"拧紧下降沿-解析到 {resultList.Count} 条数据");
                }
                else
                {
                    _logger.LogWarning("拧紧下降沿-AO,1指令未收到响应");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "拧紧下降沿Socket通讯失败");
            }

            return resultList;
        }

        private async Task<bool> EnsureSocketConnectedAsync()
        {
            if (_socketCommunicationService.IsConnected)
            {
                return true;
            }

            _logger.LogInformation("拧紧Socket未连接，尝试连接...");
            string socketIp = "127.0.0.1";
            int socketPort = 6000;

            bool connected = await _socketCommunicationService.ConnectAsync(socketIp, socketPort);
            if (!connected)
            {
                _logger.LogWarning($"拧紧Socket连接失败: {socketIp}:{socketPort}");
                return false;
            }

            _logger.LogInformation($"拧紧Socket连接成功: {socketIp}:{socketPort}");

            _socketCommunicationService.ConnectionStatusChanged += async (sender, isConnected) =>
            {
                if (!isConnected)
                {
                    _logger.LogWarning("拧紧Socket连接断开，5秒后尝试自动重连...");
                    await Task.Delay(5000);
                    await EnsureSocketConnectedAsync();
                }
            };

            return true;
        }

        private async Task SaveScrewDataAsync(PlcEventTriggeredEventArgs e, List<double> dataList, Dictionary<string, string> readDataPointsDict)
        {
            try
            {
                string partType = PlcHandlerHelper.CleanPlcString(readDataPointsDict.ContainsKey("PartType") ? readDataPointsDict["PartType"] : string.Empty);
                string recipeVer = PlcHandlerHelper.CleanPlcString(readDataPointsDict.ContainsKey("RecipeVer") ? readDataPointsDict["RecipeVer"] : string.Empty);
                string rfidBarCode = PlcHandlerHelper.CleanPlcString(readDataPointsDict.ContainsKey("BarCode") ? readDataPointsDict["BarCode"] : string.Empty);

                string filePath = "D://数据信息//拧紧//";
                string fileName = $"{e.StationCode}&{recipeVer}&{DateTime.Now.ToString("yyyyMMdd_hhmmss")}&{partType}&{rfidBarCode}&File1&OK";

                List<double> validData = FilterScrewData(dataList);

                string dataContent = string.Join(",", validData);

                string imageFileName = fileName + ".png";
                string imagePath = filePath + imageFileName;

                var processData = new WipProcessData
                {
                    ProductModel = partType,
                    Receipe = recipeVer,
                    StationCode = e.StationCode,
                    BarCode = rfidBarCode,
                    BarCodeType = "拧紧",
                    CurveTypes = "拧紧",
                    SavePath = imagePath,
                    CreateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    CreateUser = "System",
                    DataValue = dataContent
                };

                await _wipProcessDataRepository.InsertAsync(processData);
                _logger.LogInformation($"拧紧数据保存成功: StationCode={e.StationCode}, 原始数据条数={dataList.Count}, 有效数据条数={validData.Count}");

                _ = Task.Run(() =>
                {
                    try
                    {
                        GenerateCurveImage(validData, imagePath, e.StationCode, recipeVer, partType, rfidBarCode);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "后台生成拧紧曲线图片失败");
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "保存拧紧数据失败");
            }
        }

        private List<double> FilterScrewData(List<double> dataList)
        {
            if (dataList == null || dataList.Count == 0)
                return new List<double>();

            List<double> validData = new List<double>();
            validData.Add(dataList[0]);

            for (int i = 1; i < dataList.Count; i++)
            {
                double prev = validData[validData.Count - 1];
                double current = dataList[i];
                double diff = Math.Abs(current - prev);

                if (diff <= 1)
                {
                    validData.Add(current);
                }
                else if (diff > 5)
                {
                    _logger.LogDebug($"过滤拧紧数据: 索引{i}, 前值={prev:F2}, 当前值={current:F2}, 差值={diff:F2} > 5, 已舍弃");
                }
                else
                {
                    _logger.LogDebug($"过滤拧紧数据: 索引{i}, 前值={prev:F2}, 当前值={current:F2}, 差值={diff:F2} > 1, 已舍弃");
                }
            }

            return validData;
        }

        private void GenerateCurveImage(List<double> dataList, string imagePath, string stationCode, string recipe, string partType, string barCode)
        {
            if (dataList == null || dataList.Count < 2)
            {
                _logger.LogWarning("有效数据不足，无法生成拧紧曲线图片");
                return;
            }

            try
            {
                string directory = Path.GetDirectoryName(imagePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                int width = 1200;
                int height = 600;
                int marginLeft = 80;
                int marginRight = 40;
                int marginTop = 60;
                int marginBottom = 60;
                int chartWidth = width - marginLeft - marginRight;
                int chartHeight = height - marginTop - marginBottom;

                using (Bitmap bitmap = new Bitmap(width, height))
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

                    g.Clear(Color.White);

                    string title = $"拧紧曲线 - 工站:{stationCode} 程序:{recipe} 型号:{partType}";
                    using (Font titleFont = new Font("微软雅黑", 14, FontStyle.Bold))
                    {
                        g.DrawString(title, titleFont, Brushes.Black, new PointF(marginLeft, 10));
                    }

                    double minVal = dataList.Min();
                    double maxVal = dataList.Max();
                    double valRange = maxVal - minVal;
                    if (valRange < 1) valRange = 1;

                    double yMin = minVal - valRange * 0.1;
                    double yMax = maxVal + valRange * 0.1;
                    double yRange = yMax - yMin;

                    using (Pen gridPen = new Pen(Color.LightGray, 1))
                    using (Font axisFont = new Font("微软雅黑", 9))
                    {
                        int yTickCount = 5;
                        for (int i = 0; i <= yTickCount; i++)
                        {
                            double val = yMin + (yRange * i / yTickCount);
                            int y = marginTop + chartHeight - (int)(chartHeight * i / yTickCount);

                            g.DrawLine(gridPen, marginLeft, y, marginLeft + chartWidth, y);

                            string yLabel = val.ToString("F2");
                            SizeF labelSize = g.MeasureString(yLabel, axisFont);
                            g.DrawString(yLabel, axisFont, Brushes.Black, marginLeft - labelSize.Width - 5, y - labelSize.Height / 2);
                        }
                    }

                    using (Font axisFont = new Font("微软雅黑", 9))
                    {
                        int xTickCount = Math.Min(10, dataList.Count - 1);
                        for (int i = 0; i <= xTickCount; i++)
                        {
                            int dataIndex = i * (dataList.Count - 1) / xTickCount;
                            int x = marginLeft + (int)(chartWidth * dataIndex / (dataList.Count - 1));

                            string xLabel = dataIndex.ToString();
                            SizeF labelSize = g.MeasureString(xLabel, axisFont);
                            g.DrawString(xLabel, axisFont, Brushes.Black, x - labelSize.Width / 2, marginTop + chartHeight + 5);
                        }
                    }

                    using (Pen axisPen = new Pen(Color.Black, 1.5f))
                    {
                        g.DrawLine(axisPen, marginLeft, marginTop, marginLeft, marginTop + chartHeight);
                        g.DrawLine(axisPen, marginLeft, marginTop + chartHeight, marginLeft + chartWidth, marginTop + chartHeight);
                    }

                    using (Pen linePen = new Pen(Color.Green, 2f))
                    {
                        Point[] points = new Point[dataList.Count];
                        for (int i = 0; i < dataList.Count; i++)
                        {
                            int x = marginLeft + (int)(chartWidth * i / (dataList.Count - 1));
                            int y = marginTop + chartHeight - (int)(chartHeight * (dataList[i] - yMin) / yRange);
                            points[i] = new Point(x, y);
                        }
                        g.DrawLines(linePen, points);
                    }

                    using (Brush pointBrush = new SolidBrush(Color.Red))
                    {
                        for (int i = 0; i < dataList.Count; i++)
                        {
                            int x = marginLeft + (int)(chartWidth * i / (dataList.Count - 1));
                            int y = marginTop + chartHeight - (int)(chartHeight * (dataList[i] - yMin) / yRange);
                            g.FillEllipse(pointBrush, x - 3, y - 3, 6, 6);
                        }
                    }

                    bitmap.Save(imagePath, ImageFormat.Png);
                }

                _logger.LogInformation($"拧紧曲线图片生成成功: {imagePath}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"生成拧紧曲线图片失败: {imagePath}");
            }
        }
    }
}