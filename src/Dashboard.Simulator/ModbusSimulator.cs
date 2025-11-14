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
    private DateTime _startTime = DateTime.UtcNow;
    
    private double _tempVelocity = 0.0;
    private double _pressVelocity = 0.0;
    private double _steamTrend = 0;
    private double _bitumenTrend = 0;
    private double _reservoirPressure = 4200.0;
    private double _steamGenPressure = 240.0;
    private double _fuelGasFlow = 125.0;
    
    private readonly double[] _freqComponents = { 0.001, 0.002, 0.003, 0.005, 0.008, 0.013, 0.021, 0.034 };
    private readonly double[] _phaseOffsets;
    
    private readonly Dictionary<string, double> _processStates = new();
    private readonly Dictionary<string, double> _processVelocities = new();
    private readonly Dictionary<string, double> _processNoise = new();

    public ModbusSimulator()
    {
        _startTime = DateTime.UtcNow;
        _phaseOffsets = new double[_freqComponents.Length];
        for (int i = 0; i < _freqComponents.Length; i++)
        {
            _phaseOffsets[i] = _random.NextDouble() * Math.PI * 2;
        }
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
        
        _holdingRegisters[20] = 4200.0f;
        _holdingRegisters[21] = 98.5f;
        _holdingRegisters[22] = 125.0f;
        _holdingRegisters[23] = 850.0f;
        _holdingRegisters[24] = 4500.0f;
        _holdingRegisters[25] = 68.0f;
        _holdingRegisters[26] = 1200.0f;
        _holdingRegisters[27] = 92.3f;
        _holdingRegisters[28] = 3.2f;
        _holdingRegisters[29] = 95.0f;
        _holdingRegisters[30] = 2400.0f;
        _holdingRegisters[31] = 45.0f;
        _holdingRegisters[32] = 1800.0f;
        _holdingRegisters[33] = 125.0f;
        _holdingRegisters[34] = 350.0f;
        
        _holdingRegisters[200] = 215.0f;
        _holdingRegisters[201] = 245.0f;
        _holdingRegisters[202] = 68.0f;
        _holdingRegisters[203] = 4500.0f;
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
                    var elapsedSeconds = (DateTime.UtcNow - _startTime).TotalSeconds;
                    
                    double GetMultiFreqVariation(double baseFreq, double amplitude)
                    {
                        double variation = 0.0;
                        for (int i = 0; i < _freqComponents.Length; i++)
                        {
                            var freq = _freqComponents[i] * baseFreq;
                            variation += Math.Sin(elapsedSeconds * freq + _phaseOffsets[i]) * (amplitude / (_freqComponents.Length - i + 1));
                        }
                        return variation;
                    }
                    
                    double GetRandomWalk(string key, double maxDrift, double damping = 0.95)
                    {
                        if (!_processStates.ContainsKey(key))
                        {
                            _processStates[key] = 0.0;
                            _processVelocities[key] = 0.0;
                        }
                        
                        var acceleration = (_random.NextDouble() - 0.5) * maxDrift * 0.1;
                        _processVelocities[key] = _processVelocities[key] * damping + acceleration;
                        _processStates[key] += _processVelocities[key];
                        
                        _processStates[key] = Math.Clamp(_processStates[key], -maxDrift, maxDrift);
                        
                        return _processStates[key];
                    }
                    
                    double GetRealisticNoise(double stdDev)
                    {
                        if (!_processNoise.ContainsKey("noise_u"))
                        {
                            _processNoise["noise_u"] = _random.NextDouble();
                            _processNoise["noise_z"] = Math.Sqrt(-2.0 * Math.Log(_processNoise["noise_u"])) * 
                                                       Math.Cos(2.0 * Math.PI * _random.NextDouble()) * stdDev;
                        }
                        else
                        {
                            _processNoise["noise_u"] = _random.NextDouble();
                            _processNoise["noise_z"] = Math.Sqrt(-2.0 * Math.Log(_processNoise["noise_u"])) * 
                                                       Math.Cos(2.0 * Math.PI * _random.NextDouble()) * stdDev;
                        }
                        return _processNoise["noise_z"];
                    }
                    
                    var tempSP = _holdingRegisters[100];
                    var temp = _holdingRegisters[0];
                    
                    var tempMultiFreq = GetMultiFreqVariation(0.1, 8.0);
                    var tempWalk = GetRandomWalk("temp", 12.0, 0.92);
                    var tempNoise = GetRealisticNoise(1.5);
                    
                    var tempError = tempSP - temp;
                    _tempVelocity = _tempVelocity * 0.85 + tempError * 0.08;
                    var tempDelta = _tempVelocity + tempMultiFreq + tempWalk + tempNoise;
                    
                    _holdingRegisters[0] = (float)Math.Clamp(temp + tempDelta, 0f, 200f);

                    var pressSP = _holdingRegisters[101];
                    var press = _holdingRegisters[1];
                    
                    var pressMultiFreq = GetMultiFreqVariation(0.15, 3.5);
                    var pressWalk = GetRandomWalk("press", 6.0, 0.90);
                    var pressNoise = GetRealisticNoise(0.8);
                    
                    var pressError = pressSP - press;
                    _pressVelocity = _pressVelocity * 0.88 + pressError * 0.06;
                    var pressDelta = _pressVelocity + pressMultiFreq + pressWalk + pressNoise;
                    
                    _holdingRegisters[1] = (float)Math.Clamp(press + pressDelta, 0f, 50f);

                var flowSP = _holdingRegisters[102];
                var flow = _holdingRegisters[2];
                var flowMultiFreq = GetMultiFreqVariation(0.12, 12.0);
                var flowWalk = GetRandomWalk("flow", 25.0, 0.91);
                var flowNoise = GetRealisticNoise(2.5);
                var flowError = flowSP - flow;
                        if (!_processVelocities.ContainsKey("flow")) _processVelocities["flow"] = 0.0;
                _processVelocities["flow"] = _processVelocities["flow"] * 0.87 + flowError * 0.10;
                var flowDelta = _processVelocities["flow"] + flowMultiFreq + flowWalk + flowNoise;
                _holdingRegisters[2] = (float)Math.Clamp(flow + flowDelta, 0, 500);

                var levelSP = _holdingRegisters[103];
                var level = _holdingRegisters[3];
                var levelMultiFreq = GetMultiFreqVariation(0.08, 2.5);
                var levelWalk = GetRandomWalk("level", 8.0, 0.94);
                var levelNoise = GetRealisticNoise(0.7);
                var levelError = levelSP - level;
                        if (!_processVelocities.ContainsKey("level")) _processVelocities["level"] = 0.0;
                _processVelocities["level"] = _processVelocities["level"] * 0.92 + levelError * 0.04;
                var levelDelta = _processVelocities["level"] + levelMultiFreq + levelWalk + levelNoise;
                _holdingRegisters[3] = (float)Math.Clamp(level + levelDelta, 0, 100);

                _steamTrend = GetRandomWalk("steam_trend", 400.0, 0.93);
                var steamBase = 6495.0;
                var steamMultiFreq = GetMultiFreqVariation(0.05, 180.0);
                var steamNoise = GetRealisticNoise(45.0);
                var steamValue = steamBase + steamMultiFreq + _steamTrend + steamNoise;
                _holdingRegisters[10] = (float)Math.Clamp(steamValue, 5000, 8000);

                _bitumenTrend = GetRandomWalk("bitumen_trend", 150.0, 0.92);
                var bitumenBase = 2150.0;
                var steamInfluence = (_holdingRegisters[10] - 6495.0) * -0.12;
                var bitumenMultiFreq = GetMultiFreqVariation(0.05, 65.0);
                var bitumenNoise = GetRealisticNoise(18.0);
                var bitumenValue = bitumenBase + steamInfluence + bitumenMultiFreq + _bitumenTrend + bitumenNoise;
                _holdingRegisters[12] = (float)Math.Clamp(bitumenValue, 1500, 2800);

                var steamQualityMultiFreq = GetMultiFreqVariation(0.08, 220.0);
                var steamQualityNoise = GetRealisticNoise(85.0);
                var steamQuality = 17000 + steamQualityMultiFreq + steamQualityNoise;
                _holdingRegisters[11] = (float)Math.Clamp(steamQuality, 16000, 18000);

                var steamPressCorrelation = (_holdingRegisters[10] - 6495.0) * 0.012;
                var steamPressMultiFreq = GetMultiFreqVariation(0.1, 3.5);
                var steamPressNoise = GetRealisticNoise(2.2);
                var steamPressure = 240 + steamPressCorrelation + steamPressMultiFreq + steamPressNoise;
                _holdingRegisters[13] = (float)Math.Clamp(steamPressure, 220, 260);

                var reservoirTemp = _holdingRegisters[14];
                var resTempInfluence = (_holdingRegisters[10] - 6495.0) * 0.0015;
                var resTempMultiFreq = GetMultiFreqVariation(0.02, 1.2);
                var resTempNoise = GetRealisticNoise(0.25);
                var tempChange = resTempInfluence + resTempMultiFreq + resTempNoise;
                _holdingRegisters[14] = (float)Math.Clamp(reservoirTemp + tempChange, 200, 230);

                var espBase = _random.NextDouble() < 0.98 ? 98.5 : 75.0;
                var espMultiFreq = GetMultiFreqVariation(0.15, 1.2);
                var espNoise = GetRealisticNoise(0.6);
                var espStatus = espBase + espMultiFreq + espNoise;
                _holdingRegisters[15] = (float)Math.Clamp(espStatus, 0, 100);

                var wellheadBase = 45.0;
                var wellheadInfluence = (_holdingRegisters[12] - 2150.0) * 0.002;
                var wellheadMultiFreq = GetMultiFreqVariation(0.1, 2.2);
                var wellheadNoise = GetRealisticNoise(0.9);
                var wellheadTemp = wellheadBase + wellheadInfluence + wellheadMultiFreq + wellheadNoise;
                _holdingRegisters[16] = (float)Math.Clamp(wellheadTemp, 35, 55);

                var waterCut = _holdingRegisters[18] / 100;
                var bitumenProd = _holdingRegisters[12];
                var totalLiquid = bitumenProd / (1 - waterCut);
                _holdingRegisters[17] = (float)Math.Clamp(totalLiquid, 1600, 2200);

                var waterCutChange = (_random.NextDouble() - 0.5) * 0.2;
                _holdingRegisters[18] = (float)Math.Clamp(_holdingRegisters[18] + waterCutChange, 10, 25);

                var currentSOR = _holdingRegisters[12] > 0 ? _holdingRegisters[10] / _holdingRegisters[12] : 3.2;
                _holdingRegisters[28] = (float)Math.Clamp(currentSOR, 2.5, 4.5);

                var resPressInfluence = (_holdingRegisters[10] / 1000.0 - _holdingRegisters[12] / 100.0) * 0.5;
                var resPressMultiFreq = GetMultiFreqVariation(0.03, 35.0);
                var resPressNoise = GetRealisticNoise(12.0);
                _reservoirPressure += resPressInfluence + resPressMultiFreq + resPressNoise;
                _reservoirPressure = Math.Clamp(_reservoirPressure, 3800, 4800);
                _holdingRegisters[20] = (float)_reservoirPressure;

                var steamGenEffBase = 95.0;
                var steamGenEffInfluence = (_holdingRegisters[23] - 850) * 0.01 + (_holdingRegisters[22] - 125) * 0.05;
                var steamGenEffMultiFreq = GetMultiFreqVariation(0.12, 0.8);
                var steamGenEffNoise = GetRealisticNoise(0.6);
                var steamGenEff = steamGenEffBase + steamGenEffInfluence + steamGenEffMultiFreq + steamGenEffNoise;
                _holdingRegisters[21] = (float)Math.Clamp(steamGenEff, 90, 100);

                var fuelGasTarget = _holdingRegisters[10] / 100.0;
                _fuelGasFlow += (fuelGasTarget - _fuelGasFlow) * 0.1;
                var fuelGasMultiFreq = GetMultiFreqVariation(0.1, 1.8);
                var fuelGasNoise = GetRealisticNoise(1.0);
                _fuelGasFlow += fuelGasMultiFreq + fuelGasNoise;
                _fuelGasFlow = Math.Clamp(_fuelGasFlow, 100, 150);
                _holdingRegisters[22] = (float)_fuelGasFlow;

                var feedwaterBase = 840.0;
                var feedwaterInfluence = (_holdingRegisters[13] - 240) * 0.2;
                var feedwaterMultiFreq = GetMultiFreqVariation(0.08, 6.5);
                var feedwaterNoise = GetRealisticNoise(4.5);
                var feedwaterTemp = feedwaterBase + feedwaterInfluence + feedwaterMultiFreq + feedwaterNoise;
                _holdingRegisters[23] = (float)Math.Clamp(feedwaterTemp, 820, 880);

                var sepPressBase = 4400.0;
                var sepPressInfluence = (_holdingRegisters[17] / 10.0 - 185) * 2.0;
                var sepPressMultiFreq = GetMultiFreqVariation(0.07, 28.0);
                var sepPressNoise = GetRealisticNoise(22.0);
                var sepPress = sepPressBase + sepPressInfluence + sepPressMultiFreq + sepPressNoise;
                _holdingRegisters[24] = (float)Math.Clamp(sepPress, 4200, 4700);

                var sepTemp = _holdingRegisters[25];
                var sepTempSP = _holdingRegisters[202];
                var sepTempError = sepTempSP - sepTemp;
                        if (!_processVelocities.ContainsKey("sepTemp")) _processVelocities["sepTemp"] = 0.0;
                _processVelocities["sepTemp"] = _processVelocities["sepTemp"] * 0.90 + sepTempError * 0.08;
                var sepTempMultiFreq = GetMultiFreqVariation(0.1, 0.9);
                var sepTempNoise = GetRealisticNoise(0.7);
                var sepTempDelta = _processVelocities["sepTemp"] + sepTempMultiFreq + sepTempNoise;
                _holdingRegisters[25] = (float)Math.Clamp(sepTemp + sepTempDelta, 60, 75);

                var gasFlowBase = _holdingRegisters[12] * 0.55;
                var gasFlowMultiFreq = GetMultiFreqVariation(0.09, 28.0);
                var gasFlowNoise = GetRealisticNoise(22.0);
                var gasFlow = gasFlowBase + gasFlowMultiFreq + gasFlowNoise;
                _holdingRegisters[26] = (float)Math.Clamp(gasFlow, 1000, 1400);

                var dehydEffBase = 94.0;
                var dehydEffInfluence = -(_holdingRegisters[26] - 1200) / 100.0;
                var dehydEffMultiFreq = GetMultiFreqVariation(0.11, 0.6);
                var dehydEffNoise = GetRealisticNoise(0.5);
                var dehydEff = dehydEffBase + dehydEffInfluence + dehydEffMultiFreq + dehydEffNoise;
                _holdingRegisters[27] = (float)Math.Clamp(dehydEff, 90, 98);

                var waterTreatEffBase = 96.0;
                var waterTreatEffMultiFreq = GetMultiFreqVariation(0.13, 1.1);
                var waterTreatEffNoise = GetRealisticNoise(0.9);
                var waterTreatEff = waterTreatEffBase + waterTreatEffMultiFreq + waterTreatEffNoise;
                _holdingRegisters[29] = (float)Math.Clamp(waterTreatEff, 93, 98);

                var pipelinePressBase = 2350.0;
                var pipelinePressInfluence = (_holdingRegisters[17] / 10.0 - 185) * 1.5;
                var pipelinePressMultiFreq = GetMultiFreqVariation(0.08, 18.0);
                var pipelinePressNoise = GetRealisticNoise(14.0);
                var pipelinePress = pipelinePressBase + pipelinePressInfluence + pipelinePressMultiFreq + pipelinePressNoise;
                _holdingRegisters[30] = (float)Math.Clamp(pipelinePress, 2200, 2600);

                var pumpStatusBase = 94.0;
                var pumpStatusInfluence = (2500 - _holdingRegisters[30]) / 20.0;
                var pumpStatusMultiFreq = GetMultiFreqVariation(0.14, 1.2);
                var pumpStatusNoise = GetRealisticNoise(0.9);
                var pumpStatus = pumpStatusBase + pumpStatusInfluence + pumpStatusMultiFreq + pumpStatusNoise;
                _holdingRegisters[31] = (float)Math.Clamp(pumpStatus, 85, 100);

                var flashPressBase = _holdingRegisters[24] * 0.4;
                var flashPressMultiFreq = GetMultiFreqVariation(0.09, 28.0);
                var flashPressNoise = GetRealisticNoise(22.0);
                var flashPress = flashPressBase + flashPressMultiFreq + flashPressNoise;
                _holdingRegisters[32] = (float)Math.Clamp(flashPress, 1600, 2000);

                var flashTempBase = _holdingRegisters[25] * 1.8;
                var flashTempMultiFreq = GetMultiFreqVariation(0.1, 1.8);
                var flashTempNoise = GetRealisticNoise(1.4);
                var flashTemp = flashTempBase + flashTempMultiFreq + flashTempNoise;
                _holdingRegisters[33] = (float)Math.Clamp(flashTemp, 120, 135);

                var waterInjBase = 320.0;
                var waterInjInfluence = (_holdingRegisters[12] / 6.0 - 350) * 0.1;
                var waterInjMultiFreq = GetMultiFreqVariation(0.1, 9.0);
                var waterInjNoise = GetRealisticNoise(7.0);
                var waterInj = waterInjBase + waterInjInfluence + waterInjMultiFreq + waterInjNoise;
                _holdingRegisters[34] = (float)Math.Clamp(waterInj, 300, 400);

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


