// ModbusSimulator.cs - Modbus TCP simulator with process variable simulation
using System.Net;
using System.Net.Sockets;

namespace Dashboard.Simulator;

public class ModbusSimulator
{
    private readonly Dictionary<int, float> _holdingRegisters = new();
    private readonly Random _random = new();
    private readonly CancellationTokenSource _cts = new();
    private Task? _simulationTask;
    private double _timeStep = 0;
    private double _steamTrend = 0;
    private double _bitumenTrend = 0;

    public ModbusSimulator()
    {
        InitializeRegisters();
    }

    private void InitializeRegisters()
    {
        _holdingRegisters[0] = 75.0f;
        _holdingRegisters[1] = 25.0f;
        _holdingRegisters[2] = 250.0f;
        _holdingRegisters[100] = 80.0f;
        _holdingRegisters[101] = 30.0f;
        _holdingRegisters[102] = 300.0f;
        _holdingRegisters[3] = 50.0f;
        _holdingRegisters[103] = 60.0f;
        _holdingRegisters[10] = 6500.0f;
        _holdingRegisters[11] = 17000.0f;
        _holdingRegisters[12] = 2100.0f;
        _holdingRegisters[13] = 245.0f;
        _holdingRegisters[14] = 215.0f;
        _holdingRegisters[15] = 98.5f;
        _holdingRegisters[16] = 45.0f;
        _holdingRegisters[17] = 1850.0f;
        _holdingRegisters[18] = 15.2f;
    }

    public void Start()
    {
        _simulationTask = Task.Run(async () => await SimulateProcessVariables(_cts.Token));
    }

    public void Stop()
    {
        _cts.Cancel();
        _simulationTask?.Wait(TimeSpan.FromSeconds(5));
    }

    private async Task SimulateProcessVariables(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                _timeStep += 0.001;
                var tempSP = _holdingRegisters[100];
                var temp = _holdingRegisters[0];
                var tempDelta = (tempSP - temp) * 0.1f + ((float)_random.NextDouble() - 0.5f) * 2.0f;
                _holdingRegisters[0] = Math.Clamp(temp + tempDelta, 0, 200);

                var pressSP = _holdingRegisters[101];
                var press = _holdingRegisters[1];
                var pressDelta = (pressSP - press) * 0.08f + ((float)_random.NextDouble() - 0.5f) * 1.0f;
                _holdingRegisters[1] = Math.Clamp(press + pressDelta, 0, 50);

                var flowSP = _holdingRegisters[102];
                var flow = _holdingRegisters[2];
                var flowDelta = (flowSP - flow) * 0.12f + ((float)_random.NextDouble() - 0.5f) * 5.0f;
                _holdingRegisters[2] = Math.Clamp(flow + flowDelta, 0, 500);

                var levelSP = _holdingRegisters[103];
                var level = _holdingRegisters[3];
                var levelDelta = (levelSP - level) * 0.05f + ((float)_random.NextDouble() - 0.5f) * 1.5f;
                _holdingRegisters[3] = Math.Clamp(level + levelDelta, 0, 100);

                _steamTrend += (_random.NextDouble() - 0.5) * 50;
                _steamTrend = Math.Clamp(_steamTrend, -500, 500);
                var steamBase = 6495.0;
                var steamSineWave = Math.Sin(_timeStep * 2.0) * 300;
                var steamNoise = (_random.NextDouble() - 0.5) * 150;
                var steamValue = steamBase + steamSineWave + _steamTrend + steamNoise;
                _holdingRegisters[10] = (float)Math.Clamp(steamValue, 5000, 8000);

                _bitumenTrend += (_random.NextDouble() - 0.5) * 30;
                _bitumenTrend = Math.Clamp(_bitumenTrend, -200, 200);
                var bitumenBase = 2150.0;
                var bitumenSineWave = Math.Sin(_timeStep * 2.0 + Math.PI) * 100;
                var bitumenNoise = (_random.NextDouble() - 0.5) * 50;
                var bitumenValue = bitumenBase + bitumenSineWave + _bitumenTrend + bitumenNoise;
                _holdingRegisters[12] = (float)Math.Clamp(bitumenValue, 1500, 2800);

                var steamQuality = 17000 + Math.Sin(_timeStep * 10) * 300 + (_random.NextDouble() - 0.5) * 200;
                _holdingRegisters[11] = (float)Math.Clamp(steamQuality, 16000, 18000);

                var steamPressure = 240 + (_holdingRegisters[10] - 6495) * 0.01 + (_random.NextDouble() - 0.5) * 5;
                _holdingRegisters[13] = (float)Math.Clamp(steamPressure, 220, 260);

                var reservoirTemp = _holdingRegisters[14];
                var tempChange = (_random.NextDouble() - 0.5) * 0.5;
                _holdingRegisters[14] = (float)Math.Clamp(reservoirTemp + tempChange, 200, 230);

                var espStatus = _random.NextDouble() < 0.98 ? 98.5f + (float)(_random.NextDouble() - 0.5) * 1.5 
                                                             : 75.0f + (float)_random.NextDouble() * 15;
                _holdingRegisters[15] = (float)Math.Clamp(espStatus, 0, 100);

                var wellheadTemp = 45 + Math.Sin(_timeStep * 1.5) * 3 + (_random.NextDouble() - 0.5) * 2;
                _holdingRegisters[16] = (float)Math.Clamp(wellheadTemp, 35, 55);

                var waterCut = _holdingRegisters[18] / 100;
                var bitumenProd = _holdingRegisters[12];
                var totalLiquid = bitumenProd / (1 - waterCut);
                _holdingRegisters[17] = (float)Math.Clamp(totalLiquid, 1600, 2200);

                var waterCutChange = (_random.NextDouble() - 0.5) * 0.2;
                _holdingRegisters[18] = (float)Math.Clamp(_holdingRegisters[18] + waterCutChange, 10, 25);

                await Task.Delay(1000, ct);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    public float ReadHoldingRegister(int address)
    {
        return _holdingRegisters.TryGetValue(address, out var value) ? value : 0f;
    }

    public void WriteHoldingRegister(int address, float value)
    {
        _holdingRegisters[address] = value;
    }

    public Dictionary<int, float> GetAllRegisters()
    {
        return new Dictionary<int, float>(_holdingRegisters);
    }
}


