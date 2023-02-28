using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Client.Exceptions;
using System.Text;
using System.IO.Ports;

string IotHubUri = "light.azure-devices.net";
string DeviceKey = "+t1Wj0lXlIORBsKt3I5uA97xunEK9kCy6kre1uAQCZE=";
string DeviceId = "deviceA";

foreach (var str in SerialPort.GetPortNames())
{
	Console.WriteLine(str);
}
SerialPort serialPort = new SerialPort("COM8");

if (!serialPort.IsOpen)
{
	serialPort.BaudRate = 9600;
    serialPort.Parity = Parity.None;
    serialPort.StopBits = StopBits.One;
    serialPort.DataBits = 8;
    serialPort.Handshake = Handshake.None;
    serialPort.RtsEnable = true;
	serialPort.Encoding = Encoding.ASCII;
	serialPort.DataReceived += DataReceivedHandler;

    serialPort.Open();
}

void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
{
    Console.WriteLine(e.ToString());
}

CancellationToken _ct;


Console.WriteLine("------------------------------");
Console.WriteLine(" Azure IoT Hub Light Control");
Console.WriteLine("------------------------------");



var cts = new CancellationTokenSource();
_ct = cts.Token;

Console.CancelKeyPress += (sender, eventArgs) =>
{
	Console.WriteLine($"{DateTime.Now} > Cancelling...");
	cts.Cancel();

	eventArgs.Cancel = true;
};

try
{
	var t = Task.Run(Run, cts.Token);
	await t;
}
catch (IotHubCommunicationException)
{
	Console.WriteLine($"{DateTime.Now} > Operation has been canceled.");
}
catch (OperationCanceledException)
{
	Console.WriteLine($"{DateTime.Now} > Operation has been canceled.");
}
finally
{
	cts.Dispose();
}

Console.ReadKey();


async Task Run()
{
	using var deviceClient = DeviceClient.Create(IotHubUri, new DeviceAuthenticationWithRegistrySymmetricKey(DeviceId, DeviceKey));

	Console.WriteLine($"{DateTime.Now} > Connected to the best cloud on the planet.");
	Console.WriteLine($"Azure IoT Hub: {IotHubUri}");
	Console.WriteLine($"Device ID: {DeviceId}");
	while (!_ct.IsCancellationRequested)
	{
		Console.WriteLine($"{DateTime.Now} > Waiting new message from Azure...");
		var receivedMessage = await deviceClient.ReceiveAsync(_ct);
		if (receivedMessage == null) continue;

		var msg = Encoding.ASCII.GetString(receivedMessage.GetBytes());
		Console.WriteLine($"{DateTime.Now} > Received message: {msg}");

		if (int.TryParse(msg, out int res))
		{
			serialPort.WriteLine(msg);
		}
		await deviceClient.CompleteAsync(receivedMessage, _ct);

	}
}