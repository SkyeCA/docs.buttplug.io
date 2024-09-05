﻿using Buttplug.Client;

namespace DeviceControlExample
{
    class Program
    {
        private static async Task WaitForKey()
        {
            Console.WriteLine("Press any key to continue.");
            while (!Console.KeyAvailable)
            {
                await Task.Delay(1);
            }
            Console.ReadKey(true);
        }

        private static async Task RunExample()
        {
            var client = new ButtplugClient("Example Client");
            var connector = new ButtplugWebsocketConnector(new Uri("ws://127.0.0.1:12345"));

            try
            {
                await client.ConnectAsync(connector);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Can't connect, exiting!");
                Console.WriteLine($"Message: {ex.InnerException?.Message}");
                await WaitForKey();
                return;
            }
            Console.WriteLine("Connected!");
            // You usually shouldn't run Start/Stop scanning back-to-back like
            // this, but with TestDevice we know our device will be found when we
            // call StartScanning, so we can get away with it.
            await client.StartScanningAsync();
            await client.StopScanningAsync();
            Console.WriteLine("Client currently knows about these devices:");
            foreach (var device in client.Devices)
            {
                Console.WriteLine($"- {device.Name}");
            }

            await WaitForKey();

            foreach (var device in client.Devices)
            {
                Console.WriteLine($"{device.Name} supports vibration: ${device.VibrateAttributes.Count > 0}");
                if (device.VibrateAttributes.Count > 0)
                {
                   Console.WriteLine($" - Number of Vibrators: {device.VibrateAttributes.Count}");
                }
            }

            Console.WriteLine("Sending commands");

            // Now that we know the message types for our connected device, we
            // can send a message over! Seeing as we want to stick with the
            // modern generic messages, we'll go with VibrateCmd.
            //
            // There's a couple of ways to send this message.
            var testClientDevice = client.Devices[0];


            // We can use the convenience functions on ButtplugClientDevice to
            // send the message. This version sets all of the motors on a
            // vibrating device to the same speed.
            await testClientDevice.VibrateAsync(1.0);

            // If we wanted to just set one motor on and the other off, we could
            // try this version that uses an array. It'll throw an exception if
            // the array isn't the same size as the number of motors available as
            // denoted by FeatureCount, though.
            //
            // You can get the vibrator count using the following code, though we
            // know it's 2 so we don't really have to use it.
            //
            // This vibrateType variable is just used to keep us under 80 
            // characters for the dev guide, so don't feel that you have to reassign 
            // types like this. I'm just trying to make it so you don't have to
            // horizontally scroll in the manual. :)
            var vibratorCount = testClientDevice.VibrateAttributes.Count;
            await testClientDevice.VibrateAsync(new[] { 1.0, 0.0 });

            await WaitForKey();

            // And now we disconnect as usual.
            await client.DisconnectAsync();

            // If we try to send a command to a device after the client has
            // disconnected, we'll get an exception thrown.
            try
            {
                await testClientDevice.VibrateAsync(1.0);
            }
            catch (ButtplugClientConnectorException e)
            {
                Console.WriteLine("Tried to send after disconnection! Exception: ");
                Console.WriteLine(e);
            }
            await WaitForKey();
        }

        private static void Main()
        {
            // Setup a client, and wait until everything is done before exiting.
            RunExample().Wait();
        }
    }
}
