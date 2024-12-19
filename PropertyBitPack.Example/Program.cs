// See https://aka.ms/new-console-template for more information
using PropertyBitPack.Example;
using System.Drawing;

Console.WriteLine("Hello, World!");

var simpleStruct = new SimpleStruct();
var packedStruct = new PackedStruct();

Console.WriteLine($"SimpleStruct size: {System.Runtime.InteropServices.Marshal.SizeOf<SimpleStruct>()}");
Console.WriteLine($"PackedStruct size: {System.Runtime.InteropServices.Marshal.SizeOf<PackedStruct>()}");