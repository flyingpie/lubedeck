using HidSharp;
using System.Text;

namespace HidTest;

public static class Program
{
	public static async Task Main(string[] args)
	{
		Console.WriteLine("Hello, World!");

		// var dev = HidSharp.DeviceList.Local.GetAllDevices().ToList();
		// TODO(MvdO): Device has a name that I don't know whether it's deterministic.

		//var xdev = HidSharp.DeviceList.Local.GetAllDevices().ToList();


		var dev = HidSharp.DeviceList.Local.GetSerialDevices().FirstOrDefault();
		if (dev == null)
		{
			Console.WriteLine("No device found");
			return;
		}

		Console.WriteLine($"Got device {dev.DevicePath}. Friendly name: {dev.GetFriendlyName()}");

		if (!dev.CanOpen)
		{
			Console.WriteLine($"Can't open device '{dev.DevicePath}'");
		}

		// var x = dev.Open();

		if (!dev.TryOpen(new OpenConfiguration() { }, out var stream))
		{
			Console.WriteLine($"Could not open device {dev}");
		}

		using var x = stream;

		stream.BaudRate = 256_000;
		stream.ReadTimeout = 15_000;


		stream.Closed += (s, a) =>
		{
			Console.WriteLine($"CLOSED");
		};

		stream.InterruptRequested += (s, a) =>
		{
			Console.WriteLine($"INTERRUPT");
		};

		//     SET_VIBRATION: 0x1b,
		//     SHORT: 0x01,
		//		export const MAX_BRIGHTNESS = 10
		var ws = """
		         GET /index.html
		         HTTP/1.1
		         Connection: Upgrade
		         Upgrade: websocket
		         Sec-WebSocket-Key: 123abc


		         """;

		var wsBytes = Encoding.UTF8.GetBytes(ws);
		await stream.WriteAsync(wsBytes, 0, wsBytes.Length);
		await stream.FlushAsync();


		var tc = new TaskCompletionSource();

		Task.Run(async () =>
		{
			var xx1 = new MemoryStream();

			var xx2 = new MemoryStream();

			for (int i = 0; i < 1000; i++)
			{
				// Console.WriteLine("READ...");
				var bbb = stream.ReadByte();

				if (bbb == 0x82)
				{
					var len = stream.ReadByte();
					var buff = new byte[len];
					var read = stream.Read(buff, 0, buff.Length);

					if (read != len)
					{
						throw new InvalidOperationException("Oh noes!");
					}

					var len2 = buff[0];
					var cmd = buff[1];
					var tranId = buff[2];
					var data = buff[3..];

					Console.WriteLine($"CMD:{Convert.ToHexString(buff[0..1])} ({len}) {Convert.ToHexString(buff)}");
					Console.WriteLine($" - {Encoding.UTF8.GetString(buff[1..])}");
					Console.WriteLine($"LEN:{len2} CMD:{Convert.ToHexString([cmd])} TRANID:{tranId} DATA:{Convert.ToHexString(data)}");

					continue;
				}

				// Console.WriteLine($"BYTE: {bbb}");
				// xx1.WriteByte((byte)bbb);
				//
				// var str = Encoding.UTF8.GetString(xx1.ToArray());
				//
				// Console.WriteLine($"[{str.Length}] {str}");
				// Console.WriteLine($"[HEX] {Convert.ToHexString(xx1.ToArray())}");
				//
				// if (str.Length == 313)
				// {
				// 	tc.SetResult();
				// }
			}
		});

		// await tc.Task;
		Console.ReadLine();
		Console.WriteLine("VERSION");
		await GetVer(stream);

		Console.WriteLine("Done");
		Console.ReadLine();


		Console.WriteLine("Done");
		Console.ReadLine();

		stream.Close();
		stream.Dispose();
	}

	private static async Task GetVer(SerialStream stream)
	{
// 		byte[] writeBuffer = new byte[100];
//
// 		// Header
// 		writeBuffer[0] = 0x03; // Data length
// 		writeBuffer[1] = 0x03; // Command:SET_VIBRATION
// 		writeBuffer[2] = 0x01; // Transaction id
//
// 		// Data
// //		0x01, // HAPTIC_SHORT
//
// 		await stream.WriteAsync(writeBuffer, 0, writeBuffer.Length);
// 		await stream.FlushAsync();

		byte[] bSerial =
		[
			0x82,
			0x80 + 3,
			0x00,
			0x00,
			0x00,
			0x00,

			0x03, // Data length
			0x03, // Command:SET_VIBRATION
			0x01, // Transaction id
		];

		// Data
//		0x01, // HAPTIC_SHORT

		await stream.WriteAsync(bSerial, 0, bSerial.Length);
		await stream.FlushAsync();
		Console.WriteLine("SERIAL");


		byte[] bVersion =
		[
			0x82,
			0x80 + 3,
			0x00,
			0x00,
			0x00,
			0x00,

			0x03, // Data length
			0x07, // Command:SET_VIBRATION
			0x02, // Transaction id
		];

		// Data
//		0x01, // HAPTIC_SHORT

		await stream.WriteAsync(bVersion, 0, bVersion.Length);
		await stream.FlushAsync();
		Console.WriteLine("VERSION");


		while (true)
		{
			byte[] bSerial1 =
			[
				0x82,
				0x80 + 4,
				0x00,
				0x00,
				0x00,
				0x00,

				0x04, // Data length
				0x1b, // Command:SET_VIBRATION
				0x05, // Transaction id
				// 0x01, // HAPTIC_SHORT
				0x0f, // HAPTIC_LONG
			];

			// Data
//		0x01, // HAPTIC_SHORT

			await stream.WriteAsync(bSerial1, 0, bSerial1.Length);
			await stream.FlushAsync();
			Console.WriteLine("VIB");
			var readLine = Console.ReadLine();

			if (readLine == "exit")
			{
				break;
			}
		}
	}
}