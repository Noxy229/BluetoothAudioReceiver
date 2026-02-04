using System;
using System.Linq;

namespace TestSanitization
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Running Sanitization Tests...");
            bool allPassed = true;

            allPassed &= Test("SimpleName", "SimpleName");
            allPassed &= Test("   TrimMe   ", "TrimMe");
            allPassed &= Test(null, "Unknown Device");
            allPassed &= Test("", "Unknown Device");
            allPassed &= Test("   ", "Unknown Device");
            allPassed &= Test("Normal\tName\nWith\rControls", "NormalNameWithControls");

            // Test Length Limit (100 chars)
            string longName = new string('a', 150);
            string expectedLong = new string('a', 100);
            allPassed &= Test(longName, expectedLong);

            // Test Empty after sanitization
            allPassed &= Test("\t\n\r", "Unknown Device");

            if (allPassed)
            {
                Console.WriteLine("All tests passed!");
                Environment.Exit(0);
            }
            else
            {
                Console.WriteLine("Some tests failed!");
                Environment.Exit(1);
            }
        }

        static bool Test(string input, string expected)
        {
            string result = SanitizeDeviceName(input);
            if (result != expected)
            {
                Console.WriteLine($"FAILED: Input='{input}' Expected='{expected}' Got='{result}'");
                return false;
            }
            Console.WriteLine($"PASSED: Input='{input}'");
            return true;
        }

        // The Logic to be implemented in BluetoothService.cs
        static string SanitizeDeviceName(string? name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return "Unknown Device";
            }

            // Remove control characters
            string sanitized = new string(name.Where(c => !char.IsControl(c)).ToArray());

            // Trim
            sanitized = sanitized.Trim();

            // Check length
            if (sanitized.Length > 100)
            {
                sanitized = sanitized.Substring(0, 100);
            }

            // Check if empty after sanitization
            if (string.IsNullOrWhiteSpace(sanitized))
            {
                return "Unknown Device";
            }

            return sanitized;
        }
    }
}
